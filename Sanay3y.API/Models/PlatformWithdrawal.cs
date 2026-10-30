
using System;

namespace Sanay3y.API.Models
{
    public class PlatformWithdrawal
    {
        public int Id { get; set; }

        // محفظة المنصة التي سيتم السحب منها
        public int PlatformWalletId { get; set; }

        // الأدمن الذي أنشأ طلب السحب
        public int RequestedByAdminId { get; set; }

        // الأدمن الذي راجع الطلب
        public int? ReviewedByAdminId { get; set; }

        // مبلغ السحب
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "EGP";

        // MobileWallet أو BankTransfer
        public string PayoutMethod { get; set; } = "BankTransfer";

        // اسم البنك أو مزود المحفظة الإلكترونية
        public string? PayoutProvider { get; set; }

        // اسم صاحب الحساب
        public string AccountHolderName { get; set; } = string.Empty;

        // رقم الحساب البنكي أو المحفظة الإلكترونية
        public string DestinationAccount { get; set; } = string.Empty;

        // اسم البنك عند اختيار التحويل البنكي
        public string? BankName { get; set; }

        // Pending, Approved, Rejected, Paid, Failed
        public string Status { get; set; } = "Pending";

        // ملاحظات الأدمن
        public string? AdminNotes { get; set; }

        // سبب الرفض أو فشل التحويل
        public string? RejectionReason { get; set; }

        // رقم مرجع التحويل الفعلي
        public string? TransferReference { get; set; }

        public DateTime RequestedAtUtc { get; set; }
            = DateTime.UtcNow;

        public DateTime? ReviewedAtUtc { get; set; }

        public DateTime? PaidAtUtc { get; set; }

        public DateTime UpdatedAtUtc { get; set; }
            = DateTime.UtcNow;

        // العلاقات
        public PlatformWallet PlatformWallet { get; set; } = null!;

        public User RequestedByAdmin { get; set; } = null!;

        public User? ReviewedByAdmin { get; set; }
    }
}
