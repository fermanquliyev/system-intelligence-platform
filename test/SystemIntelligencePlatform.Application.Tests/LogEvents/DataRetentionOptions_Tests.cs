using Shouldly;
using Xunit;

namespace SystemIntelligencePlatform.LogEvents;

public class DataRetentionOptions_Tests
{
    [Fact]
    public void Default_LogRetentionDays_Should_Be_90()
    {
        var options = new DataRetentionOptions();
        options.LogRetentionDays.ShouldBe(90);
    }

    [Fact]
    public void SectionName_Should_Be_DataRetention()
    {
        DataRetentionOptions.SectionName.ShouldBe("DataRetention");
    }
}
