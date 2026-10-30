
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
    [Authorize(Roles = "Customer,Artisan")]
    public class WalletWithdrawalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WalletWithdrawalsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/WalletWithdrawals
        // Customer or Artisan requests a withdrawal from their own wallet.
        [HttpPost]
        public async Task<IActionResult> CreateWithdrawal(
            [FromBody] CreateWalletWithdrawalDto dto,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new
                {
                    message = "Unable to identify your account."
                });
            }

            // Validate the requested amount.
            if (dto.Amount <= 0 ||
                dto.Amount != decimal.Round(dto.Amount, 2))
            {
                return BadRequest(new
                {
                    message = "The withdrawal amount must be greater than zero and have no more than two decimal places."
                });
            }

            // Validate account holder and destination details.
            if (string.IsNullOrWhiteSpace(dto.AccountHolderName) ||
                string.IsNullOrWhiteSpace(dto.DestinationAccount))
            {
                return BadRequest(new
                {
                    message = "Account holder name and destination account are required."
                });
            }

            // Validate the payout method.
            if (dto.PayoutMethod == "MobileWallet")
            {
                if (string.IsNullOrWhiteSpace(dto.PayoutProvider))
                {
                    return BadRequest(new
                    {
                        message = "Please specify the mobile wallet provider."
                    });
                }

                if (dto.PayoutProvider.Trim().Length > 50)
                {
                    return BadRequest(new
                    {
                        message = "The mobile wallet provider name is too long."
                    });
                }
            }
            else if (dto.PayoutMethod == "BankTransfer")
            {
                if (string.IsNullOrWhiteSpace(dto.BankName))
                {
                    return BadRequest(new
                    {
                        message = "Please specify the bank name."
                    });
                }
            }
            else
            {
                return BadRequest(new
                {
                    message = "The selected payout method is not supported."
                });
            }

            // Verify that the account is active and has an allowed role.
            var account = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Role,
                    u.AccountStatus,
                    ArtisanApprovalStatus = u.ArtisanProfile != null
                        ? u.ArtisanProfile.ApprovalStatus
                        : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (account == null ||
                account.AccountStatus != "Active")
            {
                return Forbid();
            }

            if (account.Role != "Customer" &&
                account.Role != "Artisan")
            {
                return Forbid();
            }

            // Artisans must be approved.
            // Customers only need to have an active account.
            if (account.Role == "Artisan" &&
                account.ArtisanApprovalStatus != "Approved")
            {
                return Forbid();
            }

            // Reserve the requested amount and create the withdrawal
            // and wallet transaction within one database transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    w => w.UserId == userId,
                    cancellationToken);

            if (wallet == null)
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "No wallet is linked to your account. Please open your wallet first."
                });
            }

            if (wallet.Currency != "EGP")
            {
                await transaction.RollbackAsync(cancellationToken);

                return Conflict(new
                {
                    message = "The wallet currency is not supported for withdrawals."
                });
            }

            // Calculate the expected balance after reserving the amount.
            var newBalance = wallet.Balance - dto.Amount;

            // Atomically deduct the amount only if sufficient funds exist.
            var debitedRows = await _context.Wallets
                .Where(w =>
                    w.Id == wallet.Id &&
                    w.UserId == userId &&
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
                    message = "Your available wallet balance is insufficient.",
                    availableBalance = wallet.Balance,
                    requestedAmount = dto.Amount,
                    currency = wallet.Currency
                });
            }

            var now = DateTime.UtcNow;

            // Create the withdrawal request.
            var withdrawal = new WalletWithdrawal
            {
                WalletId = wallet.Id,
                Amount = dto.Amount,
                Currency = wallet.Currency,

                PayoutMethod = dto.PayoutMethod,
                PayoutProvider = dto.PayoutProvider?.Trim(),

                AccountHolderName = dto.AccountHolderName.Trim(),
                DestinationAccount = dto.DestinationAccount.Trim(),
                BankName = dto.BankName?.Trim(),

                Status = "Pending",

                RequestedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.WalletWithdrawals.Add(withdrawal);

            // Save to generate the withdrawal ID.
            await _context.SaveChangesAsync(cancellationToken);

            // Record the wallet balance reservation.
            var walletTransaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                WalletWithdrawalId = withdrawal.Id,

                Type = "WithdrawalDebit",
                Direction = "Debit",

                Amount = dto.Amount,
                BalanceAfter = newBalance,

                Status = "Completed",

                Description =
                    $"Reserved amount for withdrawal request #{withdrawal.Id}",

                Provider = "InternalWallet",
                CreatedAtUtc = now
            };

            _context.WalletTransactions.Add(walletTransaction);

            await _context.SaveChangesAsync(cancellationToken);

            // Commit the wallet deduction and request together.
            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Your withdrawal request was submitted successfully and is waiting for Admin review.",

                withdrawalId = withdrawal.Id,
                amount = withdrawal.Amount,
                currency = withdrawal.Currency,

                payoutMethod = withdrawal.PayoutMethod,
                payoutProvider = withdrawal.PayoutProvider,

                destinationAccount =
                    MaskAccount(withdrawal.DestinationAccount),

                status = withdrawal.Status,

                availableBalance = newBalance,

                requestedAtUtc = withdrawal.RequestedAtUtc
            });
        }

        // GET: api/WalletWithdrawals/my-requests
        // Return withdrawal requests belonging to the current user only.
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyWithdrawals(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new
                {
                    message = "Unable to identify your account."
                });
            }

            var withdrawals = await _context.WalletWithdrawals
                .AsNoTracking()
                .Where(w => w.Wallet.UserId == userId)
                .OrderByDescending(w => w.RequestedAtUtc)
                .Select(w => new
                {
                    w.Id,
                    w.Amount,
                    w.Currency,
                    w.PayoutMethod,
                    w.PayoutProvider,
                    w.DestinationAccount,
                    w.BankName,
                    w.Status,
                    w.AdminNotes,
                    w.RejectionReason,
                    w.TransferReference,
                    w.RequestedAtUtc,
                    w.ReviewedAtUtc,
                    w.PaidAtUtc
                })
                .ToListAsync(cancellationToken);

            var result = withdrawals.Select(w => new
            {
                withdrawalId = w.Id,
                amount = w.Amount,
                currency = w.Currency,
                payoutMethod = w.PayoutMethod,
                payoutProvider = w.PayoutProvider,

                // Never expose the full destination account.
                destinationAccount = MaskAccount(w.DestinationAccount),

                bankName = w.BankName,
                status = w.Status,
                adminNotes = w.AdminNotes,
                rejectionReason = w.RejectionReason,
                transferReference = w.TransferReference,
                requestedAtUtc = w.RequestedAtUtc,
                reviewedAtUtc = w.ReviewedAtUtc,
                paidAtUtc = w.PaidAtUtc
            });

            return Ok(new
            {
                count = withdrawals.Count,
                withdrawals = result
            });
        }

        // Mask most of the mobile wallet number or bank account number.
        private static string MaskAccount(string account)
        {
            if (string.IsNullOrWhiteSpace(account))
            {
                return string.Empty;
            }

            var value = account.Trim();

            if (value.Length <= 4)
            {
                return new string('*', value.Length);
            }

            return new string('*', value.Length - 4)
                + value[^4..];
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
