using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.DTOs;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CustomerController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // =========================================================
        // CUSTOMER ADDRESSES
        // =========================================================

        // POST: api/Customer/addresses
        [HttpPost("addresses")]
        public async Task<IActionResult> CreateAddress(
            [FromBody] CreateCustomerAddressDto dto)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var customerIsActive = await _context.Users
                .AnyAsync(u =>
                    u.Id == customerId &&
                    u.Role == "Customer" &&
                    u.AccountStatus == "Active");

            if (!customerIsActive)
            {
                return Forbid();
            }

            if (dto.Latitude.HasValue != dto.Longitude.HasValue)
            {
                return BadRequest(new
                {
                    message = "يجب إرسال خط العرض وخط الطول معًا."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            var hasDefaultAddress = await _context.CustomerAddresses
                .AnyAsync(a =>
                    a.CustomerUserId == customerId &&
                    a.IsDefault);

            var address = new CustomerAddress
            {
                CustomerUserId = customerId,
                Title = dto.Title.Trim(),
                Governorate = dto.Governorate.Trim(),
                City = dto.City.Trim(),
                District = dto.District?.Trim(),
                Street = dto.Street.Trim(),
                BuildingNumber = dto.BuildingNumber?.Trim(),
                FloorNumber = dto.FloorNumber?.Trim(),
                ApartmentNumber = dto.ApartmentNumber?.Trim(),
                Landmark = dto.Landmark?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                IsDefault = dto.IsDefault || !hasDefaultAddress,
                CreatedAtUtc = DateTime.UtcNow
            };

            if (address.IsDefault && hasDefaultAddress)
            {
                var oldDefaultAddresses =
                    await _context.CustomerAddresses
                        .Where(a =>
                            a.CustomerUserId == customerId &&
                            a.IsDefault)
                        .ToListAsync();

                foreach (var oldAddress in oldDefaultAddresses)
                {
                    oldAddress.IsDefault = false;
                }
            }

            _context.CustomerAddresses.Add(address);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return StatusCode(201, new
            {
                message = "تم إضافة العنوان بنجاح.",
                address = new
                {
                    id = address.Id,
                    title = address.Title,
                    governorate = address.Governorate,
                    city = address.City,
                    district = address.District,
                    street = address.Street,
                    buildingNumber = address.BuildingNumber,
                    floorNumber = address.FloorNumber,
                    apartmentNumber = address.ApartmentNumber,
                    landmark = address.Landmark,
                    latitude = address.Latitude,
                    longitude = address.Longitude,
                    isDefault = address.IsDefault,
                    createdAtUtc = address.CreatedAtUtc
                }
            });
        }

        // GET: api/Customer/addresses
        [HttpGet("addresses")]
        public async Task<IActionResult> GetMyAddresses()
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var addresses = await _context.CustomerAddresses
                .AsNoTracking()
                .Where(a => a.CustomerUserId == customerId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAtUtc)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    governorate = a.Governorate,
                    city = a.City,
                    district = a.District,
                    street = a.Street,
                    buildingNumber = a.BuildingNumber,
                    floorNumber = a.FloorNumber,
                    apartmentNumber = a.ApartmentNumber,
                    landmark = a.Landmark,
                    latitude = a.Latitude,
                    longitude = a.Longitude,
                    isDefault = a.IsDefault,
                    createdAtUtc = a.CreatedAtUtc
                })
                .ToListAsync();

            return Ok(new
            {
                count = addresses.Count,
                addresses
            });
        }

        // PUT: api/Customer/addresses/{id}
        [HttpPut("addresses/{id:int}")]
        public async Task<IActionResult> UpdateAddress(
            int id,
            [FromBody] UpdateCustomerAddressDto dto)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            if (dto.Latitude.HasValue != dto.Longitude.HasValue)
            {
                return BadRequest(new
                {
                    message = "يجب إرسال خط العرض وخط الطول معًا."
                });
            }

            var address = await _context.CustomerAddresses
                .SingleOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CustomerUserId == customerId);

            if (address == null)
            {
                return NotFound(new
                {
                    message = "العنوان غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            address.Title = dto.Title.Trim();
            address.Governorate = dto.Governorate.Trim();
            address.City = dto.City.Trim();
            address.District = dto.District?.Trim();
            address.Street = dto.Street.Trim();
            address.BuildingNumber = dto.BuildingNumber?.Trim();
            address.FloorNumber = dto.FloorNumber?.Trim();
            address.ApartmentNumber = dto.ApartmentNumber?.Trim();
            address.Landmark = dto.Landmark?.Trim();
            address.Latitude = dto.Latitude;
            address.Longitude = dto.Longitude;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم تعديل العنوان بنجاح.",
                address = new
                {
                    id = address.Id,
                    title = address.Title,
                    governorate = address.Governorate,
                    city = address.City,
                    district = address.District,
                    street = address.Street,
                    buildingNumber = address.BuildingNumber,
                    floorNumber = address.FloorNumber,
                    apartmentNumber = address.ApartmentNumber,
                    landmark = address.Landmark,
                    latitude = address.Latitude,
                    longitude = address.Longitude,
                    isDefault = address.IsDefault
                }
            });
        }

        // DELETE: api/Customer/addresses/{id}
        [HttpDelete("addresses/{id:int}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var address = await _context.CustomerAddresses
                .SingleOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CustomerUserId == customerId);

            if (address == null)
            {
                return NotFound(new
                {
                    message = "العنوان غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "تم حذف العنوان بنجاح.",
                deletedAddressId = id
            });
        }

        // PUT: api/Customer/addresses/{id}/default
        [HttpPut("addresses/{id:int}/default")]
        public async Task<IActionResult> SetDefaultAddress(int id)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable);

            var address = await _context.CustomerAddresses
                .SingleOrDefaultAsync(a =>
                    a.Id == id &&
                    a.CustomerUserId == customerId);

            if (address == null)
            {
                return NotFound(new
                {
                    message = "العنوان غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            var currentDefaultAddresses =
                await _context.CustomerAddresses
                    .Where(a =>
                        a.CustomerUserId == customerId &&
                        a.IsDefault)
                    .ToListAsync();

            foreach (var currentAddress in currentDefaultAddresses)
            {
                currentAddress.IsDefault = false;
            }

            address.IsDefault = true;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "تم تعيين العنوان الافتراضي بنجاح.",
                defaultAddress = new
                {
                    id = address.Id,
                    title = address.Title,
                    governorate = address.Governorate,
                    city = address.City,
                    street = address.Street,
                    isDefault = address.IsDefault
                }
            });
        }

        // =========================================================
        // CUSTOMER ORDERS
        // =========================================================

        // POST: api/Customer/orders
        // Create a service request and notify the customer.
        [HttpPost("orders")]
        public async Task<IActionResult> CreateOrder(
            [FromBody] CreateServiceRequestDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var customer = await _context.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(u =>
                    u.Id == customerId &&
                    u.Role == "Customer" &&
                    u.AccountStatus == "Active",
                    cancellationToken);

            if (customer == null)
            {
                return Forbid();
            }

            var specialtyExists = await _context.Specialties
                .AnyAsync(s =>
                    s.Id == dto.SpecialtyId &&
                    s.IsActive,
                    cancellationToken);

            if (!specialtyExists)
            {
                return BadRequest(new
                {
                    message = "التخصص غير موجود أو غير مفعّل."
                });
            }

            if (dto.IsAsSoonAsPossible)
            {
                if (dto.ScheduledAtUtc.HasValue)
                {
                    return BadRequest(new
                    {
                        message =
                            "عند اختيار أقرب وقت، لا ترسل موعدًا محددًا."
                    });
                }
            }
            else if (!dto.ScheduledAtUtc.HasValue ||
                     dto.ScheduledAtUtc.Value <= DateTimeOffset.UtcNow)
            {
                return BadRequest(new
                {
                    message =
                        "حدد تاريخًا ووقتًا مستقبليًا، أو اختر أقرب وقت."
                });
            }

            if (dto.Latitude.HasValue != dto.Longitude.HasValue)
            {
                return BadRequest(new
                {
                    message = "يجب إرسال خط العرض وخط الطول معًا."
                });
            }

            var now = DateTime.UtcNow;

            var requestNumber =
                $"SN-{now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

            var request = new ServiceRequest
            {
                RequestNumber = requestNumber,
                CustomerUserId = customer.Id,
                SpecialtyId = dto.SpecialtyId,
                ServiceTypeName = dto.ServiceTypeName.Trim(),
                ProblemDescription = dto.ProblemDescription.Trim(),
                AdditionalNotes = dto.AdditionalNotes?.Trim(),
                IsAsSoonAsPossible = dto.IsAsSoonAsPossible,
                ScheduledAtUtc =
                    dto.ScheduledAtUtc?.ToUniversalTime(),
                AddressTitle = dto.AddressTitle.Trim(),
                Governorate = dto.Governorate.Trim(),
                City = dto.City.Trim(),
                District = dto.District?.Trim(),
                Street = dto.Street.Trim(),
                BuildingNumber = dto.BuildingNumber?.Trim(),
                FloorNumber = dto.FloorNumber?.Trim(),
                ApartmentNumber = dto.ApartmentNumber?.Trim(),
                Landmark = dto.Landmark?.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Status = "PendingOffers",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    cancellationToken);

            _context.ServiceRequests.Add(request);

            await _context.SaveChangesAsync(cancellationToken);

            var notification = new Notification
            {
                UserId = customer.Id,
                Title = "تم إنشاء طلبك بنجاح",
                Message =
                    $"تم استلام طلب الخدمة رقم {request.RequestNumber}، وهو الآن في انتظار عروض الصنايعية.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = DateTime.UtcNow,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = request.Id
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return StatusCode(201, new
            {
                message = "تم إنشاء طلب الخدمة بنجاح.",
                id = request.Id,
                requestNumber = request.RequestNumber,
                status = request.Status,
                nextStep = "انتظار عروض الصنايعية",
                createdAtUtc = request.CreatedAtUtc,
                notification = new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type,
                    isRead = notification.IsRead
                }
            });
        }

        // GET: api/Customer/orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] string? status = null,
            [FromQuery] string? requestNumber = null)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var query = _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.CustomerUserId == customerId);

            if (!string.IsNullOrWhiteSpace(requestNumber))
            {
                var searchTerm = requestNumber.Trim();

                query = query.Where(r =>
                    r.RequestNumber.Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                switch (status.Trim().ToLowerInvariant())
                {
                    case "all":
                    case "الكل":
                        break;

                    case "active":
                    case "نشطة":
                        query = query.Where(r =>
                            r.Status == "PendingOffers" ||
                            r.Status == "OfferAccepted" ||
                            r.Status == "InProgress" ||
                            r.Status == "AwaitingPayment");
                        break;

                    case "completed":
                    case "مكتملة":
                        query = query.Where(r =>
                            r.Status == "Completed");
                        break;

                    case "cancelled":
                    case "ملغاة":
                        query = query.Where(r =>
                            r.Status == "Cancelled");
                        break;

                    default:
                        return BadRequest(new
                        {
                            message =
                                "حالة البحث غير صحيحة. استخدم all أو active أو completed أو cancelled."
                        });
                }
            }

            var orders = await query
                .OrderByDescending(r => r.CreatedAtUtc)
                .Select(r => new
                {
                    id = r.Id,
                    requestNumber = r.RequestNumber,
                    specialtyId = r.SpecialtyId,
                    specialtyName = r.Specialty.Name,
                    serviceTypeName = r.ServiceTypeName,
                    problemDescription = r.ProblemDescription,
                    additionalNotes = r.AdditionalNotes,
                    status = r.Status,
                    isAsSoonAsPossible = r.IsAsSoonAsPossible,
                    scheduledAtUtc = r.ScheduledAtUtc,
                    createdAtUtc = r.CreatedAtUtc,
                    updatedAtUtc = r.UpdatedAtUtc,
                    address = new
                    {
                        title = r.AddressTitle,
                        governorate = r.Governorate,
                        city = r.City,
                        district = r.District,
                        street = r.Street,
                        buildingNumber = r.BuildingNumber,
                        floorNumber = r.FloorNumber,
                        apartmentNumber = r.ApartmentNumber,
                        landmark = r.Landmark
                    }
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/Customer/orders/{id}
        [HttpGet("orders/{id:int}")]
        public async Task<IActionResult> GetOrderDetails(int id)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            var request = await _context.ServiceRequests
                .AsNoTracking()
                .Include(r => r.Specialty)
                .Include(r => r.Attachments)
                .SingleOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CustomerUserId == customerId);

            if (request == null)
            {
                return NotFound(new
                {
                    message =
                        "الطلب غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            var response = new
            {
                id = request.Id,
                requestNumber = request.RequestNumber,
                specialty = new
                {
                    id = request.SpecialtyId,
                    name = request.Specialty.Name,
                    nameEn = request.Specialty.NameEn
                },
                serviceTypeName = request.ServiceTypeName,
                problemDescription = request.ProblemDescription,
                additionalNotes = request.AdditionalNotes,
                status = request.Status,
                isAsSoonAsPossible = request.IsAsSoonAsPossible,
                scheduledAtUtc = request.ScheduledAtUtc,
                createdAtUtc = request.CreatedAtUtc,
                updatedAtUtc = request.UpdatedAtUtc,
                address = new
                {
                    title = request.AddressTitle,
                    governorate = request.Governorate,
                    city = request.City,
                    district = request.District,
                    street = request.Street,
                    buildingNumber = request.BuildingNumber,
                    floorNumber = request.FloorNumber,
                    apartmentNumber = request.ApartmentNumber,
                    landmark = request.Landmark,
                    latitude = request.Latitude,
                    longitude = request.Longitude
                },
                attachments = request.Attachments.Select(a => new
                {
                    id = a.Id,
                    originalFileName = a.OriginalFileName,
                    contentType = a.ContentType,
                    sizeBytes = a.SizeBytes,
                    createdAtUtc = a.CreatedAtUtc,
                    url =
                        $"/api/Customer/orders/{id}/attachments/{a.Id}"
                }).ToList()
            };

            return Ok(response);
        }

        // =========================================================
        // CANCEL CUSTOMER ORDER
        // =========================================================

        // PUT: api/Customer/orders/{id}/cancel
        [HttpPut("orders/{id:int}/cancel")]
        public async Task<IActionResult> CancelOrder(
            int id,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل من التوكن."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            var request = await _context.ServiceRequests
                .SingleOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CustomerUserId == customerId,
                    cancellationToken);

            if (request == null)
            {
                return NotFound(new
                {
                    message =
                        "الطلب غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            if (request.Status != "PendingOffers" &&
                request.Status != "OfferAccepted")
            {
                return BadRequest(new
                {
                    message =
                        "لا يمكن إلغاء الطلب بعد بدء تنفيذ الخدمة أو اكتمالها أو إلغائها."
                });
            }

            var now = DateTime.UtcNow;

            // Get the artisan who owns the accepted offer, if any.
            var acceptedArtisanUserId =
                await _context.ServiceOffers
                    .Where(o =>
                        o.ServiceRequestId == id &&
                        o.Status == "Accepted")
                    .Select(o => (int?)o.ArtisanUserId)
                    .FirstOrDefaultAsync(cancellationToken);

            // Update the request only if it is still in a cancellable state.
            // This protects against a concurrent start-work operation.
            var updatedRequests =
                await _context.ServiceRequests
                    .Where(r =>
                        r.Id == id &&
                        r.CustomerUserId == customerId &&
                        (r.Status == "PendingOffers" ||
                         r.Status == "OfferAccepted"))
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(r => r.Status, "Cancelled")
                            .SetProperty(r => r.UpdatedAtUtc, now),
                        cancellationToken);

            if (updatedRequests == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message =
                        "تعذر إلغاء الطلب لأنه تغيرت حالته بالفعل. حدّث بيانات الطلب وحاول مرة أخرى."
                });
            }

            // Withdraw pending and accepted offers related to this request.
            await _context.ServiceOffers
                .Where(o =>
                    o.ServiceRequestId == id &&
                    (o.Status == "Pending" ||
                     o.Status == "Accepted"))
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(o => o.Status, "Withdrawn")
                        .SetProperty(o => o.UpdatedAtUtc, now),
                    cancellationToken);

            // Notify the customer.
            var customerNotification = new Notification
            {
                UserId = customerId,
                Title = "تم إلغاء طلبك",
                Message =
                    $"تم إلغاء طلب الخدمة رقم {request.RequestNumber} بنجاح.",
                Type = "Order",
                IsRead = false,
                CreatedAtUtc = now,
                RelatedEntityType = "ServiceRequest",
                RelatedEntityId = request.Id
            };

            _context.Notifications.Add(customerNotification);

            // Notify the artisan who had the accepted offer, if one exists.
            Notification? artisanNotification = null;

            if (acceptedArtisanUserId.HasValue)
            {
                artisanNotification = new Notification
                {
                    UserId = acceptedArtisanUserId.Value,
                    Title = "تم إلغاء طلب الخدمة",
                    Message =
                        $"قام العميل بإلغاء طلب الخدمة رقم {request.RequestNumber} قبل بدء التنفيذ.",
                    Type = "Order",
                    IsRead = false,
                    CreatedAtUtc = now,
                    RelatedEntityType = "ServiceRequest",
                    RelatedEntityId = request.Id
                };

                _context.Notifications.Add(artisanNotification);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "تم إلغاء الطلب بنجاح.",
                order = new
                {
                    id = request.Id,
                    requestNumber = request.RequestNumber,
                    status = "Cancelled",
                    updatedAtUtc = now
                },
                notifications = new
                {
                    customer = new
                    {
                        id = customerNotification.Id,
                        title = customerNotification.Title,
                        message = customerNotification.Message
                    },
                    artisan = artisanNotification == null
                        ? null
                        : new
                        {
                            id = artisanNotification.Id,
                            title = artisanNotification.Title,
                            message = artisanNotification.Message
                        }
                }
            });
        }

        // =========================================================
        // CUSTOMER ORDER ATTACHMENTS
        // =========================================================

        // POST: api/Customer/orders/{id}/attachments
        [HttpPost("orders/{id:int}/attachments")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(65 * 1024 * 1024)]
        public async Task<IActionResult> UploadAttachments(
            int id,
            [FromForm] UploadServiceRequestAttachmentsDto input,
            CancellationToken cancellationToken)
        {
            var files = input?.Files;

            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب العميل."
                });
            }

            var customerIsActive = await _context.Users
                .AsNoTracking()
                .AnyAsync(u =>
                    u.Id == customerId &&
                    u.Role == "Customer" &&
                    u.AccountStatus == "Active",
                    cancellationToken);

            if (!customerIsActive)
            {
                return Forbid();
            }

            var request = await _context.ServiceRequests
                .SingleOrDefaultAsync(r =>
                    r.Id == id &&
                    r.CustomerUserId == customerId,
                    cancellationToken);

            if (request == null)
            {
                return NotFound(new
                {
                    message = "الطلب غير موجود."
                });
            }

            if (request.Status != "PendingOffers")
            {
                return BadRequest(new
                {
                    message =
                        "لا يمكن إضافة مرفقات بعد تغيير حالة الطلب."
                });
            }

            if (files == null ||
                files.Count == 0 ||
                files.Count > 5)
            {
                return BadRequest(new
                {
                    message =
                        "ارفع من ملف واحد إلى 5 ملفات في الطلب الواحد."
                });
            }

            const long maxTotalBytes = 60L * 1024 * 1024;
            const long maxImageBytes = 10L * 1024 * 1024;
            const long maxVideoBytes = 50L * 1024 * 1024;

            if (files.Any(f => f == null || f.Length <= 0) ||
                files.Sum(f => f.Length) > maxTotalBytes)
            {
                return BadRequest(new
                {
                    message =
                        "الملفات فارغة أو إجمالي حجمها أكبر من 60 ميجابايت."
                });
            }

            var existingCount =
                await _context.ServiceRequestAttachments.CountAsync(
                    a => a.ServiceRequestId == id,
                    cancellationToken);

            if (existingCount + files.Count > 10)
            {
                return BadRequest(new
                {
                    message =
                        "الحد الأقصى للمرفقات هو 10 ملفات لكل طلب."
                });
            }

            var validatedFiles =
                new List<(IFormFile File, string Extension)>();

            foreach (var file in files)
            {
                var originalPath = file.FileName.Replace('\\', '/');
                var originalName = Path.GetFileName(originalPath);

                var extension =
                    Path.GetExtension(originalName).ToLowerInvariant();

                var allowedTypes = GetAllowedContentTypes(extension);

                if (allowedTypes == null ||
                    !allowedTypes.Contains(
                        file.ContentType,
                        StringComparer.OrdinalIgnoreCase))
                {
                    return BadRequest(new
                    {
                        message = $"نوع الملف غير مسموح: {originalName}"
                    });
                }

                var isVideo =
                    extension == ".mp4" || extension == ".mov";

                var sizeLimit = isVideo
                    ? maxVideoBytes
                    : maxImageBytes;

                if (file.Length > sizeLimit)
                {
                    return BadRequest(new
                    {
                        message = isVideo
                            ? "الحد الأقصى لحجم الفيديو 50 ميجابايت."
                            : "الحد الأقصى لحجم الصورة 10 ميجابايت."
                    });
                }

                if (!await HasValidFileSignatureAsync(
                    file, extension, cancellationToken))
                {
                    return BadRequest(new
                    {
                        message =
                            $"محتوى الملف لا يطابق نوعه: {originalName}"
                    });
                }

                validatedFiles.Add((file, extension));
            }

            var storageDirectory = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "ServiceRequestAttachments",
                id.ToString());

            Directory.CreateDirectory(storageDirectory);

            var savedPaths = new List<string>();
            var records = new List<ServiceRequestAttachment>();
            var now = DateTime.UtcNow;

            try
            {
                foreach (var item in validatedFiles)
                {
                    var file = item.File;
                    var extension = item.Extension;

                    var storedFileName = $"{Guid.NewGuid():N}{extension}";

                    var fullPath = Path.Combine(
                        storageDirectory,
                        storedFileName);

                    await using (var output = new FileStream(
                        fullPath,
                        FileMode.CreateNew,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        await file.CopyToAsync(
                            output,
                            cancellationToken);
                    }

                    savedPaths.Add(fullPath);

                    var rawName = Path.GetFileName(
                        file.FileName.Replace('\\', '/'));

                    var safeOriginalName = new string(
                        rawName
                            .Where(c => !char.IsControl(c))
                            .Take(255)
                            .ToArray());

                    if (string.IsNullOrWhiteSpace(safeOriginalName))
                    {
                        safeOriginalName = "attachment" + extension;
                    }

                    records.Add(new ServiceRequestAttachment
                    {
                        ServiceRequestId = request.Id,
                        OriginalFileName = safeOriginalName,
                        StoredFileName = storedFileName,
                        ContentType = file.ContentType,
                        SizeBytes = file.Length,
                        CreatedAtUtc = now
                    });
                }

                _context.ServiceRequestAttachments.AddRange(records);

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                foreach (var path in savedPaths)
                {
                    try
                    {
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                    catch (IOException)
                    {
                        // Log cleanup failures in production.
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Log cleanup failures in production.
                    }
                }

                throw;
            }

            return StatusCode(201, new
            {
                message = "تم رفع المرفقات بنجاح.",
                requestId = request.Id,
                attachments = records.Select(a => new
                {
                    id = a.Id,
                    originalFileName = a.OriginalFileName,
                    contentType = a.ContentType,
                    sizeBytes = a.SizeBytes,
                    url =
                        $"/api/Customer/orders/{id}/attachments/{a.Id}"
                })
            });
        }

        // GET: api/Customer/orders/{id}/attachments/{attachmentId}
        [HttpGet("orders/{id:int}/attachments/{attachmentId:int}")]
        public async Task<IActionResult> GetAttachment(
            int id,
            int attachmentId,
            CancellationToken cancellationToken)
        {
            if (!TryGetCustomerId(out var customerId))
            {
                return Unauthorized();
            }

            var attachment =
                await _context.ServiceRequestAttachments
                    .AsNoTracking()
                    .Where(a =>
                        a.Id == attachmentId &&
                        a.ServiceRequestId == id &&
                        a.ServiceRequest.CustomerUserId == customerId)
                    .Select(a => new
                    {
                        a.StoredFileName,
                        a.ContentType
                    })
                    .SingleOrDefaultAsync(cancellationToken);

            if (attachment == null)
            {
                return NotFound(new
                {
                    message =
                        "المرفق غير موجود أو لا يمكنك الوصول إليه."
                });
            }

            var safeFileName =
                Path.GetFileName(attachment.StoredFileName);

            var fullPath = Path.Combine(
                _environment.ContentRootPath,
                "App_Data",
                "ServiceRequestAttachments",
                id.ToString(),
                safeFileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(new
                {
                    message = "الملف غير موجود في التخزين."
                });
            }

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Cache-Control"] = "private, no-store";

            var stream = new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return File(
                stream,
                attachment.ContentType,
                enableRangeProcessing: true);
        }

        // =========================================================
        // HELPERS
        // =========================================================

        private bool TryGetCustomerId(out int customerId)
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            return int.TryParse(userIdValue, out customerId);
        }

        private static string[]? GetAllowedContentTypes(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => new[] { "image/jpeg" },
                ".png" => new[] { "image/png" },
                ".webp" => new[] { "image/webp" },
                ".mp4" => new[] { "video/mp4" },
                ".mov" => new[] { "video/quicktime" },
                _ => null
            };
        }

        private static async Task<bool> HasValidFileSignatureAsync(
            IFormFile file,
            string extension,
            CancellationToken cancellationToken)
        {
            var header = new byte[12];
            var bytesRead = 0;

            await using var input = file.OpenReadStream();

            while (bytesRead < header.Length)
            {
                var read = await input.ReadAsync(
                    header.AsMemory(bytesRead),
                    cancellationToken);

                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
            }

            if (extension is ".jpg" or ".jpeg")
            {
                return bytesRead >= 3 &&
                       header[0] == 0xFF &&
                       header[1] == 0xD8 &&
                       header[2] == 0xFF;
            }

            if (extension == ".png")
            {
                byte[] signature =
                {
                    0x89, 0x50, 0x4E, 0x47,
                    0x0D, 0x0A, 0x1A, 0x0A
                };

                return bytesRead >= 8 &&
                       header.Take(8).SequenceEqual(signature);
            }

            if (extension == ".webp")
            {
                return bytesRead >= 12 &&
                       Encoding.ASCII.GetString(header, 0, 4) == "RIFF" &&
                       Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
            }

            if (extension is ".mp4" or ".mov")
            {
                return bytesRead >= 8 &&
                       Encoding.ASCII.GetString(header, 4, 4) == "ftyp";
            }

            return false;
        }
    }
}