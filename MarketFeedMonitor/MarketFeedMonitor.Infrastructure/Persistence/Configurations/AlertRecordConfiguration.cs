using MarketFeedMonitor.Core.Alerts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketFeedMonitor.Infrastructure.Persistence.Configurations;

public sealed class AlertRecordConfiguration : IEntityTypeConfiguration<AlertRecord>
{
    public void Configure(EntityTypeBuilder<AlertRecord> builder)
    {
        builder.ToTable("alert_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlertType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new { x.InstrumentId, x.IsActive });
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.Instrument)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.InstrumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
