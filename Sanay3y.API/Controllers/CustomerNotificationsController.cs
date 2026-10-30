
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using System.Security.Claims;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerNotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CustomerNotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // الحصول على Customer ID من JWT
        private int? GetCustomerId()
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (int.TryParse(userId, out int customerId))
                return customerId;

            return null;
        }

        // GET: api/CustomerNotifications
        // عرض إشعارات العميل
        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return Unauthorized(new
                {
                    message = "Invalid customer token."
                });

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == customerId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    n.CreatedAtUtc,
                    n.ReadAtUtc,
                    n.RelatedEntityType,
                    n.RelatedEntityId
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                notifications
            });
        }

        // GET: api/CustomerNotifications/unread-count
        // عدد الإشعارات غير المقروءة
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(
            CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return Unauthorized();

            var count = await _context.Notifications
                .CountAsync(
                    n => n.UserId == customerId.Value && !n.IsRead,
                    cancellationToken);

            return Ok(new
            {
                unreadCount = count
            });
        }

        // PUT: api/CustomerNotifications/{id}/read
        // تعليم إشعار معين كمقروء
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(
            int id,
            CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(
                    n => n.Id == id && n.UserId == customerId.Value,
                    cancellationToken);

            if (notification == null)
                return NotFound(new
                {
                    message = "Notification not found."
                });

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);
            }

            return Ok(new
            {
                message = "Notification marked as read.",
                notificationId = notification.Id,
                notification.IsRead,
                notification.ReadAtUtc
            });
        }

        // PUT: api/CustomerNotifications/read-all
        // تعليم كل الإشعارات كمقروءة
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead(
            CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return Unauthorized();

            var unreadNotifications = await _context.Notifications
                .Where(n =>
                    n.UserId == customerId.Value &&
                    !n.IsRead)
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = now;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "All notifications marked as read.",
                updatedCount = unreadNotifications.Count
            });
        }

        // DELETE: api/CustomerNotifications/{id}
        // حذف إشعار معين
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNotification(
            int id,
            CancellationToken cancellationToken)
        {
            var customerId = GetCustomerId();

            if (customerId == null)
                return Unauthorized();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(
                    n => n.Id == id && n.UserId == customerId.Value,
                    cancellationToken);

            if (notification == null)
                return NotFound(new
                {
                    message = "Notification not found."
                });

            _context.Notifications.Remove(notification);

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Notification deleted successfully."
            });
        }
    }
}
