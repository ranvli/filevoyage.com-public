// Controllers/DownloadController.cs
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;

namespace Filevoyage.com.Controllers
{
    public class DownloadController : Controller
    {
        private readonly AzureStorageService _storage;
        private readonly CosmosDbService _cosmos;

        public DownloadController(AzureStorageService storage,
                                  CosmosDbService cosmosDb)
        {
            _storage = storage;
            _cosmos = cosmosDb;
        }

        // 1) Página con botón / QR / mensaje
        [HttpGet("/{shortCode}")]
        public async Task<IActionResult> DownloadPage(string shortCode)
        {
            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null) return NotFound();

            // ✚ chequeo de expiración
            if (DateTime.UtcNow > meta.ExpirationDate)
                return View("Expired");

            // ✚ chequeo de contador
            if (meta.DownloadCount >= meta.MaxDownloads)
                return View("Expired");

            ViewBag.Filename = meta.Filename;
            ViewBag.ShortCode = shortCode;
            ViewBag.ProtectWithQR = meta.ProtectWithQR;
            return View("Download");
        }

        [HttpGet("/download/{shortCode}")]
        public async Task<IActionResult> StreamDownload(string shortCode)
        {
            // 1) Trae metadata
            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null) return NotFound();

            // 2) Comprueba límite
            if (meta.DownloadsCount >= meta.MaxDownloads)
            {
                // Puedes devolver 403, o una vista “Expired”
                return Forbid($"This link has reached its max of {meta.MaxDownloads} downloads.");
            }

            // 3) Incrementa contador
            await _cosmos.IncrementDownloadCountAsync(shortCode);
            ViewBag.Remaining = meta.MaxDownloads - meta.DownloadsCount;

            // 4) Recupera blob y transmite
            var blobName = Path.GetFileName(meta.DownloadUrl);
            var result = await _storage.DownloadFileStreamAsync(blobName);
            if (result == null) return NotFound();

            var (stream, contentType) = result.Value;
            return File(stream, contentType, meta.Filename);
        }
    }
}
