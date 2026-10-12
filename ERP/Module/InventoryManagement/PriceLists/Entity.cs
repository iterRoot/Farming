using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.PriceLists;

// ═══════════════════════════════════════════════════════════════
// PRICE LIST (OPLN)
// SAP B1 equivalent: Inventory > Price Lists > Price Lists.
// A named price list derived from a base list × factor, then rounded.
// BasePriceListId == null means the list is its own base.
// ═══════════════════════════════════════════════════════════════
public class PriceList : AuditableEntity
{
    public string   Name               { get; set; } = null!;
    public bool     MultipleBase       { get; set; }
    public int?     BasePriceListId    { get; set; }
    public PriceList? BasePriceList    { get; set; }
    public decimal  Factor             { get; set; } = 1m;
    public string   RoundingMethod     { get; set; } = "No Rounding";
    public string   RoundingRule       { get; set; } = "Round to Closest";
    public decimal? EndingInterval     { get; set; }
    public string?  AuthorizationGroup { get; set; }
    public bool     GrossPrice         { get; set; }
    public string   PrimaryCurrency    { get; set; } = "USD";
    public bool     IsActive           { get; set; } = true;
}

// ═══════════════════════════════════════════════════════════════
// ITEM PRICE (KIPL)  ·  an item's own price on one price list.
// Only prices someone typed in are stored; every other item on a
// list is priced by PriceResolver (base list × factor, rounded).
// ═══════════════════════════════════════════════════════════════
public class ItemPrice : AuditableEntity
{
    public int      PriceListId { get; set; }
    public string   ItemCode    { get; set; } = null!;
    public decimal  UnitPrice   { get; set; }

    public PriceList PriceList { get; set; } = null!;
}

public class ItemPriceConfig : IEntityTypeConfiguration<ItemPrice>
{
    public void Configure(EntityTypeBuilder<ItemPrice> b)
    {
        b.ToTable("KIPL");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 4);
        b.HasIndex(x => new { x.PriceListId, x.ItemCode }).IsUnique();
        // Prices belong to their list: deleting the list deletes them.
        b.HasOne(x => x.PriceList).WithMany().HasForeignKey(x => x.PriceListId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PriceListConfig : IEntityTypeConfiguration<PriceList>
{
    public void Configure(EntityTypeBuilder<PriceList> b)
    {
        b.ToTable("OPLN");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.Factor).HasColumnType("decimal(18,6)");
        b.Property(x => x.RoundingMethod).HasMaxLength(40);
        b.Property(x => x.RoundingRule).HasMaxLength(40);
        b.Property(x => x.EndingInterval).HasColumnType("decimal(18,6)");
        b.Property(x => x.AuthorizationGroup).HasMaxLength(60);
        b.Property(x => x.PrimaryCurrency).HasMaxLength(10);

        b.HasOne(x => x.BasePriceList)
         .WithMany()
         .HasForeignKey(x => x.BasePriceListId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
