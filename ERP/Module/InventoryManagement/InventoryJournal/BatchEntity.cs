using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ═══════════════════════════════════════════════════════════════
// BATCH / SERIAL NUMBERS
//   KBAT — what exists right now: one row per item + warehouse + batch number
//          (Quantity = on hand), or one row per serial number (Quantity 0 or 1).
//   KBTX — every movement of a number, written with the stock document that
//          moved it, so a number's whole history can be traced.
// Only items whose "Manage Item by" is Batch or Serial have rows here.
// ═══════════════════════════════════════════════════════════════
public class StockBatch
{
    public int       Id                { get; set; }
    public string    ItemCode          { get; set; } = null!;
    public string    WhsCode           { get; set; } = null!;
    public string    BatchNo           { get; set; } = null!;   // the batch number, or the serial number
    public string    Kind              { get; set; } = "Batch"; // Batch | Serial
    public decimal   Quantity          { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate        { get; set; }
    public DateTime  ReceivedDate      { get; set; }
    public DateTime  CreatedAt         { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt         { get; set; }
}

public class StockBatchMove
{
    public int       Id           { get; set; }
    public string    ItemCode     { get; set; } = null!;
    public string    WhsCode      { get; set; } = null!;
    public string    BatchNo      { get; set; } = null!;
    public decimal   Quantity     { get; set; }             // signed: + in, − out
    public DateTime  PostingDate  { get; set; }
    public string    TransType    { get; set; } = null!;
    public string    BaseDocType  { get; set; } = null!;
    public int       BaseDocEntry { get; set; }
    public string?   BaseDocNum   { get; set; }
    public string?   CardCode     { get; set; }
    public DateTime  CreatedAt    { get; set; } = DateTime.UtcNow;
}

public class StockBatchConfig : IEntityTypeConfiguration<StockBatch>
{
    public void Configure(EntityTypeBuilder<StockBatch> b)
    {
        b.ToTable("KBAT");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.WhsCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.BatchNo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Kind).HasMaxLength(10).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.HasIndex(x => new { x.ItemCode, x.WhsCode, x.BatchNo }).IsUnique();
        // a serial number exists once per item, whichever warehouse holds it
        b.HasIndex(x => new { x.ItemCode, x.BatchNo }).IsUnique().HasFilter("\"Kind\" = 'Serial'").HasDatabaseName("IX_KBAT_Serial");
        b.HasIndex(x => x.ExpiryDate);
    }
}

public class StockBatchMoveConfig : IEntityTypeConfiguration<StockBatchMove>
{
    public void Configure(EntityTypeBuilder<StockBatchMove> b)
    {
        b.ToTable("KBTX");
        b.HasKey(x => x.Id);
        b.Property(x => x.ItemCode).HasMaxLength(100).IsRequired();
        b.Property(x => x.WhsCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.BatchNo).HasMaxLength(100).IsRequired();
        b.Property(x => x.Quantity).HasPrecision(18, 4);
        b.Property(x => x.TransType).HasMaxLength(50).IsRequired();
        b.Property(x => x.BaseDocType).HasMaxLength(50).IsRequired();
        b.Property(x => x.BaseDocNum).HasMaxLength(100);
        b.Property(x => x.CardCode).HasMaxLength(50);
        b.HasIndex(x => new { x.ItemCode, x.BatchNo });
        b.HasIndex(x => new { x.BaseDocType, x.BaseDocEntry });
    }
}

// One batch (or serial) line entered against a document line.
public class BatchLine
{
    public string?   BatchNo           { get; set; }
    public decimal   Quantity          { get; set; } = 1;
    public DateTime? ExpiryDate        { get; set; }
    public DateTime? ManufacturingDate { get; set; }
}
