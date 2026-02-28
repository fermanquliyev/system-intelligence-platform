using Shouldly;
using SystemIntelligencePlatform.CostEstimation;
using Xunit;

namespace SystemIntelligencePlatform.CostEstimator;

/// <summary>
/// Pure unit tests for cost estimation functionality.
/// Tests verify that the CostEstimatorAppService correctly calculates monthly costs
/// based on log volume and feature flags.
/// </summary>
public class CostEstimator_Tests
{
    private readonly ICostEstimatorAppService _costEstimator;

    public CostEstimator_Tests()
    {
        _costEstimator = new CostEstimatorAppService();
    }

    /// <summary>
    /// Verifies cost calculation for 1 million logs per day.
    /// This represents a moderate volume scenario.
    /// </summary>
    [Fact]
    public void Should_Calculate_For_1M_Logs_Per_Day()
    {
        // Arrange
        var input = new CostEstimateInput
        {
            LogsPerDay = 1_000_000,
            AiEnrichmentEnabled = false
        };

        // Act
        var result = _costEstimator.Calculate(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalMonthlyCost.ShouldBeGreaterThan(0);
        result.ServiceBusCost.ShouldBeGreaterThanOrEqualTo(0);
        result.FunctionsCost.ShouldBeGreaterThanOrEqualTo(0);
        result.SqlStorageCost.ShouldBeGreaterThanOrEqualTo(0);
        result.AiEnrichmentCost.ShouldBe(0); // AI disabled
        result.SearchCost.ShouldBeGreaterThanOrEqualTo(0);
    }

    /// <summary>
    /// Verifies cost calculation for 10 million logs per day.
    /// This represents a high volume scenario and tests scaling behavior.
    /// </summary>
    [Fact]
    public void Should_Calculate_For_10M_Logs_Per_Day()
    {
        // Arrange
        var input = new CostEstimateInput
        {
            LogsPerDay = 10_000_000,
            AiEnrichmentEnabled = false
        };

        // Act
        var result = _costEstimator.Calculate(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalMonthlyCost.ShouldBeGreaterThan(0);
        
        // Higher volume should result in higher costs
        var input1M = new CostEstimateInput { LogsPerDay = 1_000_000, AiEnrichmentEnabled = false };
        var result1M = _costEstimator.Calculate(input1M);
        result.TotalMonthlyCost.ShouldBeGreaterThan(result1M.TotalMonthlyCost);
    }

    /// <summary>
    /// Verifies that AI enrichment costs scale proportionally with log volume.
    /// AI enrichment typically has a per-log cost component.
    /// </summary>
    [Fact]
    public void Should_Scale_AI_Cost_With_Volume()
    {
        // Arrange
        var inputLow = new CostEstimateInput
        {
            LogsPerDay = 100_000,
            AiEnrichmentEnabled = true
        };

        var inputHigh = new CostEstimateInput
        {
            LogsPerDay = 1_000_000,
            AiEnrichmentEnabled = true
        };

        // Act
        var resultLow = _costEstimator.Calculate(inputLow);
        var resultHigh = _costEstimator.Calculate(inputHigh);

        // Assert
        resultLow.AiEnrichmentCost.ShouldBeGreaterThan(0);
        resultHigh.AiEnrichmentCost.ShouldBeGreaterThan(resultLow.AiEnrichmentCost);
        resultHigh.TotalMonthlyCost.ShouldBeGreaterThan(resultLow.TotalMonthlyCost);
    }

    /// <summary>
    /// Verifies that zero logs results in zero or minimal costs.
    /// This ensures the calculator handles edge cases correctly.
    /// </summary>
    [Fact]
    public void Should_Return_Zero_For_Zero_Logs()
    {
        // Arrange
        var input = new CostEstimateInput
        {
            LogsPerDay = 0,
            AiEnrichmentEnabled = false
        };

        // Act
        var result = _costEstimator.Calculate(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalMonthlyCost.ShouldBe(0);
        result.ServiceBusCost.ShouldBe(0);
        result.FunctionsCost.ShouldBe(0);
        result.SqlStorageCost.ShouldBe(0);
        result.AiEnrichmentCost.ShouldBe(0);
        result.SearchCost.ShouldBe(0);
    }
}
