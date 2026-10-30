
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    // Admin creates a platform earnings withdrawal request
    public class CreatePlatformWithdrawalDto
    {
        [Range(
            typeof(decimal),
            "0.01",
            "9999999999999999.99",
            ErrorMessage = "Withdrawal amount must be greater than zero.")]
        public decimal Amount { get; set; }

        // MobileWallet or BankTransfer
        [Required]
        [RegularExpression(
            "^(MobileWallet|BankTransfer)$",
            ErrorMessage = "Payout method must be MobileWallet or BankTransfer.")]
        public string PayoutMethod { get; set; } = "BankTransfer";

        public string? PayoutProvider { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string AccountHolderName { get; set; } = string.Empty;

        [Required]
        [StringLength(200, MinimumLength = 5)]
        public string DestinationAccount { get; set; } = string.Empty;

        [StringLength(100)]
        public string? BankName { get; set; }
    }

    // Admin approves a withdrawal request
    public class ApprovePlatformWithdrawalDto
    {
        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // Admin rejects a withdrawal request
    public class RejectPlatformWithdrawalDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // Admin confirms that the external transfer was completed
    public class CompletePlatformWithdrawalDto
    {
        [Required]
        [StringLength(150, MinimumLength = 3)]
        public string TransferReference { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }

    // Admin records a failed external transfer
    public class FailPlatformWithdrawalDto
    {
        [Required]
        [StringLength(500, MinimumLength = 3)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500)]
        public string? AdminNotes { get; set; }
    }
}
