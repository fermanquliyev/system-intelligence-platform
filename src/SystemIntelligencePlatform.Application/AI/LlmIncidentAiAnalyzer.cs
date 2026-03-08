using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.AI;

/// <summary>
/// LLM-powered incident analyzer using Google AI Studio (Gemini/Gemma). Falls back to LocalIncidentAiAnalyzer on failure.
/// </summary>
[Dependency(ReplaceServices = true)]
public class LlmIncidentAiAnalyzer : IIncidentAiAnalyzer, ITransientDependency
{
    private readonly GoogleAiOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly LocalIncidentAiAnalyzer _fallbackAnalyzer;
    private readonly IMemoryCache _cache;
    private readonly ILogger<LlmIncidentAiAnalyzer> _logger;

    private int _consecutiveFailures;
    private DateTime _circuitOpenUntil = DateTime.MinValue;
    private readonly object _circuitLock = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    public LlmIncidentAiAnalyzer(
        IOptions<GoogleAiOptions> options,
        IHttpClientFactory httpClientFactory,
        LocalIncidentAiAnalyzer fallbackAnalyzer,
        IMemoryCache cache,
        ILogger<LlmIncidentAiAnalyzer> logger)
    {
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _fallbackAnalyzer = fallbackAnalyzer;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages)
    {
        var messages = logMessages?.Take(5).ToList() ?? new List<string>();
        if (messages.Count == 0)
            return await _fallbackAnalyzer.AnalyzeAsync(messages);

        var cacheKey = ComputeCacheKey(messages);
        if (_cache.TryGetValue(cacheKey, out AiAnalysisResult? cached))
            return cached!;

        if (IsCircuitOpen())
        {
            _logger.LogDebug("Circuit breaker open; using fallback analyzer.");
            return await FallbackAsync(messages, cacheKey);
        }

        try
        {
            var result = await CallLlmAndParseAsync(messages);
            if (result != null)
            {
                OnSuccess();
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM analysis failed; using fallback.");
            OnFailure();
        }

        return await FallbackAsync(messages, cacheKey);
    }

    private async Task<AiAnalysisResult> FallbackAsync(List<string> messages, string? cacheKey)
    {
        var result = await _fallbackAnalyzer.AnalyzeAsync(messages);
        if (cacheKey != null)
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    private bool IsCircuitOpen()
    {
        lock (_circuitLock)
        {
            if (_consecutiveFailures < _options.CircuitBreakerFailureThreshold)
                return false;
            if (DateTime.UtcNow < _circuitOpenUntil)
                return true;
            _consecutiveFailures = 0;
            return false;
        }
    }

    private void OnSuccess()
    {
        lock (_circuitLock) { _consecutiveFailures = 0; }
    }

    private void OnFailure()
    {
        lock (_circuitLock)
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _options.CircuitBreakerFailureThreshold)
                _circuitOpenUntil = DateTime.UtcNow.AddSeconds(_options.CircuitBreakerResetSeconds);
        }
    }

    private async Task<AiAnalysisResult?> CallLlmAndParseAsync(List<string> messages)
    {
        var apiKey = _options.ApiKey?.Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogDebug("AI:ApiKey not configured; using fallback.");
            return null;
        }

        var log = string.Join("\n", messages);
        var prompt = BuildPrompt(log, null, null, null);

        var request = new
        {
            contents = new[] { new { parts = new[] { new { text = prompt } } } },
            generationConfig = new
            {
                temperature = _options.Temperature,
                maxOutputTokens = _options.MaxTokens,
                responseMimeType = "application/json"
            }
        };

        var url = $"{_options.Endpoint.TrimEnd('/')}/{_options.Model}:generateContent?key={Uri.EscapeDataString(apiKey)}";
        using var client = _httpClientFactory.CreateClient("GoogleAi");
        client.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

        HttpResponseMessage? response = null;
        for (var attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var delayMs = (int)Math.Min(1000 * Math.Pow(2, attempt), 30000);
                await Task.Delay(delayMs);
            }

            try
            {
                response = await client.PostAsJsonAsync(url, request);
                if ((int)response.StatusCode == 429)
                {
                    _logger.LogWarning("Google AI rate limit (429); retry after backoff.");
                    continue;
                }
                response.EnsureSuccessStatusCode();
                break;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Google AI request failed attempt {Attempt}", attempt + 1);
                if (attempt == _options.MaxRetries) throw;
            }
        }

        if (response?.IsSuccessStatusCode != true)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var text = ExtractTextFromGenerateContentResponse(json);
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return ParseLlmResponse(text, messages);
    }

    private static string? ExtractTextFromGenerateContentResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
            {
                var first = candidates[0];
                if (first.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                {
                    var part = parts[0];
                    if (part.TryGetProperty("text", out var text))
                        return text.GetString();
                }
            }
        }
        catch { /* ignore */ }
        return null;
    }

    private AiAnalysisResult? ParseLlmResponse(string rawText, List<string> messages)
    {
        if (LlmResponseParser.TryParse(rawText, out var result))
            return result;
        _logger.LogDebug("LLM returned invalid JSON; using fallback.");
        return null;
    }

    private static string BuildPrompt(string log, string? appName, string? environment, string? previousIncidentsSummary)
    {
        var sb = new StringBuilder();
        sb.Append("You are an expert SRE analyzing application logs. Analyze the following error log and produce a structured incident analysis. Return JSON only.\n\n");
        sb.Append("Log:\n").Append(log).Append("\n\n");
        sb.Append("Context:\n");
        sb.Append("Application: ").Append(appName ?? "unknown").Append("\n");
        sb.Append("Environment: ").Append(environment ?? "unknown").Append("\n");
        if (!string.IsNullOrEmpty(previousIncidentsSummary))
            sb.Append("Previous incidents summary: ").Append(previousIncidentsSummary).Append("\n");
        sb.Append("\nReturn the following JSON fields only (no markdown, no code block):\n");
        sb.Append("{\n");
        sb.Append("  \"rootCauseSummary\": \"string\",\n");
        sb.Append("  \"suggestedFix\": \"string\",\n");
        sb.Append("  \"severity\": \"Low\" or \"Medium\" or \"High\" or \"Critical\",\n");
        sb.Append("  \"severityJustification\": \"string\",\n");
        sb.Append("  \"confidenceScore\": 0.0 to 1.0,\n");
        sb.Append("  \"containsPII\": true or false,\n");
        sb.Append("  \"piiType\": \"email\" or \"phone\" or \"token\" or \"password\" or \"none\",\n");
        sb.Append("  \"keyPhrases\": [\"string\"]\n");
        sb.Append("}\n");
        return sb.ToString();
    }

    private static string ComputeCacheKey(List<string> messages)
    {
        var combined = string.Join("|", messages);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return "llm_ai_" + Convert.ToHexString(hash).ToLowerInvariant();
    }
}
