
using System;
using System.Data;
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
    [Authorize(Roles = "Admin")]
    public class AdminPlatformWithdrawalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminPlatformWithdrawalsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AdminPlatformWithdrawals/wallet
        // عرض رصيد وأرباح المنصة
        [HttpGet("wallet")]
        public async Task<IActionResult> GetPlatformWallet(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            var wallet = await _context.PlatformWallets
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    w => w.AccountCode == "MAIN",
                    cancellationToken);

            if (wallet == null)
            {
                return NotFound(new
                {
                    message = "Platform wallet MAIN was not found."
                });
            }

            var totalCollectedCommissions =
                await _context.PlatformWalletTransactions
                    .Where(t =>
                        t.PlatformWalletId == wallet.Id &&
                        t.Type == "CommissionCredit" &&
                        t.Status == "Completed")
                    .SumAsync(
                        t => (decimal?)t.Amount,
                        cancellationToken) ?? 0m;

            var totalPaidWithdrawals =
                await _context.PlatformWithdrawals
                    .Where(w =>
                        w.PlatformWalletId == wallet.Id &&
                        w.Status == "Paid")
                    .SumAsync(
                        w => (decimal?)w.Amount,
                        cancellationToken) ?? 0m;

            var reservedWithdrawals =
                await _context.PlatformWithdrawals
                    .Where(w =>
                        w.PlatformWalletId == wallet.Id &&
                        (w.Status == "Pending" ||
                         w.Status == "Approved"))
                    .SumAsync(
                        w => (decimal?)w.Amount,
                        cancellationToken) ?? 0m;

            return Ok(new
            {
                accountCode = wallet.AccountCode,
                balance = wallet.Balance,
                currency = wallet.Currency,
                totalCollectedCommissions,
                totalPaidWithdrawals,
                reservedWithdrawals,
                updatedAtUtc = wallet.UpdatedAtUtc
            });
        }

        // GET: api/AdminPlatformWithdrawals/transactions
        // عرض سجل معاملات محفظة المنصة
        [HttpGet("transactions")]
        public async Task<IActionResult> GetPlatformTransactions(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            var wallet = await _context.PlatformWallets
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    w => w.AccountCode == "MAIN",
                    cancellationToken);

            if (wallet == null)
            {
                return NotFound(new
                {
                    message = "Platform wallet MAIN was not found."
                });
            }

            var transactions = await _context.PlatformWalletTransactions
                .AsNoTracking()
                .Where(t => t.PlatformWalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Select(t => new
                {
                    t.Id,
                    t.PlatformCommissionId,
                    t.PlatformWithdrawalId,
                    t.Type,
                    t.Direction,
                    t.Amount,
                    t.BalanceAfter,
                    t.Status,
                    t.Description,
                    t.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                count = transactions.Count,
                transactions
            });
        }

        // GET: api/AdminPlatformWithdrawals/requests
        // عرض كل طلبات سحب أرباح المنصة
        [HttpGet("requests")]
        public async Task<IActionResult> GetAllWithdrawalRequests(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            return await GetWithdrawalsAsync(
                null,
                cancellationToken);
        }

        // GET: api/AdminPlatformWithdrawals/pending
        // عرض طلبات السحب المعلقة
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingWithdrawals(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            return await GetWithdrawalsAsync(
                "Pending",
                cancellationToken);
        }

        // GET: api/AdminPlatformWithdrawals/approved
        // عرض طلبات السحب الموافق عليها
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedWithdrawals(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            return await GetWithdrawalsAsync(
                "Approved",
                cancellationToken);
        }

        // GET: api/AdminPlatformWithdrawals/1
        // عرض تفاصيل طلب سحب محدد
        [HttpGet("{withdrawalId:int}")]
        public async Task<IActionResult> GetWithdrawalDetails(
            int withdrawalId,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            var withdrawal = await _context.PlatformWithdrawals
                .AsNoTracking()
                .Where(w => w.Id == withdrawalId)
                .Select(w => new
                {
                    withdrawalId = w.Id,
                    w.PlatformWalletId,

                    w.RequestedByAdminId,
                    requestedByAdminName = w.RequestedByAdmin.FullName,

                    w.ReviewedByAdminId,

                    w.Amount,
                    w.Currency,

                    w.PayoutMethod,
                    w.PayoutProvider,
                    w.AccountHolderName,

                    // Full destination is visible only through
                    // this Admin-authorized endpoint.
                    w.DestinationAccount,

                    w.BankName,
                    w.Status,
                    w.AdminNotes,
                    w.RejectionReason,
                    w.TransferReference,

                    w.RequestedAtUtc,
                    w.ReviewedAtUtc,
                    w.PaidAtUtc,
                    w.UpdatedAtUtc
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (withdrawal == null)
            {
                return NotFound(new
                {
                    message = "Platform withdrawal request was not found."
                });
            }

            return Ok(withdrawal);
        }

        // POST: api/AdminPlatformWithdrawals
        // إنشاء طلب لسحب أرباح المنصة
        [HttpPost]
        public async Task<IActionResult> CreateWithdrawal(
            [FromBody] CreatePlatformWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (dto.Amount <= 0 ||
                dto.Amount != decimal.Round(dto.Amount, 2))
            {
                return BadRequest(new
                {
                    message = "Withdrawal amount must be positive and have no more than two decimal places."
                });
            }

            var payoutMethod = dto.PayoutMethod?.Trim();

            if (payoutMethod == "MobileWallet")
            {
                if (string.IsNullOrWhiteSpace(dto.PayoutProvider))
                {
                    return BadRequest(new
                    {
                        message = "Mobile wallet provider is required."
                    });
                }

                if (dto.PayoutProvider.Trim().Length > 50)
                {
                    return BadRequest(new
                    {
                        message = "Mobile wallet provider name is too long."
                    });
                }
            }
            else if (payoutMethod == "BankTransfer")
            {
                if (string.IsNullOrWhiteSpace(dto.BankName))
                {
                    return BadRequest(new
                    {
                        message = "Bank name is required for bank transfers."
                    });
                }
            }
            else
            {
                return BadRequest(new
                {
                    message = "Unsupported payout method."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var wallet = await _context.PlatformWallets
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    w => w.AccountCode == "MAIN",
                    cancellationToken);

            if (wallet == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Platform wallet MAIN was not found."
                });
            }

            if (wallet.Currency != "EGP")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Platform wallet currency is not supported."
                });
            }

            // Reserve the amount immediately.
            // The conditional update prevents a negative balance.
            var debitedRows = await _context.PlatformWallets
                .Where(w =>
                    w.Id == wallet.Id &&
                    w.Balance >= dto.Amount)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(
                            w => w.Balance,
                            w => w.Balance - dto.Amount)
                        .SetProperty(
                            w => w.UpdatedAtUtc,
                            DateTime.UtcNow),
                    cancellationToken);

            if (debitedRows == 0)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Platform available balance is insufficient.",
                    availableBalance = wallet.Balance,
                    requestedAmount = dto.Amount,
                    currency = wallet.Currency
                });
            }

            var now = DateTime.UtcNow;
            var balanceAfter = wallet.Balance - dto.Amount;

            var withdrawal = new PlatformWithdrawal
            {
                PlatformWalletId = wallet.Id,
                RequestedByAdminId = adminId.Value,

                Amount = dto.Amount,
                Currency = wallet.Currency,

                PayoutMethod = payoutMethod!,

                PayoutProvider =
                    dto.PayoutProvider?.Trim(),

                AccountHolderName =
                    dto.AccountHolderName.Trim(),

                DestinationAccount =
                    dto.DestinationAccount.Trim(),

                BankName = dto.BankName?.Trim(),

                Status = "Pending",

                RequestedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.PlatformWithdrawals.Add(withdrawal);

            // Save the withdrawal to obtain its ID.
            await _context.SaveChangesAsync(cancellationToken);

            // Record the reservation in the platform ledger.
            var walletTransaction = new PlatformWalletTransaction
            {
                PlatformWalletId = wallet.Id,
                PlatformCommissionId = null,
                PlatformWithdrawalId = withdrawal.Id,

                Type = "WithdrawalDebit",
                Direction = "Debit",

                Amount = dto.Amount,
                BalanceAfter = balanceAfter,

                Status = "Completed",

                Description =
                    $"Platform earnings withdrawal request #{withdrawal.Id}",

                CreatedAtUtc = now
            };

            _context.PlatformWalletTransactions.Add(walletTransaction);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Platform withdrawal request created successfully.",
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,
                payoutMethod = withdrawal.PayoutMethod,
                status = withdrawal.Status,
                availableBalance = balanceAfter,
                destinationAccount =
                    MaskAccount(withdrawal.DestinationAccount),
                requestedAtUtc = withdrawal.RequestedAtUtc
            });
        }

        // POST: api/AdminPlatformWithdrawals/1/approve
        // الموافقة على طلب السحب
        [HttpPost("{withdrawalId:int}/approve")]
        public async Task<IActionResult> ApproveWithdrawal(
            int withdrawalId,
            [FromBody] ApprovePlatformWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.PlatformWithdrawals
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Platform withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Pending")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Only pending withdrawal requests can be approved.",
                    currentStatus = withdrawal.Status
                });
            }

            var now = DateTime.UtcNow;

            withdrawal.Status = "Approved";
            withdrawal.ReviewedByAdminId = adminId.Value;
            withdrawal.ReviewedAtUtc = now;
            withdrawal.UpdatedAtUtc = now;
            withdrawal.AdminNotes = dto.AdminNotes?.Trim();

            // The amount was already reserved when the request was created.
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Withdrawal request approved. Execute the actual transfer before marking it as paid.",
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,
                status = withdrawal.Status,
                reviewedAtUtc = withdrawal.ReviewedAtUtc
            });
        }

        // POST: api/AdminPlatformWithdrawals/1/reject
        // رفض الطلب وإرجاع المبلغ إلى محفظة المنصة
        [HttpPost("{withdrawalId:int}/reject")]
        public async Task<IActionResult> RejectWithdrawal(
            int withdrawalId,
            [FromBody] RejectPlatformWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new
                {
                    message = "A rejection reason is required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.PlatformWithdrawals
                .Include(w => w.PlatformWallet)
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Platform withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Pending")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Only pending withdrawal requests can be rejected.",
                    currentStatus = withdrawal.Status
                });
            }

            var now = DateTime.UtcNow;

            RefundWithdrawal(
                withdrawal,
                adminId.Value,
                "Rejected",
                dto.Reason.Trim(),
                dto.AdminNotes?.Trim(),
                now);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Withdrawal rejected and the reserved amount was returned to the platform wallet.",
                withdrawalId = withdrawal.Id,
                refundedAmount = withdrawal.Amount,
                walletBalance = withdrawal.PlatformWallet.Balance,
                status = withdrawal.Status
            });
        }

        // POST: api/AdminPlatformWithdrawals/1/complete
        // تسجيل التحويل الفعلي بعد تنفيذه
        [HttpPost("{withdrawalId:int}/complete")]
        public async Task<IActionResult> CompleteWithdrawal(
            int withdrawalId,
            [FromBody] CompletePlatformWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.TransferReference))
            {
                return BadRequest(new
                {
                    message = "Transfer reference is required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.PlatformWithdrawals
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Platform withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Approved")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Only approved withdrawal requests can be marked as paid.",
                    currentStatus = withdrawal.Status
                });
            }

            var now = DateTime.UtcNow;

            withdrawal.Status = "Paid";
            withdrawal.TransferReference =
                dto.TransferReference.Trim();

            withdrawal.ReviewedByAdminId = adminId.Value;
            withdrawal.AdminNotes = dto.AdminNotes?.Trim();

            withdrawal.PaidAtUtc = now;
            withdrawal.UpdatedAtUtc = now;

            // The platform balance was already reduced when
            // the withdrawal request was created.
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Platform withdrawal marked as paid.",
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,
                transferReference = withdrawal.TransferReference,
                status = withdrawal.Status,
                paidAtUtc = withdrawal.PaidAtUtc
            });
        }

        // POST: api/AdminPlatformWithdrawals/1/fail
        // تسجيل فشل التحويل وإرجاع المبلغ إلى محفظة المنصة
        [HttpPost("{withdrawalId:int}/fail")]
        public async Task<IActionResult> FailWithdrawal(
            int withdrawalId,
            [FromBody] FailPlatformWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new
                {
                    message = "A transfer failure reason is required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.PlatformWithdrawals
                .Include(w => w.PlatformWallet)
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Platform withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Approved")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "Only approved withdrawal requests can be marked as failed.",
                    currentStatus = withdrawal.Status
                });
            }

            var now = DateTime.UtcNow;

            RefundWithdrawal(
                withdrawal,
                adminId.Value,
                "Failed",
                dto.Reason.Trim(),
                dto.AdminNotes?.Trim(),
                now);

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Transfer failure recorded and the reserved amount was returned to the platform wallet.",
                withdrawalId = withdrawal.Id,
                refundedAmount = withdrawal.Amount,
                walletBalance = withdrawal.PlatformWallet.Balance,
                status = withdrawal.Status
            });
        }

        // Load withdrawal requests, optionally filtered by status.
        private async Task<IActionResult> GetWithdrawalsAsync(
            string? status,
            CancellationToken cancellationToken)
        {
            var query = _context.PlatformWithdrawals
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(w => w.Status == status);
            }

            var withdrawals = await query
                .OrderByDescending(w => w.RequestedAtUtc)
                .Select(w => new
                {
                    withdrawalId = w.Id,
                    w.RequestedByAdminId,
                    requesterName = w.RequestedByAdmin.FullName,
                    w.Amount,
                    w.Currency,
                    w.PayoutMethod,
                    w.PayoutProvider,
                    w.AccountHolderName,
                    w.DestinationAccount,
                    w.BankName,
                    w.Status,
                    w.RequestedAtUtc,
                    w.ReviewedAtUtc,
                    w.PaidAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = withdrawals.Select(w => new
            {
                w.withdrawalId,
                w.RequestedByAdminId,
                w.requesterName,
                w.Amount,
                w.Currency,
                w.PayoutMethod,
                w.PayoutProvider,
                w.AccountHolderName,
                destinationAccount = MaskAccount(w.DestinationAccount),
                w.BankName,
                w.Status,
                w.RequestedAtUtc,
                w.ReviewedAtUtc,
                w.PaidAtUtc
            });

            return Ok(new
            {
                count = withdrawals.Count,
                status,
                withdrawals = result
            });
        }

        // Return a reserved amount to the platform wallet.
        // This is used only for rejection or confirmed transfer failure.
        private void RefundWithdrawal(
            PlatformWithdrawal withdrawal,
            int adminId,
            string finalStatus,
            string reason,
            string? adminNotes,
            DateTime now)
        {
            withdrawal.PlatformWallet.Balance += withdrawal.Amount;
            withdrawal.PlatformWallet.UpdatedAtUtc = now;

            withdrawal.Status = finalStatus;
            withdrawal.ReviewedByAdminId = adminId;
            withdrawal.ReviewedAtUtc = now;
            withdrawal.UpdatedAtUtc = now;

            withdrawal.RejectionReason = reason;
            withdrawal.AdminNotes = adminNotes;

            var refundTransaction = new PlatformWalletTransaction
            {
                PlatformWalletId = withdrawal.PlatformWalletId,
                PlatformCommissionId = null,
                PlatformWithdrawalId = withdrawal.Id,

                Type = "WithdrawalRefund",
                Direction = "Credit",

                Amount = withdrawal.Amount,
                BalanceAfter = withdrawal.PlatformWallet.Balance,

                Status = "Completed",

                Description =
                    $"Refund for platform withdrawal #{withdrawal.Id}. Reason: {reason}",

                CreatedAtUtc = now
            };

            _context.PlatformWalletTransactions.Add(refundTransaction);
        }

        // Verify that the current user is an active admin.
        private async Task<int?> GetActiveAdminIdAsync(
            CancellationToken cancellationToken)
        {
            var idValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (!int.TryParse(idValue, out int adminId))
            {
                return null;
            }

            return await _context.Users
                .AsNoTracking()
                .Where(u =>
                    u.Id == adminId &&
                    u.Role == "Admin" &&
                    u.AccountStatus == "Active")
                .Select(u => (int?)u.Id)
                .SingleOrDefaultAsync(cancellationToken);
        }

        // Mask payout destination in lists.
        private static string MaskAccount(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
                return string.Empty;

            var value = account.Trim();

            if (value.Length <= 4)
                return new string('*', value.Length);

            return new string('*', value.Length - 4)
                + value[^4..];
        }
    }
}
