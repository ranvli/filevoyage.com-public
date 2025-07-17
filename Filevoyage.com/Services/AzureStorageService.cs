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
            var accountName = configuration["AzureStorage:AccountName"];
            var accountKey = configuration["AzureStorage:AccountKey"];
            var containerName = configuration["AzureStorage:ContainerName"];

            var credentials = new StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");

            var serviceClient = new BlobServiceClient(blobUri, credentials);

            _containerClient = serviceClient.GetBlobContainerClient(containerName);
            _containerClient.CreateIfNotExists(PublicAccessType.None);
        }

        public async Task UploadFileAsync(string fileName, Stream fileStream)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream, overwrite: true);
        }

        public async Task<Stream?> DownloadFileAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            if (await blobClient.ExistsAsync())
            {
                var download = await blobClient.DownloadAsync();
                return download.Value.Content;
            }

            return null;
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var blobClient = _containerClient.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> ListFilesAsync()
        {
            var results = new List<string>();

            await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync())
            {
                results.Add(blobItem.Name);
            }

            return results;
        }
    }
}
