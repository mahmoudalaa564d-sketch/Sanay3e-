
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerReviewsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/CustomerReviews/requests/1
        // العميل يقيّم الصنايعي بعد اكتمال الطلب والدفع
        [HttpPost("requests/{requestId:int}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CreateReview(
            int requestId,
            [FromBody] CreateServiceReviewDto input,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int customerUserId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            // التأكد أن الطلب يخص العميل الحالي
            var serviceRequest = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == requestId &&
                         r.CustomerUserId == customerUserId,
                    cancellationToken);

            if (serviceRequest == null)
            {
                return NotFound(new
                {
                    message = "طلب الخدمة غير موجود أو لا يخص حسابك."
                });
            }

            // لا يمكن التقييم قبل اكتمال الطلب
            if (serviceRequest.Status != "Completed")
            {
                return Conflict(new
                {
                    message = "لا يمكن تقييم الصنايعي قبل اكتمال الطلب."
                });
            }

            // التأكد من وجود دفعة ناجحة لهذا الطلب
            var paymentConfirmed = await _context.ServicePayments
                .AnyAsync(
                    p => p.ServiceRequestId == requestId &&
                         p.CustomerUserId == customerUserId &&
                         p.Status == "Paid",
                    cancellationToken);

            if (!paymentConfirmed)
            {
                return Conflict(new
                {
                    message = "لا يمكن التقييم قبل تأكيد الدفع."
                });
            }

            // تحديد الصنايعي صاحب العرض المقبول
            var artisanUserId = await _context.ServiceOffers
                .Where(o =>
                    o.ServiceRequestId == requestId &&
                    o.Status == "Accepted")
                .Select(o => (int?)o.ArtisanUserId)
                .SingleOrDefaultAsync(cancellationToken);

            if (artisanUserId == null)
            {
                return Conflict(new
                {
                    message = "لم يتم العثور على الصنايعي صاحب العرض المقبول."
                });
            }

            var alreadyReviewed = await _context.ServiceReviews
                .AnyAsync(
                    r => r.ServiceRequestId == requestId,
                    cancellationToken);

            if (alreadyReviewed)
            {
                return Conflict(new
                {
                    message = "تم تقييم هذا الطلب من قبل."
                });
            }

            var review = new ServiceReview
            {
                ServiceRequestId = requestId,
                CustomerUserId = customerUserId,
                ArtisanUserId = artisanUserId.Value,
                Rating = input.Rating,
                Comment = string.IsNullOrWhiteSpace(input.Comment)
                    ? null
                    : input.Comment.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.ServiceReviews.Add(review);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // حماية إضافية من تكرار التقييم في نفس الوقت
                return Conflict(new
                {
                    message = "تعذر حفظ التقييم. ربما تم تقييم الطلب بالفعل."
                });
            }

            return StatusCode(201, new
            {
                message = "تم حفظ تقييمك بنجاح.",
                review = new
                {
                    review.Id,
                    review.ServiceRequestId,
                    review.ArtisanUserId,
                    review.Rating,
                    review.Comment,
                    review.CreatedAtUtc
                }
            });
        }

        // GET: api/CustomerReviews/artisans/4
        // عرض تقييمات الصنايعي ومتوسط التقييم
        [HttpGet("artisans/{artisanUserId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArtisanReviews(
            int artisanUserId,
            CancellationToken cancellationToken)
        {
            var artisanExists = await _context.Users
                .AnyAsync(
                    u => u.Id == artisanUserId &&
                         u.Role == "Artisan",
                    cancellationToken);

            if (!artisanExists)
            {
                return NotFound(new
                {
                    message = "حساب الصنايعي غير موجود."
                });
            }

            var reviewsQuery = _context.ServiceReviews
                .AsNoTracking()
                .Where(r => r.ArtisanUserId == artisanUserId);

            var averageRating = await reviewsQuery
                .Select(r => (double?)r.Rating)
                .AverageAsync(cancellationToken) ?? 0;

            var totalReviews = await reviewsQuery
                .CountAsync(cancellationToken);

            var reviews = await (
                from review in reviewsQuery
                join customer in _context.Users.AsNoTracking()
                    on review.CustomerUserId equals customer.Id
                orderby review.CreatedAtUtc descending
                select new
                {
                    review.Id,
                    review.ServiceRequestId,
                    CustomerName = customer.FullName,
                    review.Rating,
                    review.Comment,
                    review.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                artisanUserId,
                averageRating = Math.Round(averageRating, 2),
                totalReviews,
                reviews
            });
        }

        private bool TryGetUserId(out int userId)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(idValue, out userId);
        }
    }
}
