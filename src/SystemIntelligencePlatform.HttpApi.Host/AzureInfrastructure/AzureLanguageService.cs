using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.AI.TextAnalytics;
using Microsoft.Extensions.Logging;
using SystemIntelligencePlatform.Incidents;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.AzureInfrastructure;

public class AzureLanguageService : IIncidentAiAnalyzer, ITransientDependency
{
    private readonly TextAnalyticsClient _client;
    private readonly ILogger<AzureLanguageService> _logger;

    public AzureLanguageService(
        TextAnalyticsClient client,
        ILogger<AzureLanguageService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages)
    {
        var result = new AiAnalysisResult();
        var messages = logMessages.Take(5).ToList();

        if (messages.Count == 0)
            return result;

        try
        {
            // Sentiment analysis
            var sentimentResults = await _client.AnalyzeSentimentBatchAsync(messages);
            var sentimentScores = sentimentResults.Value
                .Where(r => !r.HasError)
                .Select(r => r.DocumentSentiment.ConfidenceScores.Positive)
                .ToList();

            if (sentimentScores.Count > 0)
            {
                result.SentimentScore = sentimentScores.Average();
            }

            // Key phrase extraction
            var keyPhraseResults = await _client.ExtractKeyPhrasesBatchAsync(messages);
            result.KeyPhrases = keyPhraseResults.Value
                .Where(r => !r.HasError)
                .SelectMany(r => r.KeyPhrases)
                .Distinct()
                .Take(20)
                .ToList();

            // Entity recognition
            var entityResults = await _client.RecognizeEntitiesBatchAsync(messages);
            result.Entities = entityResults.Value
                .Where(r => !r.HasError)
                .SelectMany(r => r.Entities.Select(e => $"{e.Text}:{e.Category}"))
                .Distinct()
                .Take(20)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI analysis partially failed; returning partial results");
        }

        return result;
    }
}
