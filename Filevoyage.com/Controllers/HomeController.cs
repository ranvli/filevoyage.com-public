// Controllers/HomeController.cs
using Filevoyage.com.Models;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;

namespace Filevoyage.com.Controllers
{
    public class HomeController : Controller
    {
        private readonly AzureStorageService _storage;
        private readonly CosmosDbService _cosmos;

        public HomeController(AzureStorageService storage,
                              CosmosDbService cosmosDb)
        {
            _storage = storage;
            _cosmos = cosmosDb;
        }

        [HttpGet("/")]
        public IActionResult Index()
        {
            var model = new UploadModel
            {
                ExpirationDate = DateTime.UtcNow.AddDays(3)
            };
            return View(model);
        }

        [HttpPost("/")]
        public async Task<IActionResult> Index(UploadModel model)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View(model);
            }

            // 1) Subir a blob
            var uniqueName = Guid.NewGuid() + Path.GetExtension(model.File.FileName);
            await using var stream = model.File.OpenReadStream();
            await _storage.UploadFileAsync(uniqueName, stream);

            // 2) Guardar metadatos en Cosmos
            var shortCode = await Nanoid.GenerateAsync(size: 6);
            var partitionKey = shortCode.Substring(0, 2);

            var meta = new FileMetadata
            {
                Id = shortCode,
                PartitionKey = partitionKey,
                Filename = model.File.FileName,
                DownloadUrl = uniqueName,     // guardamos solo el nombre del blob
                Size = model.File.Length,
                ContentType = model.File.ContentType!,
                UploadDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),

                // ✚ nuevos:
                ExpirationDate = model.ExpirationDate ?? DateTime.UtcNow.AddDays(3),
                MaxDownloads = model.MaxDownloads,
                DownloadCount = 0,
                ProtectWithQR = model.ProtectWithQR
            };

            await _cosmos.AddItemAsync(meta);

            // 3) Mostrar pantalla de éxito con el shortCode
            return View("Success", new UploadSuccessModel { ShortCode = shortCode });
        }
    }
}
