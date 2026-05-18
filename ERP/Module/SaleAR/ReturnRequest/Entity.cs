using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;

// ✅ Alias to avoid namespace/class name conflict
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.ReturnRequest;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnRequest : AuditableEntity
{
    public string    DocNum      { get; set; } = "";
    public DateTime? PostingDate { get; set; }
    public DateTime? DueDate     { get; set; }
    public string    Status      { get; set; } = "O";    // O=Open, C=Closed
    public string    Type        { get; set; } = "Item"; // Item | Service
    public decimal   Discount    { get; set; }
    public decimal   Tax         { get; set; }
    public decimal   TaxAmount   { get; set; }
    public decimal   Total       { get; set; }
    public string?   Remarks     { get; set; }

    // ── FK → BusinessPartner ───────────────────────────────────────────────
    public int      CustomerId { get; set; }
    public BPEntity Customer   { get; set; } = null!;  // ✅ alias

    // ── Lines ──────────────────────────────────────────────────────────────
    public ICollection<ReturnRequestLine> Items { get; set; } = new List<ReturnRequestLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// LINE
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnRequestLine : AuditableEntity
{
    public int       ReturnRequestId { get; set; }
    public ReturnRequest ReturnRequest   { get; set; } = null!;

    public int         ItemId { get; set; }
    public ItemsMaster Item   { get; set; } = null!;

    public string?  ItemCode { get; set; }
    public string?  ItemName { get; set; }
    public decimal  Quantity { get; set; }
    public decimal  Price    { get; set; }
    public decimal  Total    { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class ReturnRequestConfig : IEntityTypeConfiguration<ReturnRequest>
{
    public void Configure(EntityTypeBuilder<ReturnRequest> builder)
    {
        builder.ToTable("KARR"); //KARR returnrequest Table
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Status).HasMaxLength(10);
        builder.Property(m => m.Type).HasMaxLength(50);
        builder.Property(m => m.Remarks).HasMaxLength(500);
        builder.Property(m => m.Discount).HasPrecision(18, 2);
        builder.Property(m => m.Tax).HasPrecision(18, 2);
        builder.Property(m => m.TaxAmount).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Customer)
               .WithMany()
               .HasForeignKey(m => m.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(m => m.Items)
               .WithOne(l => l.ReturnRequest)
               .HasForeignKey(l => l.ReturnRequestId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReturnRequestLineConfig : IEntityTypeConfiguration<ReturnRequestLine>
{
    public void Configure(EntityTypeBuilder<ReturnRequestLine> builder)
    {
        builder.ToTable("ARR1");//KARR
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemCode).HasMaxLength(100);
        builder.Property(m => m.ItemName).HasMaxLength(200);
        builder.Property(m => m.Quantity).HasPrecision(18, 2);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.Total).HasPrecision(18, 2);

        builder.HasOne(m => m.Item)
               .WithMany()
               .HasForeignKey(m => m.ItemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}