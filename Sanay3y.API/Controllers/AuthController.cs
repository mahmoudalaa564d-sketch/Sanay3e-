
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;
using Sanay3y.API.Services;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string CurrentArtisanTermsVersion = "v1.0";

        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;
        private readonly PrivateFileStorageService _privateFileStorage;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ApplicationDbContext context,
            PasswordService passwordService,
            JwtService jwtService,
            PrivateFileStorageService privateFileStorage,
            ILogger<AuthController> logger)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _privateFileStorage = privateFileStorage;
            _logger = logger;
        }

        // Register a customer.
        [EnableRateLimiting("RegisterPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            RegisterRequest request)
        {
            var email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim().ToLowerInvariant();

            var phoneNumber =
                string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim();

            if (email is not null &&
                await _context.Users.AnyAsync(
                    u => u.Email == email))
            {
                return Conflict(new
                {
                    message = "This email is already registered."
                });
            }

            if (phoneNumber is not null &&
                await _context.Users.AnyAsync(
                    u => u.PhoneNumber == phoneNumber))
            {
                return Conflict(new
                {
                    message = "This phone number is already registered."
                });
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = _passwordService.HashPassword(
                    request.Password),
                Role = "Customer",
                AccountStatus = "PendingVerification",
                IsEmailVerified = false,
                IsPhoneVerified = false
            };

            _context.Users.Add(user);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (IsUniqueConstraintViolation(ex))
            {
                return Conflict(new
                {
                    message =
                        "An account with this email or phone number is already registered."
                });
            }

            return StatusCode(201, new
            {
                message =
                    "Registration created. Contact verification is required.",
                userId = user.Id,//ll
                role = user.Role,
                accountStatus = user.AccountStatus
            });
        }

        // Register an artisan using multipart/form-data.
        [EnableRateLimiting("RegisterPolicy")]
        [HttpPost("register-artisan")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(16 * 1024 * 1024)]
        [RequestFormLimits(
            MultipartBodyLengthLimit = 16 * 1024 * 1024)]
        public async Task<IActionResult> RegisterArtisan(
            [FromForm] ArtisanRegisterRequest request,
            CancellationToken cancellationToken)
        {
            var email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim().ToLowerInvariant();

            var phoneNumber =
                string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim();

            if (email is not null &&
                await _context.Users.AnyAsync(
                    u => u.Email == email,
                    cancellationToken))
            {
                return Conflict(new
                {
                    message = "This email is already registered."
                });
            }

            if (phoneNumber is not null &&
                await _context.Users.AnyAsync(
                    u => u.PhoneNumber == phoneNumber,
                    cancellationToken))
            {
                return Conflict(new
                {
                    message = "This phone number is already registered."
                });
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PhoneNumber = phoneNumber,
                PasswordHash = _passwordService.HashPassword(
                    request.Password),
                Role = "Artisan",
                AccountStatus = "PendingVerification",
                IsEmailVerified = false,
                IsPhoneVerified = false
            };

            var savedFileKeys = new List<string>();
            var databaseSaved = false;

            try
            {
                var profileImagePath =
                    await _privateFileStorage.SaveImageAsync(
                        request.ProfileImage!,
                        "profiles",
                        cancellationToken);

                savedFileKeys.Add(profileImagePath);

                var nationalIdFrontPath =
                    await _privateFileStorage.SaveImageAsync(
                        request.NationalIdFrontImage!,
                        "identity",
                        cancellationToken);

                savedFileKeys.Add(nationalIdFrontPath);

                var nationalIdBackPath =
                    await _privateFileStorage.SaveImageAsync(
                        request.NationalIdBackImage!,
                        "identity",
                        cancellationToken);

                savedFileKeys.Add(nationalIdBackPath);

                var artisanProfile = new ArtisanProfile
                {
                    User = user,
                    Profession = request.Profession.Trim(),
                    ExperienceYears = request.ExperienceYears,

                    // Retain the legacy field for compatibility.
                    ServiceArea = request.Center.Trim(),

                    Bio = request.Bio.Trim(),
                    ProfileImagePath = profileImagePath,
                    NationalIdFrontPath = nationalIdFrontPath,
                    NationalIdBackPath = nationalIdBackPath,

                    Governorate = request.Governorate.Trim(),
                    Center = request.Center.Trim(),
                    Village = request.Village.Trim(),

                    TermsAccepted = request.TermsAccepted,
                    TermsAcceptedAtUtc = DateTime.UtcNow,
                    TermsVersion = CurrentArtisanTermsVersion,

                    ApprovalStatus = "Pending"
                };

                artisanProfile.Specialties =
                    request.Specialties
                        .Select(s => s.Trim())
                        .Select(s => new ArtisanSpecialty
                        {
                            ArtisanProfile = artisanProfile,
                            Name = s
                        })
                        .ToList();

                artisanProfile.ServiceTypes =
                    request.ServiceTypes
                        .Select(s => s.Trim())
                        .Select(s => new ArtisanServiceType
                        {
                            ArtisanProfile = artisanProfile,
                            Name = s
                        })
                        .ToList();

                artisanProfile.NearbyServiceAreas =
                    request.NearbyServiceAreas
                        .Select(a => new ArtisanServiceArea
                        {
                            ArtisanProfile = artisanProfile,
                            Governorate = a.Governorate.Trim(),
                            Center = a.Center.Trim(),
                            Village = a.Village.Trim()
                        })
                        .ToList();

                _context.Users.Add(user);
                _context.ArtisanProfiles.Add(artisanProfile);

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    databaseSaved = true;
                }
                catch (DbUpdateException ex)
                    when (IsUniqueConstraintViolation(ex))
                {
                    await CleanupUploadedFilesAsync(savedFileKeys);

                    return Conflict(new
                    {
                        message =
                            "An account with this email or phone number is already registered."
                    });
                }

                // Return a safe summary. Never return private document paths.
                return StatusCode(201, new
                {
                    message =
                        "Artisan registration submitted. Verify your contact and wait for admin approval.",
                    userId = user.Id,
                    role = user.Role,
                    accountStatus = user.AccountStatus,
                    approvalStatus = artisanProfile.ApprovalStatus,

                    profile = new
                    {
                        user.FullName,
                        user.Email,
                        user.PhoneNumber,

                        artisanProfile.Profession,
                        artisanProfile.ExperienceYears,
                        artisanProfile.Bio,

                        artisanProfile.Governorate,
                        artisanProfile.Center,
                        artisanProfile.Village,

                        specialties = artisanProfile.Specialties
                            .Select(s => s.Name)
                            .ToArray(),

                        serviceTypes = artisanProfile.ServiceTypes
                            .Select(s => s.Name)
                            .ToArray(),

                        nearbyServiceAreas =
                            artisanProfile.NearbyServiceAreas
                                .Select(a => new
                                {
                                    a.Governorate,
                                    a.Center,
                                    a.Village
                                })
                                .ToArray(),

                        profileImageUploaded = true,
                        identityDocumentsUploaded = true,

                        artisanProfile.TermsAccepted,
                        artisanProfile.TermsAcceptedAtUtc,
                        artisanProfile.TermsVersion
                    }
                });
            }
            catch (InvalidDataException ex)
            {
                await CleanupUploadedFilesAsync(savedFileKeys);

                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch
            {
                if (!databaseSaved)
                {
                    await CleanupUploadedFilesAsync(savedFileKeys);
                }

                throw;
            }
        }

        // Login.
        [EnableRateLimiting("LoginPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request)
        {
            var email = string.IsNullOrWhiteSpace(request.Email)
                ? null
                : request.Email.Trim().ToLowerInvariant();

            var phoneNumber =
                string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? null
                    : request.PhoneNumber.Trim();

            if (email is null && phoneNumber is null)
            {
                return BadRequest(new
                {
                    message = "Enter your email or phone number."
                });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                (email != null && u.Email == email) ||
                (phoneNumber != null &&
                 u.PhoneNumber == phoneNumber));

            if (user is null ||
                !_passwordService.VerifyPassword(
                    user.PasswordHash,
                    request.Password))
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid email/phone number or password."
                });
            }

            if (user.AccountStatus != "Active")
            {
                return Unauthorized(new
                {
                    message =
                        "Your account is not active yet or is waiting for approval.",
                        userId = user.Id,
                    role = user.Role,
                    accountStatus = user.AccountStatus
                });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token,
                expiresIn = 3600,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.PhoneNumber,
                    user.Role
                }
            });
        }

        private async Task CleanupUploadedFilesAsync(
            IEnumerable<string> storageKeys)
        {
            foreach (var storageKey in storageKeys)
            {
                try
                {
                    await _privateFileStorage.DeleteAsync(storageKey);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to clean up a private upload.");
                }
            }
        }

        private static bool IsUniqueConstraintViolation(
            DbUpdateException exception)
        {
            for (
                Exception? current = exception;
                current is not null;
                current = current.InnerException)
            {
                if (current is SqlException sqlException &&
                    (sqlException.Number == 2601 ||
                     sqlException.Number == 2627))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
