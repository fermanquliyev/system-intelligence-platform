namespace SystemIntelligencePlatform.FailedLogEvents;

public static class FailedLogEventConsts
{
    public const int MaxErrorMessageLength = 4000;
    public const int MaxStackTraceLength = 8000;
    public const int MaxDeliveryCount = 10;
    public const int BaseRetryDelaySeconds = 2;
}
