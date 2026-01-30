using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VeterinaryApi.Domain.Sensors;

namespace VeterinaryApi.Infrastructure.Persistence.Configurations;

public sealed class Esp32ReadingConfiguration : IEntityTypeConfiguration<Esp32Readingcs>
{
    public void Configure(EntityTypeBuilder<Esp32Readingcs> builder)
    {
        builder.ToTable("Esp32Readings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.TimestampUtc)
            .IsRequired();

        builder.Property(e => e.GasLeak)
            .IsRequired();

        builder.Property(e => e.GasPressure)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(e => e.FlowLeak)
            .IsRequired();

        builder.Property(e => e.RawJson)
            .IsRequired();

        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.TimestampUtc);
    }
}
