using System.Collections.Generic;
using System.Threading.Tasks;
using SystemIntelligencePlatform.LogEvents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Fakes;

/// <summary>
/// In-memory fake for IBlobStorageService used during tests.
/// Captures uploads for assertion.
/// </summary>
[Dependency(ReplaceServices = true)]
public class FakeBlobStorageService : IBlobStorageService, ITransientDependency
{
    public List<(string Container, string BlobName, string Content)> Uploads { get; } = new();

    public Task UploadAsync(string containerName, string blobName, string content)
    {
        Uploads.Add((containerName, blobName, content));
        return Task.CompletedTask;
    }
}
