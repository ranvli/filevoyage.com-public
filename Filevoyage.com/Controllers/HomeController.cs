using System.Diagnostics;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Filevoyage.com.Models;
using Filevoyage.com.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Azure.Storage;

namespace Filevoyage.com.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly AzureStorageService _storage;

        public HomeController(AzureStorageService storage, IConfiguration configuration)
        {
            _configuration = configuration;
            _storage = storage;
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
            return View(model);
        }

    }
}
