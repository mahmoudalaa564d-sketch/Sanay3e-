
using System;

namespace Sanay3y.API.Models
{
    public class PlatformWalletTransaction
    {
        public int Id { get; set; }

        // محفظة المنصة
        public int PlatformWalletId { get; set; }

        // العمولة التي نتجت عنها الإضافة، إن وجدت
        public int? PlatformCommissionId { get; set; }

        // طلب سحب أرباح المنصة، إن وجد
        public int? PlatformWithdrawalId { get; set; }

        // CommissionCredit أو WithdrawalDebit أو WithdrawalRefund
        public string Type { get; set; } = "CommissionCredit";

        // Credit = إضافة، Debit = خصم
        public string Direction { get; set; } = "Credit";

        public decimal Amount { get; set; }

        // رصيد المنصة بعد تنفيذ العملية
        public decimal BalanceAfter { get; set; }

        // Pending أو Completed أو Failed أو Cancelled
        public string Status { get; set; } = "Completed";

        public string? Description { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // العلاقة بمحفظة المنصة
        public PlatformWallet PlatformWallet { get; set; } = null!;
    }
}
