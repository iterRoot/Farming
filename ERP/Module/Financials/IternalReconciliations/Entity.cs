using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Financials.IternalReconciliations;

// ══════════════════════════════════════════════════════════════════════════════
// BP RECONCILIATION HEADER (KBPR)
//
// PURPOSE: Match a business partner's AR/AP activity against itself — pair an
// invoice against the credit note or payment that settles it — the same
// mechanics as Bank Reconciliation (BankReconciliation/Entity.cs), just
// reconciling a BP subledger against itself instead of the book against an
// external bank statement, so there is no separate "statement balance" side.
// ══════════════════════════════════════════════════════════════════════════════
public class BPReconciliation : AuditableEntity
{
    public string   ReconcNo        { get; set; } = null!;   // Auto: BPR-2026-00001
    public int      PartnerId       { get; set; }
    public BPEntity Partner         { get; set; } = null!;
    public string   PartnerCode     { get; set; } = string.Empty;
    public string   PartnerName     { get; set; } = string.Empty;
    public string   Currency        { get; set; } = "USD";
    public DateTime StartDate       { get; set; }
    public DateTime EndDate         { get; set; }
    public string   Status          { get; set; } = "O";     // O=Open, C=Closed

    public decimal  OpeningBalance  { get; set; }
    public decimal  ClosingBalance  { get; set; }
    public decimal  ReconciledAmt   { get; set; }
    public decimal  UnreconciledAmt { get; set; }

    public string?  Remarks         { get; set; }
    public int      VersionNum      { get; set; } = 1;

    public ICollection<BPReconciliationLine> Lines { get; set; } = new List<BPReconciliationLine>();
}

// ══════════════════════════════════════════════════════════════════════════════
// BP RECONCILIATION LINE (BPR1) — one line per AR/AP transaction being matched
// ══════════════════════════════════════════════════════════════════════════════
public class BPReconciliationLine : AuditableEntity
{
    public int      BPReconciliationId { get; set; }
    public int      LineNum            { get; set; }

    public DateTime TransDate          { get; set; }
    public string?  RefNo              { get; set; }
    public string?  Description        { get; set; }

    public string?  DocType            { get; set; }   // KARI, KACN, KAPI, KAPC, KPYI, KPYO
    public int?     DocEntry           { get; set; }
    public string?  DocNo              { get; set; }

    public decimal  Debit              { get; set; }   // increases what the partner owes (or we owe them)
    public decimal  Credit             { get; set; }   // settles it (payment, credit note)
    public decimal  Balance            { get; set; }

    public bool      IsReconciled      { get; set; } = false;
    public DateTime? ReconciledDate    { get; set; }
    public string?   ReconciledRef     { get; set; }

    public string?  Remarks            { get; set; }

    public BPReconciliation BPReconciliation { get; set; } = null!;
}

// ─── EF CONFIG ─────────────────────────────────────────────────────────────
public class BPReconciliationConfig : IEntityTypeConfiguration<BPReconciliation>
{
    public void Configure(EntityTypeBuilder<BPReconciliation> b)
    {
        b.ToTable("KBPR");
        b.HasKey(x => x.Id);

        b.Property(x => x.ReconcNo).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.ReconcNo).IsUnique();
        b.Property(x => x.PartnerCode).HasMaxLength(50);
        b.Property(x => x.PartnerName).HasMaxLength(200);
        b.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("USD");
        b.Property(x => x.Status).HasMaxLength(1).HasDefaultValue("O");
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.OpeningBalance).HasPrecision(18, 2);
        b.Property(x => x.ClosingBalance).HasPrecision(18, 2);
        b.Property(x => x.ReconciledAmt).HasPrecision(18, 2);
        b.Property(x => x.UnreconciledAmt).HasPrecision(18, 2);

        b.HasOne(x => x.Partner)
         .WithMany()
         .HasForeignKey(x => x.PartnerId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.PartnerId);
        b.HasIndex(x => x.Status);

        b.HasCheckConstraint("CK_BPReconciliation_Status", "\"Status\" IN ('O','C')");
    }
}

public class BPReconciliationLineConfig : IEntityTypeConfiguration<BPReconciliationLine>
{
    public void Configure(EntityTypeBuilder<BPReconciliationLine> b)
    {
        b.ToTable("BPR1");
        b.HasKey(x => x.Id);

        b.Property(x => x.RefNo).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.DocType).HasMaxLength(10);
        b.Property(x => x.DocNo).HasMaxLength(50);
        b.Property(x => x.ReconciledRef).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(200);

        b.Property(x => x.Debit).HasPrecision(18, 2);
        b.Property(x => x.Credit).HasPrecision(18, 2);
        b.Property(x => x.Balance).HasPrecision(18, 2);

        b.HasOne(x => x.BPReconciliation)
         .WithMany(h => h.Lines)
         .HasForeignKey(x => x.BPReconciliationId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.BPReconciliationId);
        b.HasIndex(x => x.IsReconciled);
        b.HasIndex(x => new { x.DocType, x.DocEntry });
    }
}
