using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockTransfer;

// ═══════════════════════════════════════════════════════════════════
// STOCK TRANSFER HEADER (KSTO)
// Actual physical movement of stock between warehouses/bin locations
// Differences from Stock Transfer Request:
//   • Has Bin Location (shelf-level within warehouse)
//   • Has "Copy From" (source from STR or another doc)
//   • No Due Date — only Posting Date + Document Date
//   • Journal Remarks default: "Stock Transfers -"
// ═══════════════════════════════════════════════════════════════════
public class StockTransfer : AuditableEntity
{
    // ── Numbering ─────────────────────────────────────────────────
    public int      Number         { get; set; }            // Auto-number: 34
    public string   DocNo          { get; set; } = null!;   // STO-2026-00001
    public string   Series         { get; set; } = "Primary";

    // ── Dates ─────────────────────────────────────────────────────
    public DateTime PostingDate    { get; set; }
    public DateTime DocumentDate   { get; set; }

    // ── Business Partner ──────────────────────────────────────────
    public string?  CardCode       { get; set; }
    public string?  CardName       { get; set; }   // Name
    public string?  ContactPerson  { get; set; }
    public string?  ShipTo         { get; set; }

    // ── Warehouse & Bin Location ──────────────────────────────────
    public string   FromWarehouse  { get; set; } = "01";
    public string   ToWarehouse    { get; set; } = "01";
    public string?  ToBinLocation  { get; set; }   // Bin within To Warehouse

    // ── Pricing ───────────────────────────────────────────────────
    public string?  PriceList      { get; set; } = "Last Purchase Price";

    // ── Copy From reference ───────────────────────────────────────
    public string?  ReferencedDoc  { get; set; }
    public int?     BaseDocEntry   { get; set; }   // Source STR ID
    public string?  BaseDocType    { get; set; }   // KSTR = from Stock Transfer Request

    // ── People ────────────────────────────────────────────────────
    public string?  SalesEmployee  { get; set; }

    // ── Remarks ───────────────────────────────────────────────────
    public string?  JournalRemarks { get; set; }
    public string?  Remarks        { get; set; }

    // ── Totals ────────────────────────────────────────────────────
    public decimal  TotalQuantity  { get; set; }

    public int      VersionNum     { get; set; } = 1;
    public int?     UserSign       { get; set; }

    // Navigation
    public ICollection<StockTransferLine> Lines { get; set; } = new List<StockTransferLine>();
}

// ═══════════════════════════════════════════════════════════════════
// STOCK TRANSFER LINE (STO1)
// ═══════════════════════════════════════════════════════════════════
public class StockTransferLine : AuditableEntity
{
    public int      StockTransferId    { get; set; }
    public int      LineNum            { get; set; }

    // Item
    public string   ItemNo             { get; set; } = null!;
    public string?  ItemDescription    { get; set; }

    // From
    public string?  FromWarehouse      { get; set; }
    public string?  FromBinLocation    { get; set; }   // Bin/shelf source

    // To
    public string?  ToWarehouse        { get; set; }
    public string?  ToBinLocation      { get; set; }   // Bin/shelf destination

    // Quantity & UoM
    public decimal  Quantity           { get; set; }
    public string?  UoMCode           { get; set; }
    public string?  UoMName           { get; set; }

    // Price
    public decimal  UnitPrice          { get; set; }
    public decimal  FirstPrice         { get; set; }   // "First ..." column in SAP
    public decimal  LineTotal          { get; set; }

    public string?  Remarks            { get; set; }

    // Navigation
    public StockTransfer StockTransfer { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class StockTransferConfig : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("KSTO");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Number).ValueGeneratedOnAdd();
        builder.HasIndex(m => m.Number).IsUnique();
        builder.Property(m => m.DocNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.DocNo).IsUnique();
        builder.Property(m => m.Series).HasMaxLength(20).HasDefaultValue("Primary");

        builder.Property(m => m.CardCode).HasMaxLength(50);
        builder.Property(m => m.CardName).HasMaxLength(200);
        builder.Property(m => m.ContactPerson).HasMaxLength(200);
        builder.Property(m => m.ShipTo).HasMaxLength(500);

        builder.Property(m => m.FromWarehouse).HasMaxLength(10).HasDefaultValue("01");
        builder.Property(m => m.ToWarehouse).HasMaxLength(10).HasDefaultValue("01");
        builder.Property(m => m.ToBinLocation).HasMaxLength(50);

        builder.Property(m => m.PriceList).HasMaxLength(100);
        builder.Property(m => m.ReferencedDoc).HasMaxLength(100);
        builder.Property(m => m.BaseDocType).HasMaxLength(10);
        builder.Property(m => m.SalesEmployee).HasMaxLength(200);
        builder.Property(m => m.JournalRemarks).HasMaxLength(500);
        builder.Property(m => m.Remarks).HasMaxLength(500);

        builder.Property(m => m.TotalQuantity).HasPrecision(18, 4);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.HasIndex(m => m.PostingDate);
        builder.HasIndex(m => m.CardCode);
        builder.HasIndex(m => new { m.FromWarehouse, m.ToWarehouse });
    }
}

public class StockTransferLineConfig : IEntityTypeConfiguration<StockTransferLine>
{
    public void Configure(EntityTypeBuilder<StockTransferLine> builder)
    {
        builder.ToTable("STO1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemNo).HasMaxLength(50).IsRequired();
        builder.Property(m => m.ItemDescription).HasMaxLength(200);
        builder.Property(m => m.FromWarehouse).HasMaxLength(10);
        builder.Property(m => m.FromBinLocation).HasMaxLength(50);
        builder.Property(m => m.ToWarehouse).HasMaxLength(10);
        builder.Property(m => m.ToBinLocation).HasMaxLength(50);
        builder.Property(m => m.UoMCode).HasMaxLength(20);
        builder.Property(m => m.UoMName).HasMaxLength(100);
        builder.Property(m => m.Remarks).HasMaxLength(200);

        builder.Property(m => m.Quantity).HasPrecision(18, 4);
        builder.Property(m => m.UnitPrice).HasPrecision(18, 4);
        builder.Property(m => m.FirstPrice).HasPrecision(18, 4);
        builder.Property(m => m.LineTotal).HasPrecision(18, 4);

        builder.HasOne(m => m.StockTransfer)
            .WithMany(s => s.Lines)
            .HasForeignKey(m => m.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.StockTransferId);
        builder.HasIndex(m => m.ItemNo);
    }
}