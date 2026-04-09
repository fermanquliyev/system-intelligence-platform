namespace SystemIntelligencePlatform.BlobStorage;

public class MinioBlobStorageOptions
{
    public const string SectionName = "Minio";

    /// <summary>
    /// Host and port only (no scheme), e.g. <c>localhost:9000</c> or <c>minio:9000</c> in Docker.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool UseSsl { get; set; }
}
