using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Sanay3y.API.DTOs
{
    public class UploadServiceRequestAttachmentsDto
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new();
    }
}
