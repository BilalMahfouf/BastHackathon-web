namespace VeterinaryApi.Infrastructure.Mqtt;

public sealed class MqttOptions
{
    public string Host { get; init; } = "mqtt";
    public int Port { get; init; } = 1883;
    public string ClientId { get; init; } = "raqib-api";
    public bool UseTls { get; init; } = false;
    public string[] Topics { get; init; } = Array.Empty<string>();
    public MqttOptions()
    {
    }
}
