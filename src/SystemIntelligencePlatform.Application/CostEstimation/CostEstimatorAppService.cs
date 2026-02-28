using System;
using Volo.Abp.Application.Services;

namespace SystemIntelligencePlatform.CostEstimation;

/// <summary>
/// [MVP: Disabled] Internal cost estimation tool. Not exposed to customers.
/// Retained for internal capacity planning.
///
/// Pricing assumptions (pay-as-you-go, free tiers where applicable):
/// - Service Bus Basic: $0.05 per million operations
/// - Azure Functions Consumption: first 1M executions free, then $0.20/million
/// - Azure SQL Serverless: ~$0.000145/vCore-second + $0.10/GB storage
/// - Azure Language: $1.00 per 1000 text records (first 5000 free/month)
/// - Azure AI Search Free: $0, Basic: $75.14/month if needed
/// </summary>
public class CostEstimatorAppService : ApplicationService, ICostEstimatorAppService
{
    private const decimal ServiceBusPricePerMillion = 0.05m;
    private const decimal FunctionsPricePerMillion = 0.20m;
    private const long FunctionsFreeExecutions = 1_000_000;
    private const decimal SqlVCorePerSecond = 0.000145m;
    private const decimal SqlStoragePerGbMonth = 0.10m;
    private const decimal AiPricePer1000Records = 1.00m;
    private const int AiFreeRecordsPerMonth = 5000;
    private const decimal SearchBasicTierMonthly = 75.14m;
    private const decimal AvgLogSizeKb = 0.5m;

    public CostEstimateDto Calculate(CostEstimateInput input)
    {
        if (input.LogsPerDay <= 0)
        {
            return new CostEstimateDto();
        }

        var logsPerMonth = input.LogsPerDay * 30m;

        // Service Bus: one send operation per log event
        var sbOps = logsPerMonth;
        var serviceBusCost = (sbOps / 1_000_000m) * ServiceBusPricePerMillion;

        // Functions: one execution per log event
        var billableExecutions = Math.Max(0, logsPerMonth - FunctionsFreeExecutions);
        var functionsCost = (billableExecutions / 1_000_000m) * FunctionsPricePerMillion;

        // SQL: storage cost (assume avg 0.5KB per log, keep 30 days)
        var storageGb = (logsPerMonth * AvgLogSizeKb) / (1024m * 1024m);
        // vCore seconds: assume ~5% utilization for serverless during processing
        var estimatedActiveSeconds = (logsPerMonth / 1000m) * 0.1m;
        var sqlStorageCost = (storageGb * SqlStoragePerGbMonth) + (estimatedActiveSeconds * SqlVCorePerSecond);

        // AI enrichment: ~5% of logs trigger incident analysis (5 messages each)
        decimal aiCost = 0;
        if (input.AiEnrichmentEnabled)
        {
            var incidentRate = logsPerMonth * 0.001m; // ~0.1% become incidents
            var aiRecords = incidentRate * 5m; // 5 messages per incident analysis
            var billableAiRecords = Math.Max(0, aiRecords - AiFreeRecordsPerMonth);
            aiCost = (billableAiRecords / 1000m) * AiPricePer1000Records;
        }

        // Search: free tier handles up to 50MB, assume Basic if > 10K incidents/month
        var estimatedIncidents = logsPerMonth * 0.001m;
        var searchCost = estimatedIncidents > 10000 ? SearchBasicTierMonthly : 0m;

        var total = serviceBusCost + functionsCost + sqlStorageCost + aiCost + searchCost;

        return new CostEstimateDto
        {
            TotalMonthlyCost = Math.Round(total, 2),
            ServiceBusCost = Math.Round(serviceBusCost, 2),
            FunctionsCost = Math.Round(functionsCost, 2),
            SqlStorageCost = Math.Round(sqlStorageCost, 2),
            AiEnrichmentCost = Math.Round(aiCost, 2),
            SearchCost = Math.Round(searchCost, 2)
        };
    }
}
