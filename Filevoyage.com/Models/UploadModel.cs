using System.ComponentModel.DataAnnotations;

namespace Filevoyage.com.Models
{
    public class UploadModel
    {
        [Required]
        [Display(Name = "File")]
        public IFormFile File { get; set; }

        [Display(Name = "Expiration Date")]
        public DateTime? ExpirationDate { get; set; }

        [Display(Name = "Protect with QR")]
        public bool ProtectWithQR { get; set; }
        public string? DownloadUrl { get; set; }
    }
}
