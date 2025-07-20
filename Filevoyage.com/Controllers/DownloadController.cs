// File: Controllers/DownloadController.cs
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

        // Página que muestra el botón / mensaje
        [HttpGet("{shortCode}")]
        public async Task<IActionResult> DownloadPage(string shortCode)
        {
            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null) return NotFound();

            ViewBag.Filename = meta.Filename;
            ViewBag.ShortCode = shortCode;
            return View("Index");
        }

        // Endpoint que hace el streaming real
        [HttpGet("/download/{shortCode}")]
        public async Task<IActionResult> StreamDownload(string shortCode)
        {
            var meta = await _cosmos.GetItemByIdAsync(shortCode);
            if (meta == null) return NotFound();

            // solo el nombre de blob (sin ruta completa)
            var blobName = Path.GetFileName(meta.DownloadUrl);
            var result = await _storage.DownloadFileStreamAsync(blobName);

            if (result == null) return NotFound();

            var (stream, contentType) = result.Value;
            return File(stream, contentType, meta.Filename);
        }
    }
}
