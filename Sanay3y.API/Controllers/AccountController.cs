
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;
using Sanay3y.API.Services;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OtpService _otpService;
        private readonly IWebHostEnvironment _environment;

        public AccountController(
            ApplicationDbContext context,
            OtpService otpService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _otpService = otpService;
            _environment = environment;
        }

        // GET: api/Account/me
        // View the current user's profile.
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.Role,
                    u.AccountStatus,
                    u.IsEmailVerified,
                    u.IsPhoneVerified,
                    u.CreatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User account was not found."
                });
            }

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                role = user.Role,
                accountStatus = user.AccountStatus,
                isEmailVerified = user.IsEmailVerified,
                isPhoneVerified = user.IsPhoneVerified,
                createdAtUtc = user.CreatedAtUtc
            });
        }

        // PUT: api/Account/me
        // Update the Customer's full name.
        [Authorize(Roles = "Customer")]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateCustomerProfileDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var user = await GetActiveCustomerAsync(
                userId,
                cancellationToken);

            if (user == null)
                return Forbid();

            var fullName = dto.FullName?.Trim();

            if (string.IsNullOrWhiteSpace(fullName) ||
                fullName.Length < 2 ||
                fullName.Length > 100)
            {
                return BadRequest(new
                {
                    message = "Full name must contain between 2 and 100 characters."
                });
            }

            user.FullName = fullName;

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Profile updated successfully.",
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber
            });
        }

        // POST: api/Account/me/phone-change/request
        // Request an OTP for a new phone number.
        [Authorize(Roles = "Customer")]
        [EnableRateLimiting("OtpRequestPolicy")]
        [HttpPost("me/phone-change/request")]
        public async Task<IActionResult> RequestPhoneChangeOtp(
            [FromBody] RequestPhoneChangeOtpDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            // SMS delivery is not configured yet.
            if (!_environment.IsDevelopment())
            {
                return StatusCode(501, new
                {
                    message = "SMS delivery is not configured yet."
                });
            }

            var user = await GetActiveCustomerAsync(
                userId,
                cancellationToken);

            if (user == null)
                return Forbid();

            var newPhoneNumber = dto.NewPhoneNumber?.Trim();

            if (string.IsNullOrWhiteSpace(newPhoneNumber) ||
                !Regex.IsMatch(
                    newPhoneNumber,
                    @"^\+?[0-9]{8,15}$"))
            {
                return BadRequest(new
                {
                    message = "Please enter a valid phone number."
                });
            }

            if (newPhoneNumber == user.PhoneNumber)
            {
                return BadRequest(new
                {
                    message = "This is already your registered phone number."
                });
            }

            // Prevent using a phone number registered by another user.
            var phoneAlreadyUsed = await _context.Users
                .AnyAsync(
                    u => u.PhoneNumber == newPhoneNumber &&
                         u.Id != userId,
                    cancellationToken);

            if (phoneAlreadyUsed)
            {
                return Conflict(new
                {
                    message = "This phone number is already registered."
                });
            }

            var now = DateTime.UtcNow;

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            // Keep the existing 60-second request cooldown.
            var recentlyRequested = await _context.OtpVerifications
                .AnyAsync(
                    o => o.UserId == userId &&
                         o.Channel == "PhoneChange" &&
                         o.CreatedAtUtc > now.AddSeconds(-60),
                    cancellationToken);

            if (recentlyRequested)
            {
                await transaction.RollbackAsync(cancellationToken);

                return StatusCode(429, new
                {
                    message = "Please wait 60 seconds before requesting another code."
                });
            }

            // Invalidate previous unused phone-change codes.
            await _context.OtpVerifications
                .Where(o =>
                    o.UserId == userId &&
                    o.Channel == "PhoneChange" &&
                    o.UsedAtUtc == null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        o => o.UsedAtUtc,
                        (DateTime?)now),
                    cancellationToken);

            var code = _otpService.GenerateCode();

            var otp = new OtpVerification
            {
                UserId = userId,
                Channel = "PhoneChange",
                CodeHash = _otpService.HashCode(code),
                TargetValue = newPhoneNumber,
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(5),
                Attempts = 0
            };

            _context.OtpVerifications.Add(otp);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Development-only OTP response.
            return Ok(new
            {
                message = "Phone change verification code created for development testing.",
                expiresInSeconds = 300,
                developmentCode = code,
                newPhoneNumber = MaskPhoneNumber(newPhoneNumber)
            });
        }

        // POST: api/Account/me/phone-change/verify
        // Verify the OTP and update the phone number.
        [Authorize(Roles = "Customer")]
        [EnableRateLimiting("OtpVerifyPolicy")]
        [HttpPost("me/phone-change/verify")]
        public async Task<IActionResult> VerifyPhoneChangeOtp(
            [FromBody] VerifyPhoneChangeOtpDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
                return Unauthorized();

            var now = DateTime.UtcNow;

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var user = await GetActiveCustomerAsync(
                userId,
                cancellationToken);

            if (user == null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Forbid();
            }

            // Retrieve the latest unused phone-change OTP.
            var otp = await _context.OtpVerifications
                .AsNoTracking()
                .Where(o =>
                    o.UserId == userId &&
                    o.Channel == "PhoneChange" &&
                    o.UsedAtUtc == null)
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (otp == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "No active phone-change verification code was found."
                });
            }

            if (otp.ExpiresAtUtc <= now)
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "Verification code has expired."
                });
            }

            if (otp.Attempts >= 5)
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "Too many attempts. Request a new code later."
                });
            }

            if (string.IsNullOrWhiteSpace(otp.TargetValue))
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "The requested phone number is invalid."
                });
            }

            // Check again before saving the new phone number.
            var phoneAlreadyUsed = await _context.Users
                .AnyAsync(
                    u => u.PhoneNumber == otp.TargetValue &&
                         u.Id != userId,
                    cancellationToken);

            if (phoneAlreadyUsed)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "This phone number is already registered. Request a code for another number."
                });
            }

            // Increment attempts atomically.
            var attemptsUpdated = await _context.OtpVerifications
                .Where(o =>
                    o.Id == otp.Id &&
                    o.UserId == userId &&
                    o.Channel == "PhoneChange" &&
                    o.UsedAtUtc == null &&
                    o.ExpiresAtUtc > now &&
                    o.Attempts < 5)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        o => o.Attempts,
                        o => o.Attempts + 1),
                    cancellationToken);

            if (attemptsUpdated != 1)
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "The verification code is no longer valid. Request a new code."
                });
            }

            // A wrong code still counts as an attempt.
            if (!_otpService.VerifyCode(dto.Code, otp.CodeHash))
            {
                await transaction.CommitAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "Invalid verification code."
                });
            }

            // Consume this OTP atomically.
            var consumed = await _context.OtpVerifications
                .Where(o =>
                    o.Id == otp.Id &&
                    o.UserId == userId &&
                    o.Channel == "PhoneChange" &&
                    o.UsedAtUtc == null &&
                    o.ExpiresAtUtc > now)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        o => o.UsedAtUtc,
                        (DateTime?)now),
                    cancellationToken);

            if (consumed != 1)
            {
                await transaction.RollbackAsync(cancellationToken);

                return BadRequest(new
                {
                    message = "The verification code has already been used or expired."
                });
            }

            // Update the user's phone only after successful verification.
            user.PhoneNumber = otp.TargetValue;
            user.IsPhoneVerified = true;

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Phone number verified and updated successfully.",
                userId = user.Id,
                phoneNumber = user.PhoneNumber,
                isPhoneVerified = user.IsPhoneVerified
            });
        }

        // Retrieve an active Customer account.
        private async Task<User?> GetActiveCustomerAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Id == userId &&
                         u.Role == "Customer" &&
                         u.AccountStatus == "Active",
                    cancellationToken);
        }

        private bool TryGetUserId(out int userId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out userId);
        }

        private static string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            var value = phoneNumber.Trim();

            if (value.Length <= 4)
                return new string('*', value.Length);

            return new string('*', value.Length - 4)
                + value[^4..];
        }
    }
}
