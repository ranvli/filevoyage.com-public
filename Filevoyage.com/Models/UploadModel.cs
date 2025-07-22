// Models/UploadModel.cs
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

        [Display(Name = "Max Downloads (1–5)")]
        [Range(1, 5)]
        public int MaxDownloads { get; set; } = 1;

        // (se usa solo para mostrar el link inmediato)
        public string? DownloadUrl { get; set; }
    }
}
