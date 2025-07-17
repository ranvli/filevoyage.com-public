using Filevoyage.com.Models;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;

namespace Filevoyage.com.Services
{
    public class CosmosDbService
    {
        private readonly Container _container;

        public CosmosDbService(CosmosClient dbClient, string databaseName, string containerName)
        {
            _container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task AddItemAsync(FileMetadata item)
        {
            await _container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
        }

        public async Task<FileMetadata> GetItemByIdAsync(string id)
        {
            var partKey = id.Substring(0, 2);
            var response = await _container.ReadItemAsync<FileMetadata>(id, new PartitionKey(partKey));
            return response.Resource;
        }
    }
}
