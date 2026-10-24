using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.UomGroup;

// ═══════════════════════════════════════════════════════════════
// UOM GROUP (OUGP)
// SAP B1 equivalent: UoM Group — defines conversion between units
// e.g. "Weight Group": base = KG, 1 BAG = 50 KG, 1 TON = 1000 KG
// ═══════════════════════════════════════════════════════════════
public class UomGroup : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "UG-WEIGHT"
    public string  Name        { get; set; } = null!;  // e.g. "Weight Group"
    public string  BaseUom     { get; set; } = null!;  // base unit, e.g. "KG"
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }

    public ICollection<UomGroupLine> Lines { get; set; } = new List<UomGroupLine>();
}

// ── Conversion line: 1 [AltUom] = [Factor] [BaseUom] ──────────
public class UomGroupLine
{
    public int     Id          { get; set; }
    public int     UomGroupId  { get; set; }
    public int     LineNum     { get; set; }

    public UomGroup? UomGroup  { get; set; }

    public string  AltUom      { get; set; } = null!;  // e.g. "BAG"
    public decimal AltQty      { get; set; } = 1;      // 1 BAG
    public decimal BaseQty     { get; set; }           // = 50 KG
}

public class UomGroupConfig : IEntityTypeConfiguration<UomGroup>
{
    public void Configure(EntityTypeBuilder<UomGroup> builder)
    {
        builder.ToTable("KUGP");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BaseUom).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany(x => x.Lines)
               .WithOne(x => x.UomGroup)
               .HasForeignKey(x => x.UomGroupId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UomGroupLineConfig : IEntityTypeConfiguration<UomGroupLine>
{
    public void Configure(EntityTypeBuilder<UomGroupLine> builder)
    {
        builder.ToTable("UGP1");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AltUom).HasMaxLength(20).IsRequired();
        builder.Property(x => x.AltQty).HasColumnType("decimal(18,6)");
        builder.Property(x => x.BaseQty).HasColumnType("decimal(18,6)");
    }
}