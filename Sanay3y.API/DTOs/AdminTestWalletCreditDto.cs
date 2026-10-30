
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class AdminTestWalletCreditDto
    {
        [Range(
            typeof(decimal),
            "1.00",
            "10000.00",
            ErrorMessage = "المبلغ يجب أن يكون بين 1 و10000 جنيه.")]
        public decimal Amount { get; set; }
    }
}
