
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    // بيانات الموافقة على طلب السحب
    public class ApproveWalletWithdrawalDto
    {
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // بيانات رفض طلب السحب
    public class RejectWalletWithdrawalDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // تأكيد تنفيذ التحويل للصنايعي
    public class CompleteWalletWithdrawalDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string TransferReference { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // تسجيل فشل التحويل
    public class FailWalletWithdrawalDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
