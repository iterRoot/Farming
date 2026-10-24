using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.PurchaseRequest;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
//
// The pre-PO internal requisition: a department/requester asks for items to
// be bought, before any vendor or price is settled. A vendor can be attached
// once one is chosen, but isn't required to raise the request — unlike every
// other Purchase-AP document, which is always vendor-first.
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseRequest : AuditableEntity
{
    public string    DocNum       { get; set; } = "";
    public int?      VendorId     { get; set; }   // optional — may not be chosen yet
    public BPEntity?  Vendor       { get; set; }
    public string?   BuyerCode    { get; set; }   // buyer (KSEB code)
    public string?   RequestedBy  { get; set; }
    public string?   Department   { get; set; }
    public DateTime? PostingDate  { get; set; }
    public DateTime? NeededByDate { get; set; }
    public string    Status       { get; set; } = "O";   // O=Open, C=Closed, D=Draft
    public string    Type         { get; set; } = "Item"; // Item | Service
    public decimal   Total        { get; set; }
    public string?   Remarks      { get; set; }

    // ── Lines ─────────────────────────────────────────────────────────────
    public ICollection<PurchaseRequestLine> Items { get; set; } = new List<PurchaseRequestLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseRequestLine : AuditableEntity
{
    public int             PurchaseRequestId { get; set; }
    public PurchaseRequest PurchaseRequest   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }   // estimated unit price — no vendor quote yet
    public decimal  Total    { get; set; }
    public string?  Remark   { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class PurchaseRequestConfig : IEntityTypeConfiguration<PurchaseRequest>
{
    public void Configure(EntityTypeBuilder<PurchaseRequest> builder)
    {
        builder.ToTable("KPPR");   // Purchase Request header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();

        // Document numbers come from the reserved series, so a collision
        // means a bug in reservation — fail loudly rather than silently
        // storing two documents under one number.
        builder.HasIndex(x => x.DocNum).IsUnique();
        builder.Property(x => x.BuyerCode).HasMaxLength(50);
        builder.Property(x => x.RequestedBy).HasMaxLength(200);
        builder.Property(x => x.Department).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(10);
        builder.Property(x => x.Type).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasCheckConstraint("CK_PurchaseRequest_Status", "\"Status\" IN ('O','C','D','P')");

        builder.HasOne(x => x.Vendor)
               .WithMany()
               .HasForeignKey(x => x.VendorId)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasMany(x => x.Items)
               .WithOne(x => x.PurchaseRequest)
               .HasForeignKey(x => x.PurchaseRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PurchaseRequestLineConfig : IEntityTypeConfiguration<PurchaseRequestLine>
{
    public void Configure(EntityTypeBuilder<PurchaseRequestLine> builder)
    {
        builder.ToTable("PPR1");   // Purchase Request lines
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode).HasMaxLength(100);
        builder.Property(x => x.ItemName).HasMaxLength(200);
        builder.Property(x => x.Remark).HasMaxLength(500);
        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Total).HasPrecision(18, 2);

        builder.HasOne(x => x.Item)
               .WithMany()
               .HasForeignKey(x => x.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
