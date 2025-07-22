using Newtonsoft.Json;

namespace Filevoyage.com.Models
{
    public class FileMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("uploadDate")]
        public string UploadDate { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        // ✚ Nuevos campos:
        [JsonProperty("expirationDate")]
        public DateTime ExpirationDate { get; set; }

        [JsonProperty("maxDownloads")]
        public int MaxDownloads { get; set; }

        [JsonProperty("downloadCount")]
        public int DownloadCount { get; set; }

        [JsonProperty("protectWithQR")]
        public bool ProtectWithQR { get; set; }
        
        [JsonProperty("downloadsCount")]
        public int DownloadsCount { get; internal set; }
    }
}
