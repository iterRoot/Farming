using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.Budget;

// ═══════════════════════════════════════════════════════════════════
// BUDGET HEADER (KBGT)
// ═══════════════════════════════════════════════════════════════════
public class Budget : AuditableEntity
{
    public string  BudgetNo    { get; set; } = null!;   // Auto: BG-2026-00001
    public string  BudgetName  { get; set; } = null!;   // e.g. "Annual Budget 2026"
    public string? Description { get; set; }
    public int     FiscalYear  { get; set; }             // e.g. 2026
    public DateTime StartDate  { get; set; }
    public DateTime EndDate    { get; set; }
    public string  Status      { get; set; } = "D";     // D=Draft, A=Approved, C=Closed
    public string  Currency    { get; set; } = "USD";
    public string? Remarks     { get; set; }
    public bool    IsActive    { get; set; } = true;
    public int     VersionNum  { get; set; } = 1;
    public int?    UserSign    { get; set; }

    // Navigation
    public ICollection<BudgetLine> Lines { get; set; } = new List<BudgetLine>();
}

// ═══════════════════════════════════════════════════════════════════
// BUDGET LINE (BGT1)
// ═══════════════════════════════════════════════════════════════════
public class BudgetLine : AuditableEntity
{
    public int     BudgetId    { get; set; }
    public int     LineNum     { get; set; }

    // Account
    public string  AcctCode    { get; set; } = null!;
    public string  AcctName    { get; set; } = string.Empty;
    public string  AcctType    { get; set; } = "A";     // A/L/E/I/C/X/N/T

    // Annual budget amount
    public decimal Annual      { get; set; }

    // Monthly budget breakdown (Jan–Dec)
    public decimal Jan { get; set; }
    public decimal Feb { get; set; }
    public decimal Mar { get; set; }
    public decimal Apr { get; set; }
    public decimal May { get; set; }
    public decimal Jun { get; set; }
    public decimal Jul { get; set; }
    public decimal Aug { get; set; }
    public decimal Sep { get; set; }
    public decimal Oct { get; set; }
    public decimal Nov { get; set; }
    public decimal Dec { get; set; }

    // Dimensions
    public string? CostCenter  { get; set; }
    public string? Project     { get; set; }
    public string? Remarks     { get; set; }

    // Navigation
    public Budget Budget { get; set; } = null!;
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATIONS
// ═══════════════════════════════════════════════════════════════════
public class BudgetConfig : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("KBGT");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.BudgetNo).HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.BudgetNo).IsUnique();
        builder.Property(m => m.BudgetName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("D");
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(m => m.Remarks).HasMaxLength(500);
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        builder.HasIndex(m => m.FiscalYear);
        builder.HasIndex(m => m.Status);
    }
}

public class BudgetLineConfig : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> builder)
    {
        builder.ToTable("BGT1");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.AcctCode).HasMaxLength(20).IsRequired();
        builder.Property(m => m.AcctName).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(m => m.AcctType).HasMaxLength(1).HasDefaultValue("A");
        builder.Property(m => m.CostCenter).HasMaxLength(50);
        builder.Property(m => m.Project).HasMaxLength(50);
        builder.Property(m => m.Remarks).HasMaxLength(200);

        builder.Property(m => m.Annual).HasPrecision(18, 2);
        builder.Property(m => m.Jan).HasPrecision(18, 2);
        builder.Property(m => m.Feb).HasPrecision(18, 2);
        builder.Property(m => m.Mar).HasPrecision(18, 2);
        builder.Property(m => m.Apr).HasPrecision(18, 2);
        builder.Property(m => m.May).HasPrecision(18, 2);
        builder.Property(m => m.Jun).HasPrecision(18, 2);
        builder.Property(m => m.Jul).HasPrecision(18, 2);
        builder.Property(m => m.Aug).HasPrecision(18, 2);
        builder.Property(m => m.Sep).HasPrecision(18, 2);
        builder.Property(m => m.Oct).HasPrecision(18, 2);
        builder.Property(m => m.Nov).HasPrecision(18, 2);
        builder.Property(m => m.Dec).HasPrecision(18, 2);

        builder.HasOne(m => m.Budget)
            .WithMany(b => b.Lines)
            .HasForeignKey(m => m.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.BudgetId);
        builder.HasIndex(m => m.AcctCode);
    }
}