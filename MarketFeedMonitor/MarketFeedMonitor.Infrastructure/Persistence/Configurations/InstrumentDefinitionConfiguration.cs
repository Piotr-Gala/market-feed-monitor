using MarketFeedMonitor.Core.Instruments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketFeedMonitor.Infrastructure.Persistence.Configurations;

public sealed class InstrumentDefinitionConfiguration : IEntityTypeConfiguration<InstrumentDefinition>
{
    public void Configure(EntityTypeBuilder<InstrumentDefinition> builder)
    {
        builder.ToTable("instrument_definitions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.AssetType)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.PrimarySource)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => x.Symbol)
            .IsUnique();
    }
}
