using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.JournalVoucher;

// ═══════════════════════════════════════════════════════════════════
// JOURNAL VOUCHER HEADER (KJVH)
// Draft/Batch Journal Entries before posting to Journal Entry
// ═══════════════════════════════════════════════════════════════════
public class JournalVoucher : AuditableEntity
{
    public string VoucherNo { get; set; } = null!;           // Auto: JV-2026-00001
    public DateTime VoucherDate { get; set; }                // Voucher Date
    public DateTime DueDate { get; set; }
    public string? RefNo { get; set; }                        // Reference Number
    public string? Description { get; set; }                  // Description/Title
    public string? Memo { get; set; }                         // Memo
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal ExchangeRate { get; set; } = 1;
    public string Status { get; set; } = "D";                // D=Draft, P=Posted, V=Void
    public int? PostedToJournalEntryId { get; set; }         // Link to JE after posting
    public DateTime? PostedDate { get; set; }
    public int? PostedBy { get; set; }
    public string? ProjectCode { get; set; }
    public string? CostCenter { get; set; }
    public int? UserSign { get; set; }
    public int? UserSign2 { get; set; }
    public int VersionNum { get; set; } = 1;

    // Approval
    public string ApprovalStatus { get; set; } = "N";        // N=None, P=Pending, A=Approved, R=Rejected
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNote { get; set; }

    // Navigation
    public ICollection<JournalVoucherLine> Lines { get; set; } = new List<JournalVoucherLine>();
}

// ═══════════════════════════════════════════════════════════════════
// JOURNAL VOUCHER LINE (JVH1)
// ═══════════════════════════════════════════════════════════════════
public class JournalVoucherLine : AuditableEntity
{
    public int JournalVoucherId { get; set; }
    public int LineNum { get; set; }
    public string AccountCode { get; set; } = null!;
    public string AccountName { get; set; } = null!;
    public string CardCode { get; set; }= null!;
    public string CardName { get; set; }= null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal DebitFC { get; set; }
    public decimal CreditFC { get; set; }
    public string? LineMemo { get; set; }
    public string? CostCenter { get; set; }
    public string? Project { get; set; }
    public string? TaxCode { get; set; }
    public decimal TaxAmount { get; set; }
    public string? Reference { get; set; }
    public DateTime? DueDate { get; set; }

    // Navigation
    public JournalVoucher JournalVoucher { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class JournalVoucherConfig : IEntityTypeConfiguration<JournalVoucher>
{
    public void Configure(EntityTypeBuilder<JournalVoucher> builder)
    {
        builder.ToTable("KJVH");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.VoucherNo).HasMaxLength(50).IsRequired();
        builder.Property(m => m.RefNo).HasMaxLength(50);
        builder.Property(m => m.Description).HasMaxLength(200);
        builder.Property(m => m.Memo).HasMaxLength(500);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("D");
        builder.Property(m => m.ApprovalStatus).HasMaxLength(1).HasDefaultValue("N");
        builder.Property(m => m.ApprovalNote).HasMaxLength(500);
        builder.Property(m => m.ProjectCode).HasMaxLength(50);
        builder.Property(m => m.CostCenter).HasMaxLength(50);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.Property(m => m.TotalDebit).HasPrecision(18, 2);
        builder.Property(m => m.TotalCredit).HasPrecision(18, 2);
        builder.Property(m => m.ExchangeRate).HasPrecision(18, 6);

        builder.HasIndex(m => m.VoucherNo).IsUnique();
        builder.HasIndex(m => m.VoucherDate);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => m.ApprovalStatus);
    }
}

public class JournalVoucherLineConfig : IEntityTypeConfiguration<JournalVoucherLine>
{
    public void Configure(EntityTypeBuilder<JournalVoucherLine> builder)
    {
        builder.ToTable("JVH1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.AccountCode).HasMaxLength(50).IsRequired();
        builder.Property(m => m.AccountName).HasMaxLength(200);
        builder.Property(m => m.CardCode).HasMaxLength(50);
        builder.Property(m => m.CardName).HasMaxLength(200);
        builder.Property(m => m.LineMemo).HasMaxLength(500);
        builder.Property(m => m.CostCenter).HasMaxLength(50);
        builder.Property(m => m.Project).HasMaxLength(50);
        builder.Property(m => m.TaxCode).HasMaxLength(20);
        builder.Property(m => m.Reference).HasMaxLength(50);

        builder.Property(m => m.Debit).HasPrecision(18, 2);
        builder.Property(m => m.Credit).HasPrecision(18, 2);
        builder.Property(m => m.DebitFC).HasPrecision(18, 2);
        builder.Property(m => m.CreditFC).HasPrecision(18, 2);
        builder.Property(m => m.TaxAmount).HasPrecision(18, 2);

        builder.HasOne(m => m.JournalVoucher)
            .WithMany(j => j.Lines)
            .HasForeignKey(m => m.JournalVoucherId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.JournalVoucherId);
        builder.HasIndex(m => m.AccountCode);
    }
}