using System.Collections.Generic;

namespace SystemIntelligencePlatform.Security;

public interface IPiiDetector
{
    bool ContainsPii(string? text);

    string Mask(string? text);

    IReadOnlyList<PiiSpan> FindSpans(string? text);
}

public readonly struct PiiSpan
{
    public PiiSpan(int start, int length, string kind)
    {
        Start = start;
        Length = length;
        Kind = kind;
    }

    public int Start { get; }
    public int Length { get; }
    public string Kind { get; }
}
