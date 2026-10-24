using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.AlternativeItem;

// ═══════════════════════════════════════════════════════════════════
// ALTERNATIVE ITEM  (KALT)
// SAP B1 equivalent: Inventory > Item Management > Alternative Items.
// For one item, a ranked list of substitutes with a matching factor
// (how many of the alternative replace one of the original) so a sales
// or purchasing clerk can swap when the first choice is out of stock.
// ═══════════════════════════════════════════════════════════════════
public class AlternativeItem : AuditableEntity
{
    public string  ItemCode    { get; set; } = null!;   // the original item
    public string  AltItemCode { get; set; } = null!;   // the substitute
    public string? AltItemName { get; set; }
    public decimal Factor      { get; set; } = 1m;       // units of substitute per 1 original
    public int     Ranking     { get; set; }             // lower = preferred
    public string? Criteria    { get; set; }             // free-text reason / note
}

public class AlternativeItemConfig : IEntityTypeConfiguration<AlternativeItem>
{
    public void Configure(EntityTypeBuilder<AlternativeItem> b)
    {
        b.ToTable("KALT");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AltItemCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AltItemName).HasMaxLength(200);
        b.Property(x => x.Factor).HasPrecision(18, 4);
        b.Property(x => x.Criteria).HasMaxLength(254);
        b.HasIndex(x => new { x.ItemCode, x.AltItemCode }).IsUnique();
        b.HasIndex(x => x.ItemCode);
    }
}
