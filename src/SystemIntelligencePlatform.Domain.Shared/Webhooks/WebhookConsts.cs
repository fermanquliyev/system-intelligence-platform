namespace SystemIntelligencePlatform.Webhooks;

public static class WebhookConsts
{
    public const int MaxUrlLength = 2048;
    public const int MaxSecretLength = 256;
    public const int MaxRetryAttempts = 3;
    public const int RetryDelaySeconds = 5;
}
