
using System;

namespace Sanay3y.API.Models
{
    public class WalletWithdrawal
    {
        public int Id { get; set; }

        // المحفظة التي سيتم السحب منها
        public int WalletId { get; set; }

        // مبلغ السحب
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        // MobileWallet أو BankTransfer
        public string PayoutMethod { get; set; } = "MobileWallet";

        // مثال: Vodafone Cash أو البنك الأهلي
        public string? PayoutProvider { get; set; }

        // اسم صاحب وسيلة استلام الأموال
        public string AccountHolderName { get; set; } = string.Empty;

        // رقم المحفظة الإلكترونية أو رقم الحساب / IBAN
        public string DestinationAccount { get; set; } = string.Empty;

        // اسم البنك عند اختيار التحويل البنكي
        public string? BankName { get; set; }

        // Pending, Approved, Rejected, Paid, Failed, Cancelled
        public string Status { get; set; } = "Pending";

        // ملاحظات الأدمن
        public string? AdminNotes { get; set; }

        // سبب رفض أو فشل السحب
        public string? RejectionReason { get; set; }

        // مرجع التحويل بعد تنفيذه
        public string? TransferReference { get; set; }

        // الأدمن الذي راجع الطلب
        public int? ReviewedByAdminId { get; set; }

        public DateTime RequestedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? ReviewedAtUtc { get; set; }

        public DateTime? PaidAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // العلاقات
        public Wallet Wallet { get; set; } = null!;

        public User? ReviewedByAdmin { get; set; }
    }
}
