
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateWalletTopUpDto
    {
        [Range(
            typeof(decimal),
            "1.00",
            "10000.00",
            ErrorMessage = "مبلغ الشحن يجب أن يكون من 1 إلى 10000 جنيه.")]
        public decimal Amount { get; set; }
    }
}
