using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.ExchangeRateDifferences;

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE DIFFERENCES — HEADER (KERD)
// SAP B1 equivalent: Financials > Exchange Rate Differences
//
// A period-end revaluation run. Foreign-currency balances were
// posted at the rate of their transaction date; at period end the
// closing rate differs, so the local-currency value of those
// balances has moved. Each run records the closing rate used and
// one line per revalued balance, then posts the net gain/loss.
//
// A run is Open while it is being reviewed and Posted once the
// journal has been raised. Posted runs are immutable — correcting
// one means cancelling it and raising another.
// ═══════════════════════════════════════════════════════════════
public class ExchangeRateDifference : AuditableEntity
{
    public string?  DocNo        { get; set; }            // ERD-2026-00001 (auto)
    public string   Status       { get; set; } = "Open";  // Open / Posted / Cancelled

    public DateTime PostingDate  { get; set; }
    public DateTime DocumentDate { get; set; }

    /// <summary>Foreign currency being revalued, e.g. "USD".</summary>
    public string   Currency     { get; set; } = null!;
    /// <summary>Company local currency the balances are expressed in.</summary>
    public string   BaseCurrency { get; set; } = "KHR";
    /// <summary>Closing rate applied to every line in this run.</summary>
    public decimal  ClosingRate  { get; set; }

    // Optional G/L account selection range, mirroring the SAP screen.
    public string?  AccountFrom  { get; set; }
    public string?  AccountTo    { get; set; }

    /// <summary>Sum of positive line differences.</summary>
    public decimal  TotalGain      { get; set; }
    /// <summary>Sum of negative line differences, stored positive.</summary>
    public decimal  TotalLoss      { get; set; }
    /// <summary>TotalGain - TotalLoss.</summary>
    public decimal  NetDifference  { get; set; }

    public string?  Remarks      { get; set; }

    public ICollection<ExchangeRateDifferenceLine> Lines { get; set; }
        = new List<ExchangeRateDifferenceLine>();
}

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE DIFFERENCES — LINE (ERD1)
// One revalued balance: what it was worth at the historical rate
// versus the closing rate.
// ═══════════════════════════════════════════════════════════════
public class ExchangeRateDifferenceLine
{
    public int Id                        { get; set; }
    public int ExchangeRateDifferenceId  { get; set; }
    public int LineNum                   { get; set; }

    public ExchangeRateDifference? ExchangeRateDifference { get; set; }

    /// <summary>"GL" for a ledger account, "BP" for a business partner balance.</summary>
    public string  SourceType  { get; set; } = "GL";
    public string  AccountCode { get; set; } = null!;
    public string? AccountName { get; set; }

    /// <summary>Outstanding balance in the foreign currency.</summary>
    public decimal ForeignBalance     { get; set; }
    /// <summary>Rate the balance was originally posted at.</summary>
    public decimal HistoricalRate     { get; set; }
    /// <summary>Closing rate from the run header.</summary>
    public decimal CurrentRate        { get; set; }

    public decimal LocalBalanceBefore { get; set; }
    public decimal LocalBalanceAfter  { get; set; }
    /// <summary>LocalBalanceAfter - LocalBalanceBefore. Positive is a gain.</summary>
    public decimal Difference         { get; set; }

    public string? Remarks { get; set; }
}

// ─── EF configuration ────────────────────────────────────────
public class ExchangeRateDifferenceConfig : IEntityTypeConfiguration<ExchangeRateDifference>
{
    public void Configure(EntityTypeBuilder<ExchangeRateDifference> b)
    {
        b.ToTable("KERD");
        b.HasKey(x => x.Id);

        b.Property(x => x.DocNo).HasMaxLength(50);
        b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Open");
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.BaseCurrency).HasMaxLength(10);
        b.Property(x => x.AccountFrom).HasMaxLength(50);
        b.Property(x => x.AccountTo).HasMaxLength(50);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.ClosingRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.TotalGain).HasColumnType("decimal(18,2)");
        b.Property(x => x.TotalLoss).HasColumnType("decimal(18,2)");
        b.Property(x => x.NetDifference).HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.DocNo).IsUnique();
        b.HasIndex(x => new { x.Currency, x.PostingDate });

        b.HasMany(x => x.Lines)
            .WithOne(l => l.ExchangeRateDifference)
            .HasForeignKey(l => l.ExchangeRateDifferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExchangeRateDifferenceLineConfig : IEntityTypeConfiguration<ExchangeRateDifferenceLine>
{
    public void Configure(EntityTypeBuilder<ExchangeRateDifferenceLine> b)
    {
        b.ToTable("ERD1");
        b.HasKey(x => x.Id);

        b.Property(x => x.SourceType).HasMaxLength(10).HasDefaultValue("GL");
        b.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountName).HasMaxLength(200);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.Property(x => x.ForeignBalance).HasColumnType("decimal(18,2)");
        b.Property(x => x.HistoricalRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.CurrentRate).HasColumnType("decimal(18,6)");
        b.Property(x => x.LocalBalanceBefore).HasColumnType("decimal(18,2)");
        b.Property(x => x.LocalBalanceAfter).HasColumnType("decimal(18,2)");
        b.Property(x => x.Difference).HasColumnType("decimal(18,2)");

        b.HasIndex(x => x.ExchangeRateDifferenceId);
    }
}
