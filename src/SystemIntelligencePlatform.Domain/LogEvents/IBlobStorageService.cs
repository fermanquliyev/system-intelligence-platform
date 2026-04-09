using System.Threading.Tasks;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Abstraction for blob storage used by the archival process.
/// Infrastructure provides the implementation (MinIO / S3-compatible API in production).
/// </summary>
public interface IBlobStorageService
{
    Task UploadAsync(string containerName, string blobName, string content);
}
