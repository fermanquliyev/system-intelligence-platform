using Shouldly;
using SystemIntelligencePlatform.Security;
using Xunit;

namespace SystemIntelligencePlatform.Application.Tests;

public class PiiDetectorTests
{
    private readonly PiiDetector _detector = new();

    [Fact]
    public void ContainsPii_detects_email()
    {
        _detector.ContainsPii("contact user@example.com please").ShouldBeTrue();
    }

    [Fact]
    public void Mask_replaces_email()
    {
        var m = _detector.Mask("token user@example.com end");
        m.ShouldNotContain("example.com");
        m.ShouldContain('*');
    }

    [Fact]
    public void ContainsPii_false_for_plain_text()
    {
        _detector.ContainsPii("no secrets here").ShouldBeFalse();
    }
}
