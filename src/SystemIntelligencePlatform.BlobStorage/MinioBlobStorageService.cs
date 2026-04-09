using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using SystemIntelligencePlatform.InstanceConfiguration;
using SystemIntelligencePlatform.LogEvents;

namespace SystemIntelligencePlatform.BlobStorage;

public class MinioBlobStorageService(
    IInstanceConfigurationProvider instanceConfiguration,
    IOptions<MinioBlobStorageOptions> fileOptions,
    ILogger<MinioBlobStorageService> logger) : IBlobStorageService
{
    public async Task UploadAsync(string containerName, string blobName, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerName);
        ArgumentException.ThrowIfNullOrWhiteSpace(blobName);

        var options = GetEffectiveOptions();
        if (string.IsNullOrWhiteSpace(options.Endpoint))
        {
            throw new InvalidOperationException(
                $"MinIO is not configured: set {MinioBlobStorageOptions.SectionName}:{nameof(MinioBlobStorageOptions.Endpoint)} or instance setting Minio:Endpoint.");
        }

        if (string.IsNullOrWhiteSpace(options.AccessKey) || string.IsNullOrWhiteSpace(options.SecretKey))
        {
            throw new InvalidOperationException(
                $"MinIO credentials are not configured: set access key and secret key (appsettings or instance settings Minio:AccessKey / Minio:SecretKey).");
        }

        IMinioClient client = new MinioClient()
            .WithEndpoint(options.Endpoint.Trim())
            .WithCredentials(options.AccessKey, options.SecretKey);

        if (options.UseSsl)
        {
            client = client.WithSSL();
        }

        using var minio = client.Build();

        var exists = await minio.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(containerName));

        if (!exists)
        {
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(containerName));
            logger.LogInformation("Created MinIO bucket {Bucket}", containerName);
        }

        var bytes = Encoding.UTF8.GetBytes(content);
        await using var stream = new MemoryStream(bytes, writable: false);

        await minio.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(containerName)
                .WithObject(blobName)
                .WithStreamData(stream)
                .WithObjectSize(bytes.LongLength)
                .WithContentType("application/json"));
    }

    private MinioBlobStorageOptions GetEffectiveOptions()
    {
        var f = fileOptions.Value;
        return new MinioBlobStorageOptions
        {
            Endpoint = CoalesceSetting("Minio:Endpoint", f.Endpoint),
            AccessKey = CoalesceSetting("Minio:AccessKey", f.AccessKey),
            SecretKey = CoalesceSetting("Minio:SecretKey", f.SecretKey),
            UseSsl = ParseBool(instanceConfiguration.GetEffectiveSetting("Minio:UseSsl"), f.UseSsl),
        };
    }

    private string CoalesceSetting(string key, string fileValue)
    {
        var s = instanceConfiguration.GetEffectiveSetting(key);
        return string.IsNullOrWhiteSpace(s) ? fileValue : s.Trim();
    }

    private static bool ParseBool(string? s, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            return fallback;
        }

        if (bool.TryParse(s, out var b))
        {
            return b;
        }

        if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i))
        {
            return i != 0;
        }

        return fallback;
    }
}
