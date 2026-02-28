using System.Threading.Tasks;

namespace SystemIntelligencePlatform.LogEvents;

/// <summary>
/// Abstraction for blob storage used by the archival process.
/// Infrastructure provides the Azure Blob Storage implementation.
/// </summary>
public interface IBlobStorageService
{
    Task UploadAsync(string containerName, string blobName, string content);
}
