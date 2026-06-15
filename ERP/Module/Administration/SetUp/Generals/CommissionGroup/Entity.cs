using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Sales.CommissionGroup;

// ═══════════════════════════════════════════════════════════════
// COMMISSION GROUP  (KCMG)
// SAP B1 equivalent: Commission Groups
// Groups salespersons and defines commission rules applied
// when they close sales transactions.
//
// Commission Types:
//   Percentage → commission = SaleAmount × Rate / 100
//   Fixed      → flat amount per transaction
//   Tiered     → rate changes based on amount brackets
// ═══════════════════════════════════════════════════════════════
public class CommissionGroup : AuditableEntity
{
    public string   Code             { get; set; } = null!;  // e.g. "CG-SENIOR"
    public string   Name             { get; set; } = null!;  // e.g. "Senior Sales Team"
    public string   CommissionType   { get; set; } = "Percentage"; // Percentage / Fixed / Tiered
    public decimal  CommissionRate   { get; set; }           // % or fixed amount
    public decimal? MinSaleAmount    { get; set; }           // min sale to qualify
    public decimal? MaxCommission    { get; set; }           // cap on commission earned
    public string?  Currency         { get; set; } = "KHR";
    public string?  Description      { get; set; }
    public bool     IsActive         { get; set; } = true;
    public string?  Remarks          { get; set; }

    // Tiered brackets (stored as child rows)
    public ICollection<CommissionTier> Tiers { get; set; } = new List<CommissionTier>();
}

// ── Tiered commission brackets ────────────────────────────────
// e.g. 0–1,000 USD → 3%, 1,001–5,000 USD → 5%, >5,000 → 7%
public class CommissionTier
{
    public int     Id                { get; set; }
    public int     CommissionGroupId { get; set; }
    public int     TierOrder         { get; set; }
    public decimal FromAmount        { get; set; }
    public decimal ToAmount          { get; set; }  // 0 = unlimited
    public decimal Rate              { get; set; }  // %

    public CommissionGroup? CommissionGroup { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class CommissionGroupConfig : IEntityTypeConfiguration<CommissionGroup>
{
    public void Configure(EntityTypeBuilder<CommissionGroup> b)
    {
        b.ToTable("KCMG");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.CommissionType).HasMaxLength(20);
        b.Property(x => x.CommissionRate).HasColumnType("decimal(18,4)");
        b.Property(x => x.MinSaleAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.MaxCommission).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();

        b.HasMany(x => x.Tiers)
         .WithOne(x => x.CommissionGroup)
         .HasForeignKey(x => x.CommissionGroupId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CommissionTierConfig : IEntityTypeConfiguration<CommissionTier>
{
    public void Configure(EntityTypeBuilder<CommissionTier> b)
    {
        b.ToTable("KCM1");
        b.HasKey(x => x.Id);
        b.Property(x => x.FromAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.ToAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Rate).HasColumnType("decimal(18,4)");
    }
}