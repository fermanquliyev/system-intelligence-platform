using System;

namespace SystemIntelligencePlatform.Infrastructure;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";

    public string GetConnectionUri()
    {
        var vhost = string.IsNullOrEmpty(VirtualHost) || VirtualHost == "/"
            ? "/"
            : Uri.EscapeDataString(VirtualHost.TrimStart('/'));
        return $"amqp://{Uri.EscapeDataString(Username)}:{Uri.EscapeDataString(Password)}@{Host}:{Port}/{vhost}";
    }
}
