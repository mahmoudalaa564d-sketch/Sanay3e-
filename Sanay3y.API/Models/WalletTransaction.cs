
using System;

namespace Sanay3y.API.Models
{
    public class WalletTransaction
    {
        public int Id { get; set; }

        public int WalletId { get; set; }

        public int? ServicePaymentId { get; set; }

        // طلب السحب المرتبط بالمعاملة
        public int? WalletWithdrawalId { get; set; }

        // TopUp أو ServicePaymentDebit أو
        // ServicePaymentCredit أو RefundCredit أو
        // WithdrawalDebit أو WithdrawalRefund
        public string Type { get; set; } = "TopUp";

        // Credit = إضافة، Debit = خصم
        public string Direction { get; set; } = "Credit";

        public decimal Amount { get; set; }

        // الرصيد بعد تنفيذ العملية
        public decimal BalanceAfter { get; set; }

        public string Status { get; set; } = "Pending";

        public string? Description { get; set; }

        public string? Provider { get; set; }

        public string? ProviderTransactionId { get; set; }

        public DateTime CreatedAtUtc { get; set; }
            = DateTime.UtcNow;

        public Wallet Wallet { get; set; } = null!;

        public ServicePayment? ServicePayment { get; set; }

        // العلاقة بطلب السحب
        public WalletWithdrawal? WalletWithdrawal { get; set; }
    }
}
