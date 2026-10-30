
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
    [Authorize(Roles = "Admin")]
    public class AdminWalletWithdrawalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminWalletWithdrawalsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AdminWalletWithdrawals/pending
        // Get withdrawal requests waiting for Admin review.
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingWithdrawals(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            return await GetWithdrawalsByStatusAsync(
                "Pending",
                cancellationToken);
        }

        // GET: api/AdminWalletWithdrawals/approved
        // Get approved requests waiting for the actual transfer.
        [HttpGet("approved")]
        public async Task<IActionResult> GetApprovedWithdrawals(
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            return await GetWithdrawalsByStatusAsync(
                "Approved",
                cancellationToken);
        }

        // GET: api/AdminWalletWithdrawals/1
        // Get full details of a specific withdrawal request.
        [HttpGet("{withdrawalId:int}")]
        public async Task<IActionResult> GetWithdrawalDetails(
            int withdrawalId,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            var withdrawal = await _context.WalletWithdrawals
                .AsNoTracking()
                .Where(w => w.Id == withdrawalId)
                .Select(w => new
                {
                    withdrawalId = w.Id,

                    userId = w.Wallet.UserId,
                    userName = w.Wallet.User.FullName,
                    userRole = w.Wallet.User.Role,

                    w.Amount,
                    w.Currency,

                    w.PayoutMethod,
                    w.PayoutProvider,
                    w.AccountHolderName,
                    w.DestinationAccount,
                    w.BankName,

                    w.Status,
                    w.AdminNotes,
                    w.RejectionReason,
                    w.TransferReference,

                    w.ReviewedByAdminId,
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
                    message = "Withdrawal request was not found."
                });
            }

            // Full payout details are available to authorized Admins only.
            return Ok(withdrawal);
        }

        // POST: api/AdminWalletWithdrawals/1/approve
        // Approve a pending withdrawal request.
        [HttpPost("{withdrawalId:int}/approve")]
        public async Task<IActionResult> ApproveWithdrawal(
            int withdrawalId,
            [FromBody] ApproveWalletWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.WalletWithdrawals
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Pending")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "This withdrawal request cannot be approved in its current state.",
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
            // Do not deduct it again.
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Withdrawal request approved. Complete the actual transfer before marking it as paid.",
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,
                status = withdrawal.Status,
                reviewedAtUtc = withdrawal.ReviewedAtUtc
            });
        }

        // POST: api/AdminWalletWithdrawals/1/reject
        // Reject a pending request and return the reserved amount.
        [HttpPost("{withdrawalId:int}/reject")]
        public async Task<IActionResult> RejectWithdrawal(
            int withdrawalId,
            [FromBody] RejectWalletWithdrawalDto dto,
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
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.WalletWithdrawals
                .Include(w => w.Wallet)
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Pending")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "This withdrawal request cannot be rejected in its current state.",
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
                message = "Withdrawal request rejected. The reserved amount has been returned to the user's wallet.",
                withdrawalId = withdrawal.Id,
                refundedAmount = withdrawal.Amount,
                walletBalance = withdrawal.Wallet.Balance,
                status = withdrawal.Status
            });
        }

        // POST: api/AdminWalletWithdrawals/1/complete
        // Confirm that the actual transfer has been completed.
        [HttpPost("{withdrawalId:int}/complete")]
        public async Task<IActionResult> CompleteWithdrawal(
            int withdrawalId,
            [FromBody] CompleteWalletWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.TransferReference))
            {
                return BadRequest(new
                {
                    message = "A transfer reference is required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.WalletWithdrawals
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Approved")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "The withdrawal request must be approved before confirming the transfer.",
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

            // The wallet amount was already reserved.
            // This operation only records confirmation of the actual transfer.
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Withdrawal request has been marked as paid.",
                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,
                transferReference = withdrawal.TransferReference,
                status = withdrawal.Status,
                paidAtUtc = withdrawal.PaidAtUtc
            });
        }

        // POST: api/AdminWalletWithdrawals/1/fail
        // Record a failed transfer and return the reserved amount.
        [HttpPost("{withdrawalId:int}/fail")]
        public async Task<IActionResult> FailWithdrawal(
            int withdrawalId,
            [FromBody] FailWalletWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            var adminId = await GetActiveAdminIdAsync(cancellationToken);

            if (adminId == null)
                return Forbid();

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new
                {
                    message = "A reason for the failed transfer is required."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    System.Data.IsolationLevel.Serializable,
                    cancellationToken);

            var withdrawal = await _context.WalletWithdrawals
                .Include(w => w.Wallet)
                .FirstOrDefaultAsync(
                    w => w.Id == withdrawalId,
                    cancellationToken);

            if (withdrawal == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return NotFound(new
                {
                    message = "Withdrawal request was not found."
                });
            }

            if (withdrawal.Status != "Approved")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "This withdrawal request cannot be marked as failed in its current state.",
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
                message = "The transfer was marked as failed. The reserved amount has been returned to the user's wallet.",
                withdrawalId = withdrawal.Id,
                refundedAmount = withdrawal.Amount,
                walletBalance = withdrawal.Wallet.Balance,
                status = withdrawal.Status
            });
        }

        // Get withdrawal requests by status.
        // This includes requests submitted by both Customers and Artisans.
        private async Task<IActionResult> GetWithdrawalsByStatusAsync(
            string status,
            CancellationToken cancellationToken)
        {
            var withdrawals = await _context.WalletWithdrawals
                .AsNoTracking()
                .Where(w => w.Status == status)
                .OrderBy(w => w.RequestedAtUtc)
                .Select(w => new
                {
                    withdrawalId = w.Id,

                    userId = w.Wallet.UserId,
                    userName = w.Wallet.User.FullName,
                    userRole = w.Wallet.User.Role,

                    w.Amount,
                    w.Currency,
                    w.PayoutMethod,
                    w.PayoutProvider,
                    w.DestinationAccount,
                    w.BankName,

                    w.Status,
                    w.RequestedAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = withdrawals.Select(w => new
            {
                w.withdrawalId,

                w.userId,
                w.userName,
                w.userRole,

                w.Amount,
                w.Currency,
                w.PayoutMethod,
                w.PayoutProvider,

                // Show only the last four characters in the list.
                destinationAccount = MaskAccount(w.DestinationAccount),

                w.BankName,
                w.Status,
                w.RequestedAtUtc
            });

            return Ok(new
            {
                count = withdrawals.Count,
                status,
                withdrawals = result
            });
        }

        // Return the reserved withdrawal amount to the user's wallet.
        // This is used only when a request is rejected or a transfer fails.
        private void RefundWithdrawal(
            WalletWithdrawal withdrawal,
            int adminId,
            string finalStatus,
            string reason,
            string? adminNotes,
            DateTime now)
        {
            withdrawal.Wallet.Balance += withdrawal.Amount;
            withdrawal.Wallet.UpdatedAtUtc = now;

            withdrawal.Status = finalStatus;
            withdrawal.ReviewedByAdminId = adminId;
            withdrawal.ReviewedAtUtc = now;
            withdrawal.UpdatedAtUtc = now;

            withdrawal.RejectionReason = reason;
            withdrawal.AdminNotes = adminNotes;

            // Record the returned amount in the wallet transaction history.
            var refundTransaction = new WalletTransaction
            {
                WalletId = withdrawal.WalletId,
                WalletWithdrawalId = withdrawal.Id,

                Type = "WithdrawalRefund",
                Direction = "Credit",

                Amount = withdrawal.Amount,
                BalanceAfter = withdrawal.Wallet.Balance,

                Status = "Completed",

                Description =
                    $"Returned amount for withdrawal request #{withdrawal.Id}. Reason: {reason}",

                Provider = "InternalWallet",
                CreatedAtUtc = now
            };

            _context.WalletTransactions.Add(refundTransaction);
        }

        // Verify that the current Admin account is active.
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

        // Mask the destination account in lists.
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
