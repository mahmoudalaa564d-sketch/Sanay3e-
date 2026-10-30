
using System.ComponentModel.DataAnnotations;

namespace Sanay3y.API.DTOs
{
    public class CreateServiceReviewDto
    {
        [Range(1, 5, ErrorMessage = "التقييم لازم يكون من 1 إلى 5.")]
        public int Rating { get; set; }

        [StringLength(
            1000,
            ErrorMessage = "التعليق لا يمكن أن يزيد عن 1000 حرف.")]
        public string? Comment { get; set; }
    }
}
