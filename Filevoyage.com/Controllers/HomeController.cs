using Filevoyage.com.Models;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;
using NanoidDotNet;
using QRCoder;
using static QRCoder.QRCodeGenerator;

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

            // 1) Subir blob
            var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(model.File.FileName)}";
            await using var stream = model.File.OpenReadStream();
            await _storage.UploadFileAsync(uniqueName, stream);

            // 2) Guardar metadatos en Cosmos
            var shortCode = await Nanoid.GenerateAsync(size: 6);
            var partition = shortCode.Substring(0, 2);
            var expiration = model.ExpirationDate.GetValueOrDefault(DateTime.UtcNow.AddDays(3));

            var meta = new FileMetadata
            {
                Id = shortCode,
                PartitionKey = partition,
                Filename = model.File.FileName,
                DownloadUrl = uniqueName,
                Size = model.File.Length,
                ContentType = model.File.ContentType!,
                UploadDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ExpirationDate = expiration,
                MaxDownloads = model.MaxDownloads,
                DownloadCount = 0,
                ProtectWithQR = model.ProtectWithQR
            };
            await _cosmos.AddItemAsync(meta);

            // 3) URL de descarga
            var downloadUrl = Url.Action(
                "DownloadPage",
                "Download",
                new { shortCode },
                protocol: Request.Scheme);

            // 4) Generar QR
            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(downloadUrl, ECCLevel.Q);
            using var qrPng = new PngByteQRCode(qrData);
            var qrBytes = qrPng.GetGraphic(20);
            var qrDataUrl = $"data:image/png;base64,{Convert.ToBase64String(qrBytes)}";

            // 5) ViewModel success
            var vm = new UploadSuccessModel
            {
                ShortCode = shortCode,
                DownloadUrl = downloadUrl,
                QrImageDataUrl = qrDataUrl
            };

            return View("Success", vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
