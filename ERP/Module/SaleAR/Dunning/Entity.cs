using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.SaleAR.Dunning;

// ═══════════════════════════════════════════════════════════════
// DUNNING LETTER  (KDUN / DUN1)  ·  SAP B1 equivalent: OCDN wizard output
// The record a dunning run produces: one letter per customer per
// run, at the run's recommended level, listing every overdue AR
// invoice that earned it. Informational only — no GL impact, same
// as real SAP B1 dunning. ARInvoiceId is a plain indexed int, not
// an EF FK: a dunning letter must never block deleting an invoice
// (or vice versa) the way a Restrict FK would.
// ═══════════════════════════════════════════════════════════════
public class DunningLetter : AuditableEntity
{
    public string   LetterNo         { get; set; } = null!;  // "DUN-2026-00001"
    public string    CustomerCode    { get; set; } = null!;
    public string    CustomerName    { get; set; } = null!;
    public string    DunningTermCode { get; set; } = null!;
    public int       Level           { get; set; }
    public string    LetterText      { get; set; } = null!;  // the matched level's text, snapshotted at run time
    public DateTime  RunDate         { get; set; }
    public decimal   TotalOverdue    { get; set; }
    public string?   Remarks         { get; set; }

    public ICollection<DunningLetterLine> Lines { get; set; } = new List<DunningLetterLine>();
}

public class DunningLetterLine
{
    public int      Id             { get; set; }
    public int      DunningLetterId{ get; set; }
    public int      ARInvoiceId    { get; set; }
    public string   DocNum         { get; set; } = null!;
    public DateTime? DueDate       { get; set; }
    public int      DaysOverdue    { get; set; }
    public decimal  Balance        { get; set; }

    public DunningLetter DunningLetter { get; set; } = null!;
}

public class DunningLetterConfig : IEntityTypeConfiguration<DunningLetter>
{
    public void Configure(EntityTypeBuilder<DunningLetter> b)
    {
        b.ToTable("KDUN");
        b.HasKey(x => x.Id);
        b.Property(x => x.LetterNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.LetterNo).IsUnique();
        b.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.DunningTermCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.LetterText).HasMaxLength(4000);
        b.Property(x => x.TotalOverdue).HasPrecision(18, 2);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.HasIndex(x => x.CustomerCode);
        b.HasIndex(x => x.RunDate);

        b.HasMany(x => x.Lines).WithOne(l => l.DunningLetter)
         .HasForeignKey(l => l.DunningLetterId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DunningLetterLineConfig : IEntityTypeConfiguration<DunningLetterLine>
{
    public void Configure(EntityTypeBuilder<DunningLetterLine> b)
    {
        b.ToTable("DUN1");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocNum).HasMaxLength(30);
        b.Property(x => x.Balance).HasPrecision(18, 2);
        b.HasIndex(x => x.ARInvoiceId);
    }
}
