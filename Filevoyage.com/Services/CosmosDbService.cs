// Services/CosmosDbService.cs
using Filevoyage.com.Models;
using Microsoft.Azure.Cosmos;

namespace Filevoyage.com.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(CosmosClient client, string databaseName, string containerName)
        {
            _container = client.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(FileMetadata item)
            => await _container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));

        public async Task<FileMetadata?> GetItemByIdAsync(string id)
        {
            var pk = id.Substring(0, 2);
            try
            {
                var resp = await _container.ReadItemAsync<FileMetadata>(id, new PartitionKey(pk));
                return resp.Resource;
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task UpdateItemAsync(FileMetadata item)
            => await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));

        public async Task<FileMetadata?> IncrementDownloadCountAsync(string id)
        {
            // recupera con partición
            var meta = await GetItemByIdAsync(id);
            if (meta == null) return null;

            meta.DownloadsCount++;
            // regraba todo el documento
            await _container.UpsertItemAsync(meta, new PartitionKey(meta.PartitionKey));
            return meta;
        }
    }
}