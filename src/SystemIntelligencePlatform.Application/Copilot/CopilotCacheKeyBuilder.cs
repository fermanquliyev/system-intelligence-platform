using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SystemIntelligencePlatform.Copilot;

public static class CopilotCacheKeyBuilder
{
    private static readonly Regex NormalizeNumbers = new(@"\b\d+\b", RegexOptions.Compiled);
    private static readonly Regex NormalizeGuids = new(
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled);

    public static string NormalizeForPatternHash(IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            var n = NormalizeGuids.Replace(NormalizeNumbers.Replace(line, "#"), "#");
            sb.AppendLine(n);
        }

        return sb.ToString();
    }

    public static string ComputePatternHash(IEnumerable<string> lines)
    {
        var normalized = NormalizeForPatternHash(lines);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
