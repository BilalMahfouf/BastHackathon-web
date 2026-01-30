namespace VeterinaryApi.Infrastructure.Mqtt;


public sealed record Esp32Response(
    bool GasLeak,
    decimal GasPresure,
    bool FlowLeak,
    decimal Tempature,
    DateTime Date);
