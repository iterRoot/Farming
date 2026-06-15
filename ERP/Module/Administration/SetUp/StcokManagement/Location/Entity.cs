using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.Locations;

// ═══════════════════════════════════════════════════════════════
// LOCATION (KLOC)
// SAP B1 equivalent: Bin Locations / Storage Locations
// Sub-locations inside a warehouse: Aisle → Row → Shelf → Bin
// e.g. WH-001 / A / 03 / 02 / 01 = "Aisle A, Row 3, Shelf 2, Bin 1"
// ═══════════════════════════════════════════════════════════════
public class Location : AuditableEntity
{
    public string   Code          { get; set; } = null!;  // e.g. "A-03-02-01"
    public string   Name          { get; set; } = null!;  // e.g. "Aisle A, Row 3, Shelf 2"
    public string?  WarehouseCode { get; set; }           // FK to Warehouse.Code
    public string?  Aisle         { get; set; }           // e.g. "A"
    public string?  Row           { get; set; }           // e.g. "03"
    public string?  Shelf         { get; set; }           // e.g. "02"
    public string?  Bin           { get; set; }           // e.g. "01"
    public string?  Zone          { get; set; }           // e.g. "Cold", "Dry", "Hazmat"
    public decimal? MaxWeight     { get; set; }           // kg capacity
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

public class LocationConfig : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("KLOC");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.WarehouseCode).HasMaxLength(50);
        builder.Property(x => x.Aisle).HasMaxLength(20);
        builder.Property(x => x.Row).HasMaxLength(20);
        builder.Property(x => x.Shelf).HasMaxLength(20);
        builder.Property(x => x.Bin).HasMaxLength(20);
        builder.Property(x => x.Zone).HasMaxLength(50);
        builder.Property(x => x.MaxWeight).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}