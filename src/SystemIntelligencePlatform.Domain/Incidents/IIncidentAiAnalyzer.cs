using System.Collections.Generic;
using System.Threading.Tasks;

namespace SystemIntelligencePlatform.Incidents;

/// <summary>
/// Analyzes incident-related log messages using AI/NLP services.
/// Infrastructure layer provides the Azure Language implementation.
/// </summary>
public interface IIncidentAiAnalyzer
{
    Task<AiAnalysisResult> AnalyzeAsync(IEnumerable<string> logMessages);
}

public class AiAnalysisResult
{
    public double? SentimentScore { get; set; }
    public List<string> KeyPhrases { get; set; } = new();
    public List<string> Entities { get; set; } = new();
}
