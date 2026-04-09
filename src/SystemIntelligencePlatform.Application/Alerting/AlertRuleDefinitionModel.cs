using System.Text.Json.Serialization;

namespace SystemIntelligencePlatform.Alerting;

/// <summary>JSON shape stored in <see cref="AlertRule.DefinitionJson"/>.</summary>
public class AlertRuleDefinitionModel
{
    [JsonPropertyName("minErrorsLastHour")]
    public int? MinErrorsLastHour { get; set; }

    [JsonPropertyName("minWarningsLastHour")]
    public int? MinWarningsLastHour { get; set; }
}
