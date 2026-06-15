// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.Financials.CostAccounting;

// // ═══════════════════════════════════════════════════════════════════
// // COST CENTER (KPRC)
// // ═══════════════════════════════════════════════════════════════════
// public class CostCenter : AuditableEntity
// {
//     public string    CostCenterCode { get; set; } = null!;  // CC-001
//     public string    CostCenterName { get; set; } = null!;  // Farm Division
//     public string?   Description    { get; set; }
//     public string    CenterType     { get; set; } = "D";    // D=Department R=Region P=Project
//     public string?   ParentCode     { get; set; }           // self-ref
//     public string?   Manager        { get; set; }
//     public string    Currency       { get; set; } = "USD";
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public bool      IsActive       { get; set; } = true;
//     public int       VersionNum     { get; set; } = 1;
//     public decimal   TotalDebit     { get; set; }
//     public decimal   TotalCredit    { get; set; }
//     public decimal   Balance        { get; set; }

//     public CostCenter?             ParentCostCenter { get; set; }
//     public ICollection<CostCenter> ChildCostCenters { get; set; } = new List<CostCenter>();
// }

// // ═══════════════════════════════════════════════════════════════════
// // DISTRIBUTION RULE HEADER (KDRL)
// // ═══════════════════════════════════════════════════════════════════
// public class DistributionRule : AuditableEntity
// {
//     public string  RuleCode    { get; set; } = null!;  // DR-001
//     public string  RuleName    { get; set; } = null!;
//     public string? Description { get; set; }
//     public bool    IsActive    { get; set; } = true;
//     public bool    InUse       { get; set; } = false;
//     public int     VersionNum  { get; set; } = 1;

//     public ICollection<DistributionRuleLine> Lines { get; set; } = new List<DistributionRuleLine>();
// }

// // ═══════════════════════════════════════════════════════════════════
// // DISTRIBUTION RULE LINE (DRL1) — total must = 100%
// // ═══════════════════════════════════════════════════════════════════
// public class DistributionRuleLine : AuditableEntity
// {
//     public int     DistributionRuleId { get; set; }
//     public int     LineNum            { get; set; }
//     public string  CostCenterCode     { get; set; } = null!;
//     public string  CostCenterName     { get; set; } = string.Empty;
//     public decimal Percentage         { get; set; }
//     public string? Remarks            { get; set; }

//     public DistributionRule DistributionRule { get; set; } = null!;
// }

// // ═══════════════════════════════════════════════════════════════════
// // PROJECT (KPRJ)
// // ═══════════════════════════════════════════════════════════════════
// public class Project : AuditableEntity
// {
//     public string    ProjectCode    { get; set; } = null!;  // PROJ-001
//     public string    ProjectName    { get; set; } = null!;
//     public string?   Description    { get; set; }
//     public string    Status         { get; set; } = "P";   // P=Planning A=Active C=Completed X=Cancelled
//     public DateTime? StartDate      { get; set; }
//     public DateTime? EndDate        { get; set; }
//     public decimal   BudgetAmount   { get; set; }
//     public string?   Manager        { get; set; }
//     public string?   CustomerCode   { get; set; }
//     public string?   CustomerName   { get; set; }
//     public string?   CostCenterCode { get; set; }
//     public string    Currency       { get; set; } = "USD";
//     public bool      IsActive       { get; set; } = true;
//     public int       VersionNum     { get; set; } = 1;
//     public decimal   TotalDebit     { get; set; }
//     public decimal   TotalCredit    { get; set; }
//     public decimal   ActualCost     { get; set; }
//     public decimal   ActualRevenue  { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // EF CONFIGURATIONS
// // ═══════════════════════════════════════════════════════════════════
// public class CostCenterConfig : IEntityTypeConfiguration<CostCenter>
// {
//     public void Configure(EntityTypeBuilder<CostCenter> builder)
//     {
//         builder.ToTable("KPRC");
//         builder.HasKey(x => x.Id);
//         builder.Property(m => m.CostCenterCode).HasMaxLength(20).IsRequired();
//         builder.HasIndex(m => m.CostCenterCode).IsUnique();
//         builder.Property(m => m.CostCenterName).HasMaxLength(200).IsRequired();
//         builder.Property(m => m.Description).HasMaxLength(500);
//         builder.Property(m => m.CenterType).HasMaxLength(1).HasDefaultValue("D");
//         builder.Property(m => m.ParentCode).HasMaxLength(20);
//         builder.Property(m => m.Manager).HasMaxLength(100);
//         builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
//         builder.Property(m => m.VersionNum).HasDefaultValue(1);
//         builder.Property(m => m.TotalDebit).HasPrecision(18, 2);
//         builder.Property(m => m.TotalCredit).HasPrecision(18, 2);
//         builder.Property(m => m.Balance).HasPrecision(18, 2);
//         builder.HasOne(m => m.ParentCostCenter)
//             .WithMany(p => p.ChildCostCenters)
//             .HasForeignKey(m => m.ParentCode)
//             .HasPrincipalKey(p => p.CostCenterCode)
//             .OnDelete(DeleteBehavior.Restrict);
//         builder.HasIndex(m => m.ParentCode);
//         builder.HasIndex(m => m.IsActive);
//     }
// }

// public class DistributionRuleConfig : IEntityTypeConfiguration<DistributionRule>
// {
//     public void Configure(EntityTypeBuilder<DistributionRule> builder)
//     {
//         builder.ToTable("KDRL");
//         builder.HasKey(x => x.Id);
//         builder.Property(m => m.RuleCode).HasMaxLength(20).IsRequired();
//         builder.HasIndex(m => m.RuleCode).IsUnique();
//         builder.Property(m => m.RuleName).HasMaxLength(200).IsRequired();
//         builder.Property(m => m.Description).HasMaxLength(500);
//         builder.Property(m => m.VersionNum).HasDefaultValue(1);
//     }
// }

// public class DistributionRuleLineConfig : IEntityTypeConfiguration<DistributionRuleLine>
// {
//     public void Configure(EntityTypeBuilder<DistributionRuleLine> builder)
//     {
//         builder.ToTable("DRL1");
//         builder.HasKey(x => x.Id);
//         builder.Property(m => m.CostCenterCode).HasMaxLength(20).IsRequired();
//         builder.Property(m => m.CostCenterName).HasMaxLength(200).HasDefaultValue(string.Empty);
//         builder.Property(m => m.Percentage).HasPrecision(8, 2);
//         builder.Property(m => m.Remarks).HasMaxLength(200);
//         builder.HasOne(m => m.DistributionRule)
//             .WithMany(d => d.Lines)
//             .HasForeignKey(m => m.DistributionRuleId)
//             .OnDelete(DeleteBehavior.Cascade);
//         builder.HasIndex(m => m.DistributionRuleId);
//     }
// }

// public class ProjectConfig : IEntityTypeConfiguration<Project>
// {
//     public void Configure(EntityTypeBuilder<Project> builder)
//     {
//         builder.ToTable("KPRJ");
//         builder.HasKey(x => x.Id);
//         builder.Property(m => m.ProjectCode).HasMaxLength(20).IsRequired();
//         builder.HasIndex(m => m.ProjectCode).IsUnique();
//         builder.Property(m => m.ProjectName).HasMaxLength(200).IsRequired();
//         builder.Property(m => m.Description).HasMaxLength(500);
//         builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("P");
//         builder.Property(m => m.Manager).HasMaxLength(100);
//         builder.Property(m => m.CustomerCode).HasMaxLength(50);
//         builder.Property(m => m.CustomerName).HasMaxLength(200);
//         builder.Property(m => m.CostCenterCode).HasMaxLength(20);
//         builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
//         builder.Property(m => m.BudgetAmount).HasPrecision(18, 2);
//         builder.Property(m => m.TotalDebit).HasPrecision(18, 2);
//         builder.Property(m => m.TotalCredit).HasPrecision(18, 2);
//         builder.Property(m => m.ActualCost).HasPrecision(18, 2);
//         builder.Property(m => m.ActualRevenue).HasPrecision(18, 2);
//         builder.Property(m => m.VersionNum).HasDefaultValue(1);
//         builder.HasIndex(m => m.Status);
//         builder.HasIndex(m => m.CostCenterCode);
//     }
// }