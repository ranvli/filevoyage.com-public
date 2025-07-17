using System.Diagnostics;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Filevoyage.com.Models;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage;
using NanoidDotNet;

namespace Filevoyage.com.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AzureStorageService _storage;
        private readonly CosmosDbService _cosmosDb;

        public HomeController(AzureStorageService storage, IConfiguration configuration, CosmosDbService cosmosDb)
        {
            _configuration = configuration;
            _storage = storage;
            _cosmosDb = cosmosDb;
        }


        [HttpGet]
        public IActionResult Index()
        {
            var model = new UploadModel
            {
                ExpirationDate = DateTime.UtcNow.AddDays(3)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Index(UploadModel model)
        {
            if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View(model);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.File.FileName);

            var containerName = _configuration["AzureStorage:ContainerName"];
            var accountName = _configuration["AzureStorage:AccountName"];
            var accountKey = _configuration["AzureStorage:AccountKey"];

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var blobServiceClient = new BlobServiceClient(blobUri, credential);

            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(uniqueFileName);
            using (var stream = model.File.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            model.DownloadUrl = blobClient.Uri.ToString();
            var shortCode = await Nanoid.GenerateAsync(size: 4);
            var partitionKey = shortCode.Substring(0, 2);

            // Guardar en CosmosDB
            var metadata = new FileMetadata
            {
                Id = shortCode,
                PartitionKey = partitionKey,
                Filename = model.File.FileName,
                UploadDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Size = model.File.Length,
                ContentType = model.File.ContentType,
                DownloadUrl = blobClient.Uri.ToString()
            };


            await _cosmosDb.AddItemAsync(metadata);

            // Redirigir a vista de éxito
            return View("Success", new UploadSuccessModel { ShortCode = shortCode });
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToBlob(string shortCode)
        {
            if (string.IsNullOrWhiteSpace(shortCode))
                return NotFound();

            var metadata = await _cosmosDb.GetItemByIdAsync(shortCode);

            if (metadata == null || string.IsNullOrEmpty(metadata.DownloadUrl))
                return NotFound();

            // Leer desde Azure Storage usando SDK
            var accountName = _configuration["AzureStorage:AccountName"];
            var accountKey = _configuration["AzureStorage:AccountKey"];
            var containerName = _configuration["AzureStorage:ContainerName"];

            var credential = new StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var blobServiceClient = new BlobServiceClient(blobUri, credential);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Path.GetFileName(metadata.DownloadUrl));

            var response = await blobClient.DownloadAsync();
            var stream = response.Value.Content;
            var contentType = response.Value.Details.ContentType ?? "application/octet-stream";

            return File(stream, contentType, metadata.Filename);
        }
    }
}
