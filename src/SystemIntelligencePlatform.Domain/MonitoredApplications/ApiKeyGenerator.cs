using System;
using System.Security.Cryptography;
using System.Text;

namespace SystemIntelligencePlatform.MonitoredApplications;

public static class ApiKeyGenerator
{
    public static string Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"sip_{Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
    }

    public static string Hash(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexStringLower(bytes);
    }

    /// <summary>
    /// Constant-time comparison to prevent timing attacks.
    /// </summary>
    public static bool ValidateHash(string apiKey, string storedHash)
    {
        var computedHash = Hash(apiKey);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }
}
