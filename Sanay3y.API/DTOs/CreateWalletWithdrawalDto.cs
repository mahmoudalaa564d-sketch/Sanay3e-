
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateWalletWithdrawalDto
    {
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage = "مبلغ السحب يجب أن يكون أكبر من صفر.")]
        public decimal Amount { get; set; }

        // MobileWallet أو BankTransfer
        [Required]
        [RegularExpression(
            "^(MobileWallet|BankTransfer)$",
            ErrorMessage = "طريقة السحب يجب أن تكون MobileWallet أو BankTransfer.")]
        public string PayoutMethod { get; set; } = "MobileWallet";

        // Vodafone Cash أو Orange Cash أو غيرها
        public string? PayoutProvider { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string AccountHolderName { get; set; } = string.Empty;

        // رقم المحفظة الإلكترونية أو رقم الحساب البنكي
        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string DestinationAccount { get; set; } = string.Empty;

        // مطلوب عند اختيار التحويل البنكي
        [StringLength(100)]
        public string? BankName { get; set; }
    }
}
