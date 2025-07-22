namespace Filevoyage.com.Models
{
    public class UploadSuccessModel
    {
        // El short code que aparece en la URL corta
        public string ShortCode { get; set; }

        // URL completa para descargar
        public string DownloadUrl { get; set; }

        // Imagen de QR en formato data:image/png;base64,...
        public string QrImageDataUrl { get; set; }
    }
}
