using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;

namespace SystemIntelligencePlatform.Security;

public class PiiDetector : IPiiDetector, ITransientDependency
{
    private static readonly Regex Email = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled);

    private static readonly Regex JwtLike = new(
        @"eyJ[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}\.[A-Za-z0-9_-]{10,}",
        RegexOptions.Compiled);

    private static readonly Regex ApiKeyLike = new(
        @"(?:api[_-]?key|apikey|authorization)\s*[:=]\s*['""]?([A-Za-z0-9_\-]{20,})",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public bool ContainsPii(string? text) => text != null && FindSpans(text).Count > 0;

    public string Mask(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? "";

        var spans = FindSpans(text);
        if (spans.Count == 0)
            return text;

        var sb = new StringBuilder(text);
        foreach (var s in spans.OrderByDescending(x => x.Start))
        {
            var len = Math.Min(s.Length, sb.Length - s.Start);
            if (s.Start >= 0 && len > 0 && s.Start + len <= sb.Length)
                sb.Remove(s.Start, len).Insert(s.Start, new string('*', Math.Min(len, 12)));
        }

        return sb.ToString();
    }

    public IReadOnlyList<PiiSpan> FindSpans(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var list = new List<PiiSpan>();
        AddMatches(list, text, Email, "email");
        AddMatches(list, text, JwtLike, "token");
        AddMatches(list, text, ApiKeyLike, "apiKey");
        list.Sort((a, b) => a.Start.CompareTo(b.Start));
        return MergeOverlapping(list);
    }

    private static void AddMatches(List<PiiSpan> list, string text, Regex rx, string kind)
    {
        foreach (Match m in rx.Matches(text))
        {
            if (m.Success)
                list.Add(new PiiSpan(m.Index, m.Length, kind));
        }
    }

    private static List<PiiSpan> MergeOverlapping(List<PiiSpan> sorted)
    {
        if (sorted.Count == 0)
            return sorted;

        var r = new List<PiiSpan> { sorted[0] };
        for (var i = 1; i < sorted.Count; i++)
        {
            var prev = r[^1];
            var cur = sorted[i];
            if (cur.Start <= prev.Start + prev.Length)
            {
                var end = Math.Max(prev.Start + prev.Length, cur.Start + cur.Length);
                r[^1] = new PiiSpan(prev.Start, end - prev.Start, prev.Kind + "+" + cur.Kind);
            }
            else
            {
                r.Add(cur);
            }
        }

        return r;
    }
}
