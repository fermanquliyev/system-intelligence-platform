namespace SystemIntelligencePlatform.CostEstimation;

public class CostEstimateInput
{
    public long LogsPerDay { get; set; }
    public bool AiEnrichmentEnabled { get; set; } = true;
}

public class CostEstimateDto
{
    public decimal TotalMonthlyCost { get; set; }
    public decimal ServiceBusCost { get; set; }
    public decimal FunctionsCost { get; set; }
    public decimal SqlStorageCost { get; set; }
    public decimal AiEnrichmentCost { get; set; }
    public decimal SearchCost { get; set; }
}
