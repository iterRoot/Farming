using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.BinLocation;

// ═══════════════════════════════════════════════════════════════════
// BIN LOCATION  (KBIN)
// SAP B1 equivalent: Inventory > Bin Locations. A storage position
// inside a warehouse (aisle/rack/shelf). This master defines the bins;
// bin-level stock movement is a larger feature not built here — each
// bin carries an enabled flag and an optional default so the catalogue
// is usable on its own.
// ═══════════════════════════════════════════════════════════════════
public class BinLocation : AuditableEntity
{
    public string  Code          { get; set; } = null!;   // unique bin code, e.g. WH01-A-01-1
    public string  WarehouseCode { get; set; } = null!;   // FK → KWHS.Code
    public string? Description   { get; set; }
    public string? Sublevel1     { get; set; }            // aisle
    public string? Sublevel2     { get; set; }            // rack / shelf
    public bool    IsActive      { get; set; } = true;
    public bool    IsReceivingBin { get; set; }           // default bin for receipts

    public decimal? MaxWeight    { get; set; }
}

public class BinLocationConfig : IEntityTypeConfiguration<BinLocation>
{
    public void Configure(EntityTypeBuilder<BinLocation> b)
    {
        b.ToTable("KBIN");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.WarehouseCode).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.WarehouseCode);
        b.Property(x => x.Description).HasMaxLength(200);
        b.Property(x => x.Sublevel1).HasMaxLength(50);
        b.Property(x => x.Sublevel2).HasMaxLength(50);
        b.Property(x => x.MaxWeight).HasPrecision(18, 4);
    }
}
