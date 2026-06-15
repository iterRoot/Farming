using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockTransferRequest;

// ═══════════════════════════════════════════════════════════════════
// STOCK TRANSFER REQUEST HEADER (KSTR)
// Request to move inventory from one warehouse to another
// Status: O=Open, A=Approved, D=Delivered, C=Closed, X=Cancelled
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequest : AuditableEntity
{
    // ── Numbering ─────────────────────────────────────────────────
    public string   DocNo          { get; set; } = null!;   // Auto: STR-2026-00001
    public string   Series         { get; set; } = "Primary";

    // ── Status ────────────────────────────────────────────────────
    public string   Status         { get; set; } = "O";    // O=Open A=Approved D=Delivered C=Closed X=Cancelled

    // ── Dates ─────────────────────────────────────────────────────
    public DateTime PostingDate    { get; set; }
    public DateTime DueDate        { get; set; }
    public DateTime DocumentDate   { get; set; }

    // ── Business Partner ──────────────────────────────────────────
    public string?  CardCode       { get; set; }   // Business Partner
    public string?  CardName       { get; set; }   // Name
    public string?  ContactPerson  { get; set; }
    public string?  ShipTo         { get; set; }   // Ship To address

    // ── Warehouse ─────────────────────────────────────────────────
    public string   FromWarehouse  { get; set; } = "01";   // Default From Warehouse
    public string   ToWarehouse    { get; set; } = "01";   // Default To Warehouse

    // ── Pricing ───────────────────────────────────────────────────
    public string?  PriceList      { get; set; } = "Last Purchase Price";

    // ── References ────────────────────────────────────────────────
    public string?  ReferencedDoc  { get; set; }
    public int?     BaseDocEntry   { get; set; }
    public string?  BaseDocType    { get; set; }

    // ── People ────────────────────────────────────────────────────
    public string?  SalesEmployee  { get; set; }

    // ── Remarks ───────────────────────────────────────────────────
    public string?  JournalRemarks { get; set; }
    public string?  PickPackRemarks{ get; set; }
    public string?  Remarks        { get; set; }

    // ── Totals ────────────────────────────────────────────────────
    public decimal  TotalQuantity  { get; set; }

    public int      VersionNum     { get; set; } = 1;
    public int?     UserSign       { get; set; }

    // Navigation
    public ICollection<StockTransferRequestLine> Lines { get; set; } = new List<StockTransferRequestLine>();
}

// ═══════════════════════════════════════════════════════════════════
// STOCK TRANSFER REQUEST LINE (STR1)
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestLine : AuditableEntity
{
    public int      StockTransferRequestId { get; set; }
    public int      LineNum                { get; set; }

    // Item
    public string   ItemNo                 { get; set; } = null!;
    public string?  ItemDescription        { get; set; }

    // Warehouses — can override header defaults per line
    public string?  FromWarehouse          { get; set; }
    public string?  ToWarehouse            { get; set; }

    // Quantity & UoM
    public decimal  Quantity               { get; set; }
    public string?  UoMCode               { get; set; }
    public string?  UoMName               { get; set; }

    // Price
    public decimal  UnitPrice             { get; set; }
    public decimal  LineTotal             { get; set; }

    // Status per line
    public string   LineStatus            { get; set; } = "O";  // O=Open, D=Delivered, C=Closed

    public string?  Remarks               { get; set; }

    // Navigation
    public StockTransferRequest StockTransferRequest { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestConfig : IEntityTypeConfiguration<StockTransferRequest>
{
    public void Configure(EntityTypeBuilder<StockTransferRequest> builder)
    {
        builder.ToTable("KSTR");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.DocNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.DocNo).IsUnique();
        builder.Property(m => m.Series).HasMaxLength(20).HasDefaultValue("Primary");
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("O");

        builder.Property(m => m.CardCode).HasMaxLength(50);
        builder.Property(m => m.CardName).HasMaxLength(200);
        builder.Property(m => m.ContactPerson).HasMaxLength(200);
        builder.Property(m => m.ShipTo).HasMaxLength(500);

        builder.Property(m => m.FromWarehouse).HasMaxLength(10).HasDefaultValue("01");
        builder.Property(m => m.ToWarehouse).HasMaxLength(10).HasDefaultValue("01");
        builder.Property(m => m.PriceList).HasMaxLength(100);

        builder.Property(m => m.ReferencedDoc).HasMaxLength(100);
        builder.Property(m => m.BaseDocType).HasMaxLength(10);
        builder.Property(m => m.SalesEmployee).HasMaxLength(200);
        builder.Property(m => m.JournalRemarks).HasMaxLength(500);
        builder.Property(m => m.PickPackRemarks).HasMaxLength(500);
        builder.Property(m => m.Remarks).HasMaxLength(500);

        builder.Property(m => m.TotalQuantity).HasPrecision(18, 4);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.PostingDate);
        builder.HasIndex(m => m.CardCode);
        builder.HasIndex(m => new { m.FromWarehouse, m.ToWarehouse });
    }
}

public class StockTransferRequestLineConfig : IEntityTypeConfiguration<StockTransferRequestLine>
{
    public void Configure(EntityTypeBuilder<StockTransferRequestLine> builder)
    {
        builder.ToTable("STR1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemNo).HasMaxLength(50).IsRequired();
        builder.Property(m => m.ItemDescription).HasMaxLength(200);
        builder.Property(m => m.FromWarehouse).HasMaxLength(10);
        builder.Property(m => m.ToWarehouse).HasMaxLength(10);
        builder.Property(m => m.UoMCode).HasMaxLength(20);
        builder.Property(m => m.UoMName).HasMaxLength(100);
        builder.Property(m => m.LineStatus).HasMaxLength(1).HasDefaultValue("O");
        builder.Property(m => m.Remarks).HasMaxLength(200);

        builder.Property(m => m.Quantity).HasPrecision(18, 4);
        builder.Property(m => m.UnitPrice).HasPrecision(18, 4);
        builder.Property(m => m.LineTotal).HasPrecision(18, 4);

        builder.HasOne(m => m.StockTransferRequest)
            .WithMany(s => s.Lines)
            .HasForeignKey(m => m.StockTransferRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.StockTransferRequestId);
        builder.HasIndex(m => m.ItemNo);
    }
}