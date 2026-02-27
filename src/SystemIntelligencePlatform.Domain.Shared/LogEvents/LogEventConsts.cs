namespace SystemIntelligencePlatform.LogEvents;

public static class LogEventConsts
{
    public const int MaxMessageLength = 4000;
    public const int MaxSourceLength = 512;
    public const int MaxHashSignatureLength = 128;
    public const int MaxExceptionTypeLength = 512;
    public const int MaxStackTraceLength = 8000;
    public const int MaxCorrelationIdLength = 64;
    public const int BulkInsertBatchSize = 1000;
}
