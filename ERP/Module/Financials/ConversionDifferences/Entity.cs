using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.ConversionDifferences;

// ═══════════════════════════════════════════════════════════════
// CONVERSION DIFFERENCES — HEADER (KCDF)
// SAP B1 equivalent: Financials > Conversion Differences
//
// The sibling of Exchange Rate Differences, one currency layer up.
// Exchange rate differences restate FOREIGN balances into the local
// currency; conversion differences restate LOCAL-currency balances
// into the company's second reporting ("system") currency, whose
// rate has also moved since the entries were posted.
//
// Each run fixes one conversion rate and applies it to every line,
// so a run's arithmetic is reproducible from its header. A run is
// Open while under review and Posted once its journal is raised;
// posted runs are immutable and are corrected by raising another.
// ═══════════════════════════════════════════════════════════════
public class ConversionDifference : AuditableEntity
{
    public string?  DocNo        { get; set; }            // CDF-2026-00001 (auto)
    public string   Status       { get; set; } = "Open";  // Open / Posted / Cancelled

    public DateTime PostingDate  { get; set; }
    public DateTime DocumentDate { get; set; }

    /// <summary>Local currency the balances are held in, e.g. "KHR".</summary>
    public string   LocalCurrency  { get; set; } = "KHR";
    /// <summary>Second reporting currency being converted to, e.g. "USD".</summary>
    public string   SystemCurrency { get; set; } = null!;
    /// <summary>Conversion rate applied to every line in this run.</summary>
    public decimal  ConversionRate { get; set; }

    // Optional G/L account selection range, mirroring the SAP screen.
    public string?  AccountFrom  { get; set; }
    public string?  AccountTo    { get; set; }

    /// <summary>Sum of positive line differences.</summary>
    public decimal  TotalGain     { get; set; }
    /// <summary>Sum of negative line differences, stored positive.</summary>
    public decimal  TotalLoss     { get; set; }
    /// <summary>TotalGain - TotalLoss.</summary>
    public decimal  NetDifference { get; set; }

    public string?  Remarks      { get; set; }

    public ICollection<ConversionDifferenceLine> Lines { get; set; }
        = new List<ConversionDifferenceLine>();
}

// ═══════════════════════════════════════════════════════════════
// CONVERSION DIFFERENCES — LINE (CDF1)
// One restated balance: its system-currency value at the rate it was
// booked at, versus its value at this run's conversion rate.
// ═══════════════════════════════════════════════════════════════
public class ConversionDifferenceLine
{
    public int Id                     { get; set; }
    public int ConversionDifferenceId { get; set; }
    public int LineNum                { get; set; }

    public ConversionDifference? ConversionDifference { get; set; }

    /// <summary>"GL" for a ledger account, "BP" for a business partner balance.</summary>
    public string  SourceType  { get; set; } = "GL";
    public string  AccountCode { get; set; } = null!;
    public string? AccountName { get; set; }

    /// <summary>Outstanding balance in the local currency.</summary>
    public decimal LocalBalance         { get; set; }
    /// <summary>Rate the balance was originally converted at.</summary>
    public decimal HistoricalRate       { get; set; }
    /// <summary>Conversion rate from the run header.</summary>
    public decimal CurrentRate          { get; set; }

    public decimal SystemBalanceBefore  { get; set; }
    public decimal SystemBalanceAfter   { get; set; }
    /// <summary>SystemBalanceAfter - SystemBalanceBefore. Positive is a gain.</summary>
    public decimal Difference           { get; set; }

    public string? Remarks { get; set; }
}

// ─── EF configuration ────────────────────────────────────────
public class ConversionDifferenceConfig : IEntityTypeConfiguration<ConversionDifference>
{
    public void Configure(EntityTypeBuilder<ConversionDifference> b)
    {
        b.ToTable("KCDF");
        b.HasKey(x => x.Id);

        b.Property(x => x.DocNo).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
        b.Property(x => x.LocalCurrency).HasMaxLength(10);
        b.Property(x => x.SystemCurrency).HasMaxLength(10).IsRequired();
        b.Property(x => x.AccountFrom).HasMaxLength(50);
        b.Property(x => x.AccountTo).HasMaxLength(50);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.ConversionRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.TotalGain).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalLoss).HasColumnType("decimal(18,2)");
        b.Property(x => x.NetDifference).HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.DocNo).IsUnique();
        b.HasIndex(x => new { x.SystemCurrency, x.PostingDate });

        b.HasMany(x => x.Lines)
            .WithOne(l => l.ConversionDifference)
            .HasForeignKey(l => l.ConversionDifferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConversionDifferenceLineConfig : IEntityTypeConfiguration<ConversionDifferenceLine>
{
    public void Configure(EntityTypeBuilder<ConversionDifferenceLine> b)
    {
        b.ToTable("CDF1");
        b.HasKey(x => x.Id);

        b.Property(x => x.SourceType).HasMaxLength(10).HasDefaultValue("GL");
        b.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountName).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.LocalBalance).HasColumnType("decimal(18,2)");
        b.Property(x => x.HistoricalRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.CurrentRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.SystemBalanceBefore).HasColumnType("decimal(18,2)");
        b.Property(x => x.SystemBalanceAfter).HasColumnType("decimal(18,2)");
        b.Property(x => x.Difference).HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.ConversionDifferenceId);
    }
}
