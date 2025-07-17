using Newtonsoft.Json;

namespace Filevoyage.com.Models
{
    public class FileMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }  // Guid o filename

        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }    // "a3", por ejemplo

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("uploadDate")]
        public string UploadDate { get; set; }  // ej "2025-07-17"

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }
    }
}
