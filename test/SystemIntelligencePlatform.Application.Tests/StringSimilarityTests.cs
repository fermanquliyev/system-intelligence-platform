using Shouldly;
using SystemIntelligencePlatform.Text;
using Xunit;

namespace SystemIntelligencePlatform.Application.Tests;

public class StringSimilarityTests
{
    [Fact]
    public void Levenshtein_identical_is_zero()
    {
        StringSimilarity.Levenshtein("abc", "abc").ShouldBe(0);
    }

    [Fact]
    public void Levenshtein_one_edit()
    {
        StringSimilarity.Levenshtein("abc", "abx").ShouldBe(1);
    }
}
