using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.ShippingTypes;

// ═══════════════════════════════════════════════════════════════
// SHIPPING TYPE (KSHT)
// SAP B1 equivalent: Shipping Types setup
// Defines how goods are transported: Air, Sea, Land, Express...
// ═══════════════════════════════════════════════════════════════
public class ShippingType : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "SHP-AIR"
    public string  Name        { get; set; } = null!;  // e.g. "Air Freight"
    public string? Carrier     { get; set; }           // e.g. "DHL", "FedEx"
    public string? Website     { get; set; }
    public string? Phone       { get; set; }
    public string? TrackingUrl { get; set; }           // e.g. https://track.dhl.com/{0}
    public int?    LeadTimeDays{ get; set; }           // estimated transit days
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class ShippingTypeConfig : IEntityTypeConfiguration<ShippingType>
{
    public void Configure(EntityTypeBuilder<ShippingType> builder)
    {
        builder.ToTable("KSHT");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Carrier).HasMaxLength(100);
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.TrackingUrl).HasMaxLength(300);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}