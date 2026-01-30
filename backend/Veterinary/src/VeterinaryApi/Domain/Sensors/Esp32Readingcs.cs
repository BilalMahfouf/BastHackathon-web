using VeterinaryApi.Domain.Common;

namespace VeterinaryApi.Domain.Sensors;

public sealed class Esp32Readingcs : Entity
{
    public string DeviceId { get; set; } = default!;
    public DateTime TimestampUtc { get;  set; }
    public bool GasLeak { get;  set; }
    public decimal GasPressure { get;  set; }
    public bool FlowLeak { get;  set; }
    public string RawJson { get;  set; } = default!;
    
}
