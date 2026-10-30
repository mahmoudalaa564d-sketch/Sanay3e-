
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateServiceOfferDto
    {
        [Range(typeof(decimal), "0.01", "1000000000",
            ErrorMessage = "السعر يجب أن يكون أكبر من صفر.")]
        public decimal Price { get; set; }

        [StringLength(1000,
            ErrorMessage = "الرسالة لا يمكن أن تتجاوز 1000 حرف.")]
        public string? Message { get; set; }

        [Range(1, 10080,
            ErrorMessage = "وقت الوصول يجب أن يكون من دقيقة إلى 7 أيام.")]
        public int? EstimatedArrivalMinutes { get; set; }
    }
}
