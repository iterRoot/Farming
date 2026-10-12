using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.InventoryManagement.StockRevaluation;

// Stock revaluation document — sets a new moving-average cost for items and
// posts the resulting inventory-value variance. Header KSRV, cost lines SRV1,
// serial/batch lines SRV2.
//
// Documents saved before costs were applied (CostsApplied = false) are
// records only; they changed no cost and posted nothing.
public class StockRevaluation : AuditableEntity
{
    public string    Number          { get; set; } = null!;   // SREV-2026-00001
    public string?   Series          { get; set; }
    public DateTime? PostingDate     { get; set; }
    public DateTime? DocumentDate    { get; set; }
    public string?   RevaluationType { get; set; }            // Price Change | ...
    public string?   Ref2            { get; set; }
    public bool      InflationBased  { get; set; }
    public int?      ReferencedDocumentId { get; set; }
    public string?   ReferencedDocumentLabel { get; set; }
    public string    Status          { get; set; } = "Posted";  // Posted | Cancelled
    public string?   Remarks         { get; set; }
    public string?   JournalRemark   { get; set; }
    public bool      CostsApplied    { get; set; }
    public int?      JournalEntryId  { get; set; }   // null when the total variance was zero

    public ICollection<StockRevaluationLine>       Lines            { get; set; } = new List<StockRevaluationLine>();
    public ICollection<StockRevaluationSerialLine> SerialBatchLines { get; set; } = new List<StockRevaluationSerialLine>();
}

public class StockRevaluationLine : AuditableEntity
{
    public int      StockRevaluationId { get; set; }
    public int      LineNum            { get; set; }
    public string?  ItemNo             { get; set; }
    public string?  ItemDescription    { get; set; }
    public string?  WhsCode            { get; set; }
    public decimal  CurrentCost        { get; set; }
    public decimal  NewCost            { get; set; }
    public string?  Uom                { get; set; }
    public decimal  InStock            { get; set; }

    public StockRevaluation StockRevaluation { get; set; } = null!;
}

public class StockRevaluationSerialLine : AuditableEntity
{
    public int      StockRevaluationId { get; set; }
    public int      LineNum            { get; set; }
    public string?  SbNo               { get; set; }
    public DateTime? AdmissionDate     { get; set; }
    public DateTime? ExpiryDate        { get; set; }
    public decimal  CurrentCost        { get; set; }
    public decimal  NewCost            { get; set; }
    public decimal  OpenQty            { get; set; }

    public StockRevaluation StockRevaluation { get; set; } = null!;
}

public class StockRevaluationConfig : IEntityTypeConfiguration<StockRevaluation>
{
    public void Configure(EntityTypeBuilder<StockRevaluation> b)
    {
        b.ToTable("KSRV");
        b.HasKey(x => x.Id);
        b.Property(m => m.Number).HasMaxLength(30).IsRequired();
        b.HasIndex(m => m.Number).IsUnique();
        b.Property(m => m.Series).HasMaxLength(40);
        b.Property(m => m.RevaluationType).HasMaxLength(40);
        b.Property(m => m.Ref2).HasMaxLength(60);
        b.Property(m => m.ReferencedDocumentLabel).HasMaxLength(120);
        b.Property(m => m.Status).HasMaxLength(15).HasDefaultValue("Posted");
        b.Property(m => m.Remarks).HasMaxLength(500);
        b.Property(m => m.JournalRemark).HasMaxLength(200);
        b.HasIndex(m => m.Status);
    }
}

public class StockRevaluationLineConfig : IEntityTypeConfiguration<StockRevaluationLine>
{
    public void Configure(EntityTypeBuilder<StockRevaluationLine> b)
    {
        b.ToTable("SRV1");
        b.HasKey(x => x.Id);
        b.Property(m => m.ItemNo).HasMaxLength(60);
        b.Property(m => m.ItemDescription).HasMaxLength(200);
        b.Property(m => m.WhsCode).HasMaxLength(20);
        b.Property(m => m.Uom).HasMaxLength(20);
        b.Property(m => m.CurrentCost).HasPrecision(18, 4);
        b.Property(m => m.NewCost).HasPrecision(18, 4);
        b.Property(m => m.InStock).HasPrecision(18, 4);
        b.HasOne(m => m.StockRevaluation)
            .WithMany(s => s.Lines)
            .HasForeignKey(m => m.StockRevaluationId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(m => m.StockRevaluationId);
    }
}

public class StockRevaluationSerialLineConfig : IEntityTypeConfiguration<StockRevaluationSerialLine>
{
    public void Configure(EntityTypeBuilder<StockRevaluationSerialLine> b)
    {
        b.ToTable("SRV2");
        b.HasKey(x => x.Id);
        b.Property(m => m.SbNo).HasMaxLength(60);
        b.Property(m => m.CurrentCost).HasPrecision(18, 4);
        b.Property(m => m.NewCost).HasPrecision(18, 4);
        b.Property(m => m.OpenQty).HasPrecision(18, 4);
        b.HasOne(m => m.StockRevaluation)
            .WithMany(s => s.SerialBatchLines)
            .HasForeignKey(m => m.StockRevaluationId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(m => m.StockRevaluationId);
    }
}
