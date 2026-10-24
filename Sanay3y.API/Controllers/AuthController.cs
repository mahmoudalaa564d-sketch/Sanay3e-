
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthController(
            ApplicationDbContext context,
            PasswordService passwordService,
            JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
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
                userId = user.Id,
                role = user.Role,
                accountStatus = user.AccountStatus
            });
        }

        // Register an artisan.
        [EnableRateLimiting("RegisterPolicy")]
        [HttpPost("register-artisan")]
        public async Task<IActionResult> RegisterArtisan(
            ArtisanRegisterRequest request)
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
                Role = "Artisan",
                AccountStatus = "PendingVerification",
                IsEmailVerified = false,
                IsPhoneVerified = false
            };

            var artisanProfile = new ArtisanProfile
            {
                User = user,
                Profession = request.Profession.Trim(),
                ExperienceYears = request.ExperienceYears,
                ServiceArea = request.ServiceArea.Trim(),
                ApprovalStatus = "Pending"
            };

            _context.Users.Add(user);
            _context.ArtisanProfiles.Add(artisanProfile);

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
                    "Artisan registration created. Verify your contact and wait for admin approval.",
                userId = user.Id,
                role = user.Role,
                accountStatus = user.AccountStatus,
                approvalStatus = artisanProfile.ApprovalStatus
            });
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
                        "Your account is not active yet or is waiting for approval."
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
