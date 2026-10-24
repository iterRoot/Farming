using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ═══════════════════════════════════════════════════════════════
// INVENTORY JOURNAL — one row per stock-affecting transaction.
// Written only by IInventoryPostingService from documents that move
// stock (Goods Receipt PO, Delivery, ...). Qty/value balances are
// carried on each row so history never needs to be replayed.
// ═══════════════════════════════════════════════════════════════
public class InventoryJournal : AuditableEntity
{
    public string   ItemCode     { get; set; } = null!;
    public string   WhsCode      { get; set; } = null!;
    public DateTime PostingDate  { get; set; } = DateTime.UtcNow;

    public string   TransType    { get; set; } = null!; // "Goods Receipt PO", "Delivery", ...
    public string   BaseDocType  { get; set; } = null!; // "GoodsReceiptPO", "Delivery", ...
    public int      BaseDocEntry { get; set; }
    public string?  BaseDocNum   { get; set; }

    // Who the movement was with, captured at posting time. Denormalised on
    // purpose: the ledger is a historical record, so it must not change if the
    // document is later reassigned to a different business partner.
    public string?  CardCode     { get; set; }
    public string?  CardName     { get; set; }

    public decimal InQty        { get; set; }
    public decimal OutQty       { get; set; }
    public decimal UnitCost     { get; set; }
    public decimal TransValue   { get; set; }   // signed: +in value, -out value
    public decimal QtyBalance   { get; set; }   // running qty balance after this row
    public decimal ValueBalance { get; set; }   // running value balance after this row

    // Standard-cost items only: (actual unit cost − standard) × qty on a receipt.
    // Positive = paid more than standard (unfavourable).
    public decimal PriceVariance { get; set; }
}

// ═══════════════════════════════════════════════════════════════
// COST LAYER (KCLY) — one receipt lot of a FIFO-valued item in one
// warehouse. Issues consume the oldest layers first; RemainingQty
// is what's left of the lot. Only FIFO items have layers.
// ═══════════════════════════════════════════════════════════════
public class InventoryCostLayer
{
    public int      Id             { get; set; }
    public string   ItemCode       { get; set; } = null!;
    public string   WhsCode        { get; set; } = null!;
    public DateTime ReceivedDate   { get; set; }
    public string   SourceDocType  { get; set; } = null!;   // "Opening" for the layer made when an item switches to FIFO
    public int      SourceDocEntry { get; set; }
    public string?  SourceDocNum   { get; set; }
    public decimal  InitialQty     { get; set; }
    public decimal  RemainingQty   { get; set; }
    public decimal  UnitCost       { get; set; }
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;
}

public class InventoryCostLayerConfig : IEntityTypeConfiguration<InventoryCostLayer>
{
    public void Configure(EntityTypeBuilder<InventoryCostLayer> b)
    {
        b.ToTable("KCLY");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.WhsCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.SourceDocType).HasMaxLength(50).IsRequired();
        b.Property(x => x.SourceDocNum).HasMaxLength(100);
        b.Property(x => x.InitialQty).HasPrecision(18, 4);
        b.Property(x => x.RemainingQty).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 6);
        b.HasIndex(x => new { x.ItemCode, x.WhsCode, x.ReceivedDate, x.Id });
    }
}

public class InventoryJournalConfig : IEntityTypeConfiguration<InventoryJournal>
{
    public void Configure(EntityTypeBuilder<InventoryJournal> b)
    {
        b.ToTable("KINM");
        b.HasKey(x => x.Id);

        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.WhsCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.TransType).HasMaxLength(50).IsRequired();
        b.Property(x => x.BaseDocType).HasMaxLength(50).IsRequired();
        b.Property(x => x.BaseDocNum).HasMaxLength(100);
        b.Property(x => x.CardCode).HasMaxLength(50);
        b.Property(x => x.CardName).HasMaxLength(200);

        b.Property(x => x.InQty).HasPrecision(18, 4);
        b.Property(x => x.OutQty).HasPrecision(18, 4);
        b.Property(x => x.UnitCost).HasPrecision(18, 4);
        b.Property(x => x.TransValue).HasPrecision(18, 4);
        b.Property(x => x.QtyBalance).HasPrecision(18, 4);
        b.Property(x => x.ValueBalance).HasPrecision(18, 4);
        b.Property(x => x.PriceVariance).HasPrecision(18, 4);

        b.HasIndex(x => new { x.ItemCode, x.WhsCode, x.PostingDate });
        b.HasIndex(x => new { x.BaseDocType, x.BaseDocEntry });
    }
}
