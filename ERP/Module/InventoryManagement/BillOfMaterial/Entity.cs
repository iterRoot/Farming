using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.BillOfMaterial;

// ═══════════════════════════════════════════════════════════════════
// BILL OF MATERIAL  (KBOM / BOM1)
// SAP B1 equivalent: Inventory > Product Trees. Recipe for how many of
// each component it takes to produce Quantity units of ParentItemCode.
// ═══════════════════════════════════════════════════════════════════
public class BillOfMaterial : AuditableEntity
{
    public string  Code            { get; set; } = null!;   // e.g. "BOM-0001"
    public string  ParentItemCode  { get; set; } = null!;   // the finished good
    public string  ParentItemName  { get; set; } = null!;
    public decimal Quantity        { get; set; } = 1m;      // units of parent this recipe yields
    public string  Warehouse       { get; set; } = null!;   // default production warehouse
    public bool    IsActive        { get; set; } = true;
    public string? Remarks         { get; set; }

    public ICollection<BillOfMaterialLine> Lines { get; set; } = new List<BillOfMaterialLine>();
}

public class BillOfMaterialLine
{
    public int     Id                { get; set; }
    public int     BillOfMaterialId  { get; set; }
    public int     LineNum           { get; set; }
    public string  ItemCode          { get; set; } = null!;
    public string  ItemName          { get; set; } = null!;
    public decimal Quantity          { get; set; }          // needed per Quantity units of parent above

    public BillOfMaterial BillOfMaterial { get; set; } = null!;
}

public class BillOfMaterialConfig : IEntityTypeConfiguration<BillOfMaterial>
{
    public void Configure(EntityTypeBuilder<BillOfMaterial> b)
    {
        b.ToTable("KBOM");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.ParentItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ParentItemName).HasMaxLength(200);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.Warehouse).HasMaxLength(50).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.ParentItemCode);

        b.HasMany(x => x.Lines).WithOne(l => l.BillOfMaterial)
         .HasForeignKey(l => l.BillOfMaterialId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class BillOfMaterialLineConfig : IEntityTypeConfiguration<BillOfMaterialLine>
{
    public void Configure(EntityTypeBuilder<BillOfMaterialLine> b)
    {
        b.ToTable("BOM1");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.Quantity).HasPrecision(18, 4);
    }
}
