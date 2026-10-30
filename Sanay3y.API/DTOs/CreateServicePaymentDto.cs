
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateServicePaymentDto
    {
        [Required(ErrorMessage = "طريقة الدفع مطلوبة.")]
        [RegularExpression(
            "(?i)^(Cash|Card|MobileWallet|Kiosk|Installments)$",
            ErrorMessage = "طريقة الدفع غير صحيحة.")]
        public string PaymentMethod { get; set; } = "Cash";
    }
}
