using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.CostAccounting;

// ═══════════════════════════════════════════════════════════════════
// COST ACCOUNTING — Cost Centers and Distribution Rules.
//
// Projects were part of the original design for this module but they now
// live in Financials/FinancialProjects (table KPRJ). This module owns
// KPRC (cost centres), KDRL (distribution rule headers) and DRL1 (lines).
// ═══════════════════════════════════════════════════════════════════

// ── COST CENTRE (KPRC) ─────────────────────────────────────────────
public class CostCenter : AuditableEntity
{
    public string  CostCenterCode { get; set; } = null!;   // CC-001
    public string  CostCenterName { get; set; } = null!;   // Farm Division
    public string? Description    { get; set; }
    public string  CenterType     { get; set; } = "D";     // D=Department R=Region P=Project
    public string? ParentCode     { get; set; }            // self-reference on CostCenterCode
    public string? Manager        { get; set; }
    public string  Currency       { get; set; } = "USD";
    public DateTime? StartDate    { get; set; }
    public DateTime? EndDate      { get; set; }
    public bool    IsActive       { get; set; } = true;
    public int     VersionNum     { get; set; } = 1;

    // Roll-ups, maintained by whatever posts to the centre (nothing yet).
    public decimal TotalDebit  { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance     { get; set; }

    public CostCenter?              ParentCostCenter { get; set; }
    public ICollection<CostCenter>  ChildCostCenters { get; set; } = new List<CostCenter>();
}

// ── DISTRIBUTION RULE HEADER (KDRL) ────────────────────────────────
public class DistributionRule : AuditableEntity
{
    public string  RuleCode    { get; set; } = null!;   // DR-001
    public string  RuleName    { get; set; } = null!;
    public string? Description  { get; set; }
    public bool    IsActive     { get; set; } = true;
    public bool    InUse        { get; set; }            // set once a posting references it
    public int     VersionNum   { get; set; } = 1;

    public ICollection<DistributionRuleLine> Lines { get; set; } = new List<DistributionRuleLine>();
}

// ── DISTRIBUTION RULE LINE (DRL1) — the line percentages must total 100 ──
public class DistributionRuleLine : AuditableEntity
{
    public int     DistributionRuleId { get; set; }
    public int     LineNum            { get; set; }
    public string  CostCenterCode     { get; set; } = null!;
    public string  CostCenterName     { get; set; } = string.Empty;
    public decimal Percentage         { get; set; }
    public string? Remarks            { get; set; }

    public DistributionRule DistributionRule { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATION
// ═══════════════════════════════════════════════════════════════════
public class CostCenterConfig : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("KPRC");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.CostCenterCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(m => m.CostCenterCode).IsUnique();
        builder.Property(m => m.CostCenterName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.CenterType).HasMaxLength(1).HasDefaultValue("D");
        builder.Property(m => m.ParentCode).HasMaxLength(20);
        builder.Property(m => m.Manager).HasMaxLength(100);
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.VersionNum).HasDefaultValue(1);
        builder.Property(m => m.TotalDebit).HasPrecision(18, 2);
        builder.Property(m => m.TotalCredit).HasPrecision(18, 2);
        builder.Property(m => m.Balance).HasPrecision(18, 2);

        builder.HasOne(m => m.ParentCostCenter)
            .WithMany(p => p.ChildCostCenters)
            .HasForeignKey(m => m.ParentCode)
            .HasPrincipalKey(p => p.CostCenterCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => m.ParentCode);
        builder.HasIndex(m => m.IsActive);
    }
}

public class DistributionRuleConfig : IEntityTypeConfiguration<DistributionRule>
{
    public void Configure(EntityTypeBuilder<DistributionRule> builder)
    {
        builder.ToTable("KDRL");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.RuleCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(m => m.RuleCode).IsUnique();
        builder.Property(m => m.RuleName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);
    }
}

public class DistributionRuleLineConfig : IEntityTypeConfiguration<DistributionRuleLine>
{
    public void Configure(EntityTypeBuilder<DistributionRuleLine> builder)
    {
        builder.ToTable("DRL1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.CostCenterCode).HasMaxLength(20).IsRequired();
        builder.Property(m => m.CostCenterName).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.Percentage).HasPrecision(8, 2);
        builder.Property(m => m.Remarks).HasMaxLength(200);

        builder.HasOne(m => m.DistributionRule)
            .WithMany(d => d.Lines)
            .HasForeignKey(m => m.DistributionRuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.DistributionRuleId);
    }
}
