using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Admin/pending-artisans
        [HttpGet("pending-artisans")]
        public async Task<IActionResult> GetPendingArtisans()
        {
            var artisans = await _context.ArtisanProfiles
                .Where(p =>
                    p.ApprovalStatus == "Pending" &&
                    p.User.AccountStatus == "PendingApproval")
                .Select(p => new
                {
                    UserId = p.UserId,
                    p.User.FullName,
                    p.User.Email,
                    p.User.PhoneNumber,
                    p.Profession,
                    p.ExperienceYears,
                    p.ServiceArea,
                    p.ApprovalStatus,
                    p.User.AccountStatus
                })
                .ToListAsync();

            return Ok(artisans);
        }

        // PUT: api/Admin/artisans/{userId}/approve
        [HttpPut("artisans/{userId:int}/approve")]
        public async Task<IActionResult> ApproveArtisan(int userId)
        {
            var profile = await _context.ArtisanProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile is null)
            {
                return NotFound(new
                {
                    message = "Artisan profile was not found."
                });
            }

            if (profile.ApprovalStatus != "Pending" ||
                profile.User.AccountStatus != "PendingApproval")
            {
                return BadRequest(new
                {
                    message = "This artisan is not waiting for approval."
                });
            }

            var adminIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(adminIdValue))
            {
                adminIdValue = User.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);
            }

            if (!int.TryParse(adminIdValue, out var adminId))
            {
                return Unauthorized(new
                {
                    message = "Admin identity is invalid."
                });
            }

            var adminExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == adminId &&
                    u.Role == "Admin" &&
                    u.AccountStatus == "Active");

            if (!adminExists)
            {
                return Forbid();
            }

            profile.ApprovalStatus = "Approved";
            profile.ReviewedByAdminId = adminId;
            profile.ReviewedAtUtc = DateTime.UtcNow;
            profile.RejectionReason = null;
            profile.User.AccountStatus = "Active";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Artisan approved successfully.",
                userId = profile.UserId,
                approvalStatus = profile.ApprovalStatus,
                accountStatus = profile.User.AccountStatus
            });
        }

        // PUT: api/Admin/artisans/{userId}/reject
        [HttpPut("artisans/{userId:int}/reject")]
        public async Task<IActionResult> RejectArtisan(
            int userId,
            RejectArtisanRequest request)
        {
            var profile = await _context.ArtisanProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile is null)
            {
                return NotFound(new
                {
                    message = "Artisan profile was not found."
                });
            }

            if (profile.ApprovalStatus != "Pending" ||
                profile.User.AccountStatus != "PendingApproval")
            {
                return BadRequest(new
                {
                    message = "This artisan is not waiting for approval."
                });
            }

            var adminIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(adminIdValue))
            {
                adminIdValue = User.FindFirstValue(
                    JwtRegisteredClaimNames.Sub);
            }

            if (!int.TryParse(adminIdValue, out var adminId))
            {
                return Unauthorized(new
                {
                    message = "Admin identity is invalid."
                });
            }

            var adminExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == adminId &&
                    u.Role == "Admin" &&
                    u.AccountStatus == "Active");

            if (!adminExists)
            {
                return Forbid();
            }

            profile.ApprovalStatus = "Rejected";
            profile.ReviewedByAdminId = adminId;
            profile.ReviewedAtUtc = DateTime.UtcNow;
            profile.RejectionReason = request.Reason.Trim();

            // A rejected artisan cannot log in.
            profile.User.AccountStatus = "Suspended";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Artisan rejected.",
                userId = profile.UserId,
                approvalStatus = profile.ApprovalStatus,
                accountStatus = profile.User.AccountStatus,
                rejectionReason = profile.RejectionReason
            });
        }
    }
}