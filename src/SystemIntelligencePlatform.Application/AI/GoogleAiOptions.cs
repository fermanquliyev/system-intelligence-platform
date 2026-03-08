namespace SystemIntelligencePlatform.AI;

/// <summary>
/// Configuration for Google AI Studio (Gemini / Gemma). ApiKey should be set via environment variable (e.g. AI__ApiKey).
/// </summary>
public class GoogleAiOptions
{
    public const string SectionName = "AI";

    public string Provider { get; set; } = "Google";
    public string Model { get; set; } = "gemma-3-4b-it";
    /// <summary>API key; load from environment variable AI__ApiKey in production.</summary>
    public string ApiKey { get; set; } = "";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com/v1beta/models";
    public int MaxTokens { get; set; } = 1024;
    public double Temperature { get; set; } = 0.2;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public int CircuitBreakerFailureThreshold { get; set; } = 5;
    public int CircuitBreakerResetSeconds { get; set; } = 60;
    /// <summary>Optional: max requests per minute for rate limit handling (e.g. 15).</summary>
    public int? RequestsPerMinuteLimit { get; set; }
}
