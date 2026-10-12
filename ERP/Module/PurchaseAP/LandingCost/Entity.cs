using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using GoodsReceiptPOEntity = FarmingApi.Modules.PurchaseAP.GoodsReceiptPO.GoodsReceiptPO;

namespace FarmingApi.Modules.PurchaseAP.LandingCost;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER — allocates a set of extra costs (freight, customs duty, insurance,
// handling...) billed by one vendor across one or more Goods Receipt POs,
// proportional to each line's own received value (Quantity * Price). Posting
// layers the allocated per-unit amount straight onto ItemsMaster.ItemCost, the
// same field InventoryJournal/PostingService.cs already maintains as a moving
// average on every stock-in — this is a flat add to that average, not a
// re-weighted blend, since no lot tracking exists anywhere in the app to know
// how much of a given receipt's quantity is still on hand.
// ══════════════════════════════════════════════════════════════════════════════
public class LandedCost : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DocDate     { get; set; }
    public string    Status      { get; set; } = "D";   // D=Draft, O=Posted

    public int       VendorId { get; set; }              // who billed the landed costs
    public BPEntity   Vendor  { get; set; } = null!;

    public decimal   Total   { get; set; }                // sum of CostLines, snapshot at Post
    public string?   Remarks { get; set; }

    public ICollection<LandedCostBaseDoc> BaseDocs  { get; set; } = new List<LandedCostBaseDoc>();
    public ICollection<LandedCostLine>    CostLines { get; set; } = new List<LandedCostLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// BASE DOCS — the Goods Receipt PO(s) this Landed Cost allocates across.
// ══════════════════════════════════════════════════════════════════════════════
public class LandedCostBaseDoc : AuditableEntity
{
    public int            LandedCostId { get; set; }
    public LandedCost     LandedCost   { get; set; } = null!;

    public int                 GoodsReceiptPOId { get; set; }
    public GoodsReceiptPOEntity GoodsReceiptPO  { get; set; } = null!;
}

// ══════════════════════════════════════════════════════════════════════════════
// COST LINES — the individual extra charges being allocated (Freight $500,
// Customs Duty $120...).
// ══════════════════════════════════════════════════════════════════════════════
public class LandedCostLine : AuditableEntity
{
    public int         LandedCostId { get; set; }
    public LandedCost  LandedCost   { get; set; } = null!;

    public string  CostType { get; set; } = "Freight";  // Freight | CustomsDuty | Insurance | Handling | Other
    public decimal Amount   { get; set; }
    public string? Remarks  { get; set; }
}

// ─── EF CONFIG ─────────────────────────────────────────────────────────────
public class LandedCostConfig : IEntityTypeConfiguration<LandedCost>
{
    public void Configure(EntityTypeBuilder<LandedCost> b)
    {
        b.ToTable("KLDC");
        b.HasKey(x => x.Id);

        b.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasMaxLength(5).IsRequired();
        b.Property(x => x.Total).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasOne(x => x.Vendor)
         .WithMany()
         .HasForeignKey(x => x.VendorId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.BaseDocs)
         .WithOne(x => x.LandedCost)
         .HasForeignKey(x => x.LandedCostId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.CostLines)
         .WithOne(x => x.LandedCost)
         .HasForeignKey(x => x.LandedCostId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasCheckConstraint("CK_LandedCost_Status", "\"Status\" IN ('D','O')");
    }
}

public class LandedCostBaseDocConfig : IEntityTypeConfiguration<LandedCostBaseDoc>
{
    public void Configure(EntityTypeBuilder<LandedCostBaseDoc> b)
    {
        b.ToTable("LDC1");
        b.HasKey(x => x.Id);

        b.HasOne(x => x.GoodsReceiptPO)
         .WithMany()
         .HasForeignKey(x => x.GoodsReceiptPOId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => new { x.LandedCostId, x.GoodsReceiptPOId }).IsUnique();
    }
}

public class LandedCostLineConfig : IEntityTypeConfiguration<LandedCostLine>
{
    public void Configure(EntityTypeBuilder<LandedCostLine> b)
    {
        b.ToTable("LDC2");
        b.HasKey(x => x.Id);

        b.Property(x => x.CostType).HasMaxLength(50).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}
