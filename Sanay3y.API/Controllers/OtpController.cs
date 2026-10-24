
using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class OtpController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly OtpService _otpService;
        private readonly IWebHostEnvironment _environment;

        public OtpController(
            ApplicationDbContext context,
            OtpService otpService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _otpService = otpService;
            _environment = environment;
        }

        // Request an OTP for a registered contact.
        [EnableRateLimiting("OtpRequestPolicy")]
        [HttpPost("request")]
        public async Task<IActionResult> RequestOtp(
            RequestOtpRequest request)
        {
            // Real email/SMS delivery is not configured yet.
            if (!_environment.IsDevelopment())
            {
                return StatusCode(501, new
                {
                    message = "OTP delivery is not configured yet."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user is null)
            {
                return NotFound(new
                {
                    message = "User was not found."
                });
            }

            if (request.Channel == "Email")
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new
                    {
                        message =
                            "No email is registered for this user."
                    });
                }

                if (user.IsEmailVerified)
                {
                    return BadRequest(new
                    {
                        message = "Email is already verified."
                    });
                }
            }
            else if (request.Channel == "Phone")
            {
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    return BadRequest(new
                    {
                        message =
                            "No phone number is registered for this user."
                    });
                }

                if (user.IsPhoneVerified)
                {
                    return BadRequest(new
                    {
                        message = "Phone number is already verified."
                    });
                }
            }
            else
            {
                return BadRequest(new
                {
                    message = "Channel must be Email or Phone."
                });
            }

            var now = DateTime.UtcNow;

            var recentlyRequested =
                await _context.OtpVerifications.AnyAsync(o =>
                    o.UserId == request.UserId &&
                    o.Channel == request.Channel &&
                    o.CreatedAtUtc > now.AddSeconds(-60));

            if (recentlyRequested)
            {
                return StatusCode(429, new
                {
                    message =
                        "Please wait 60 seconds before requesting another code."
                });
            }

            var code = _otpService.GenerateCode();

            var otp = new OtpVerification
            {
                UserId = user.Id,
                Channel = request.Channel,
                CodeHash = _otpService.HashCode(code),
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(5),
                Attempts = 0
            };

            _context.OtpVerifications.Add(otp);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "OTP created for development testing.",
                expiresInSeconds = 300,
                developmentCode = code
            });
        }

        [EnableRateLimiting("OtpVerifyPolicy")]
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp(
            VerifyOtpRequest request)
        {
            if (request.Channel != "Email" &&
                request.Channel != "Phone")
            {
                return BadRequest(new
                {
                    message = "Channel must be Email or Phone."
                });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user is null)
            {
                return BadRequest(new
                {
                    message = "Invalid verification request."
                });
            }

            var otp = await _context.OtpVerifications
                .Where(o =>
                    o.UserId == request.UserId &&
                    o.Channel == request.Channel &&
                    o.UsedAtUtc == null)
                .OrderByDescending(o => o.CreatedAtUtc)
                .FirstOrDefaultAsync();

            if (otp is null)
            {
                return BadRequest(new
                {
                    message =
                        "No active verification code was found."
                });
            }

            var now = DateTime.UtcNow;

            if (otp.ExpiresAtUtc <= now)
            {
                return BadRequest(new
                {
                    message = "Verification code has expired."
                });
            }

            if (otp.Attempts >= 5)
            {
                return BadRequest(new
                {
                    message =
                        "Too many attempts. Request a new code later."
                });
            }

            // Count each attempt, including a successful attempt.
            otp.Attempts++;

            if (!_otpService.VerifyCode(request.Code, otp.CodeHash))
            {
                await _context.SaveChangesAsync();

                return BadRequest(new
                {
                    message = "Invalid verification code."
                });
            }

            if (request.Channel == "Email")
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return BadRequest(new
                    {
                        message =
                            "No email is registered for this user."
                    });
                }

                user.IsEmailVerified = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    return BadRequest(new
                    {
                        message =
                            "No phone number is registered for this user."
                    });
                }

                user.IsPhoneVerified = true;
            }

            // Prevent reuse of the verification code.
            otp.UsedAtUtc = now;

            if (user.Role == "Customer")
            {
                user.AccountStatus = "Active";
            }
            else if (user.Role == "Artisan")
            {
                var artisanProfileExists =
                    await _context.ArtisanProfiles
                        .AnyAsync(p => p.UserId == user.Id);

                if (!artisanProfileExists)
                {
                    return BadRequest(new
                    {
                        message =
                            "Artisan profile was not found. Contact support."
                    });
                }

                user.AccountStatus = "PendingApproval";
            }
            else
            {
                // Do not activate accounts with unexpected roles.
                return BadRequest(new
                {
                    message =
                        "This account role cannot be activated through OTP verification."
                });
            }

            await _context.SaveChangesAsync();

            var responseMessage = user.Role == "Artisan"
                ? "Contact verified successfully. Your account is waiting for admin approval."
                : "Contact verified successfully. Your account is now active.";

            return Ok(new
            {
                message = responseMessage,
                userId = user.Id,
                channel = request.Channel,
                role = user.Role,
                accountStatus = user.AccountStatus
            });
        }
    }
}
