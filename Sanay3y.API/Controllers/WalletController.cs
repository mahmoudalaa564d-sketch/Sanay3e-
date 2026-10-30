
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sanay3y.API.Data;
using Sanay3y.API.Models;

namespace Sanay3y.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Customer,Artisan")]
    public class WalletController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Wallet/me
        // عرض رصيد المحفظة للمستخدم الحالي
        [HttpGet("me")]
        public async Task<IActionResult> GetMyWallet(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب المستخدم."
                });
            }

            var wallet = await GetOrCreateWalletAsync(
                userId,
                cancellationToken);

            return Ok(new
            {
                wallet.Id,
                wallet.UserId,
                wallet.Balance,
                wallet.Currency,
                wallet.CreatedAtUtc,
                wallet.UpdatedAtUtc
            });
        }

        // GET: api/Wallet/my-transactions
        // عرض سجل معاملات المحفظة للمستخدم الحالي
        [HttpGet("my-transactions")]
        public async Task<IActionResult> GetMyTransactions(
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out int userId))
            {
                return Unauthorized(new
                {
                    message = "تعذر تحديد حساب المستخدم."
                });
            }

            var wallet = await GetOrCreateWalletAsync(
                userId,
                cancellationToken);

            var transactions = await _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.CreatedAtUtc)
                .Select(t => new
                {
                    t.Id,
                    t.WalletId,
                    t.ServicePaymentId,
                    t.Type,
                    t.Direction,
                    t.Amount,
                    t.BalanceAfter,
                    t.Status,
                    t.Description,
                    t.Provider,
                    t.CreatedAtUtc
                })
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                walletId = wallet.Id,
                wallet.Currency,
                totalTransactions = transactions.Count,
                transactions
            });
        }

        // إنشاء محفظة برصيد صفر إذا لم تكن موجودة
        private async Task<Wallet> GetOrCreateWalletAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(
                    w => w.UserId == userId,
                    cancellationToken);

            if (wallet != null)
            {
                return wallet;
            }

            var now = DateTime.UtcNow;

            wallet = new Wallet
            {
                UserId = userId,
                Balance = 0m,
                Currency = "EGP",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            _context.Wallets.Add(wallet);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return wallet;
            }
            catch (DbUpdateException)
            {
                // قد يكون طلب متزامن أنشأ المحفظة بالفعل
                _context.Entry(wallet).State = EntityState.Detached;

                var existingWallet = await _context.Wallets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        w => w.UserId == userId,
                        cancellationToken);

                if (existingWallet != null)
                {
                    return existingWallet;
                }

                // لا نخفي أخطاء قاعدة البيانات الأخرى
                throw;
            }
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
