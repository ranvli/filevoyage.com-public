// File: Services/AzureStorageService.cs
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Filevoyage.com.Services
{
    public class AzureStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureStorageService(IConfiguration configuration)
        {
            var accountName = configuration["AzureStorage:AccountName"]!;
            var accountKey = configuration["AzureStorage:AccountKey"]!;
            var containerName = configuration["AzureStorage:ContainerName"]!;

            var creds = new StorageSharedKeyCredential(accountName, accountKey);
            var uri = new Uri($"https://{accountName}.blob.core.windows.net");
            var svc = new BlobServiceClient(uri, creds);

            _containerClient = svc.GetBlobContainerClient(containerName);
            // Aseguramos existencia sin habilitar acceso público
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task UploadFileAsync(string fileName, Stream stream)
            => await _containerClient
                    .GetBlobClient(fileName)
                    .UploadAsync(stream, overwrite: true);

        /// <summary>
        /// Devuelve el contenido y su content-type, o null si no existe.
        /// </summary>
        public async Task<(Stream Content, string ContentType)?> DownloadFileStreamAsync(string fileName)
        {
            var blob = _containerClient.GetBlobClient(fileName);
            if (!await blob.ExistsAsync()) return null;

            var download = await blob.DownloadAsync();
            var ct = download.Value.Details.ContentType ?? "application/octet-stream";
            return (download.Value.Content, ct);
        }

        public async Task DeleteFileAsync(string fileName)
            => await _containerClient.GetBlobClient(fileName)
                                     .DeleteIfExistsAsync();

        public async Task<List<string>> ListFilesAsync()
        {
            var list = new List<string>();
            await foreach (var item in _containerClient.GetBlobsAsync())
                list.Add(item.Name);
            return list;
        }
    }
}
