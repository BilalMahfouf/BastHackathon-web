
using MQTTnet;

namespace VeterinaryApi.Infrastructure.Mqtt;

public sealed class MqttWorker : BackgroundService
{
    private readonly MqttOptions _mqttOptions;
    private  IMqttClient _client;
    private readonly IServiceProvider _serviceProvider;

    public MqttWorker(
            MqttOptions mqttOptions,
            IMqttClient mqttClient,
            IServiceProvider serviceProvider)
    {
        _mqttOptions = mqttOptions;
        this._client = mqttClient;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttClientFactory();
        _client = factory.CreateMqttClient();

        _client.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var rawJson = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());

            // Expect: raqib/sensors/<deviceId>
            if (!topic.StartsWith("raqib/sensors/", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Ignoring topic {Topic}", topic);
                return;
            }

            var parts = topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var deviceId = parts.Length >= 3 ? parts[2] : "unknown";

            try
            {
                var entity = ParseToEntity(deviceId, rawJson);

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // <-- CHANGE THIS TYPE

                db.GasSensorReadings.Add(entity);
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Saved reading device={DeviceId} leak={Leak} pressure={Pressure} flowLeak={FlowLeak} time={Time}",
                    entity.DeviceId, entity.GasLeak, entity.GasPressure, entity.FlowLeak, entity.TimestampUtc);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MQTT handling failed topic={Topic} raw={Raw}", topic, rawJson);
            }
        };

        _client.DisconnectedAsync += async _ =>
        {
            _logger.LogWarning("MQTT disconnected. Reconnecting...");
            if (stoppingToken.IsCancellationRequested) return;
            await Task.Delay(2000, stoppingToken);
            await ConnectAndSubscribe(stoppingToken);
        };

        await ConnectAndSubscribe(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ConnectAndSubscribe(CancellationToken ct)
    {
        if (_client is null) return;

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Host, _options.Port)
            .WithClientId(_options.ClientId)
            .WithCleanSession(false);

        if (_options.UseTls)
        {
            builder.WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true
            });
        }

        _logger.LogInformation("Connecting MQTT to {Host}:{Port}", _options.Host, _options.Port);
        await _client.ConnectAsync(builder.Build(), ct);

        var topics = _options.Topics ?? Array.Empty<string>();
        if (topics.Length == 0)
        {
            _logger.LogWarning("No MQTT topics configured.");
            return;
        }

        var sub = new MqttClientSubscribeOptionsBuilder();
        foreach (var t in topics)
        {
            sub.WithTopicFilter(f => f.WithTopic(t).WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce));
        }

        await _client.SubscribeAsync(sub.Build(), ct);
        _logger.LogInformation("Subscribed to {Count} topic(s).", topics.Length);
    }

    private static GasSensorReading ParseToEntity(string deviceId, string rawJson)
    {
        var j = JObject.Parse(rawJson);

        bool gasLeak = j.Value<bool?>("gasLeak") ?? false;
        bool flowLeak = j.Value<bool?>("flowLeak") ?? false;

        // Accept gasPresure (your field) and gasPressure (common spelling)
        decimal gasPressure =
            j.Value<decimal?>("gasPresure")
            ?? j.Value<decimal?>("gasPressure")
            ?? 0m;

        // Accept ISO UTC string; if missing, use UtcNow
        // If ESP32 sends "2026-01-30T12:34:56Z" this works.
        DateTime timestampUtc =
            TryParseUtc(j["dateTime"])
            ?? DateTime.UtcNow;

        return new GasSensorReading(
            deviceId: deviceId,
            timestampUtc: timestampUtc,
            gasLeak: gasLeak,
            gasPressure: gasPressure,
            flowLeak: flowLeak,
            rawJson: rawJson
        );
    }

    private static DateTime? TryParseUtc(JToken? token)
    {
        if (token is null) return null;

        // If it is a string date
        if (token.Type == JTokenType.String)
        {
            var s = token.Value<string>();
            if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var dt))
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
        }

        // If it is unix seconds (optional support)
        if (token.Type == JTokenType.Integer && long.TryParse(token.ToString(), out var unix))
        {
            return DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime;
        }

        return null;
    }
    }
}
