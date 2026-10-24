using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

// ═══════════════════════════════════════════════════════════════════
// CHART OF ACCOUNTS — SAP B1 OACT structure
// Level starts at 1 (root), matches SAP B1 exactly
// AcctType: A/L/E/I/C/X/N/T — 8 types matching SAP right-panel tabs
// ═══════════════════════════════════════════════════════════════════
public class ChartOfAccounts : AuditableEntity
{
    // ── Core ─────────────────────────────────────────────────────
    public string AcctCode    { get; set; } = null!;   // GL Account Code
    public string AcctName    { get; set; } = null!;   // Account Name
    public string? FrgnName   { get; set; }            // Foreign language name
    public string? ExternalCode { get; set; }          // External / Export code

    // ── Hierarchy ────────────────────────────────────────────────
    // Level 1 = Root (no parent), Level 2+ = children
    public int     Level     { get; set; }
    public string? FatherNum { get; set; }             // Parent AcctCode

    // ── Classification ───────────────────────────────────────────
    // A=Assets  L=Liabilities  E=Capital&Reserves
    // I=Turnover  C=CostOfSales  X=OperatingCosts
    // N=NonOperating  T=TaxationExtraordinary
    public string AcctType { get; set; } = "A";
    public bool   IsGroup  { get; set; }               // true = Header/Title
    public bool   IsActive { get; set; } = true;

    // ── Currency ─────────────────────────────────────────────────
    public string Currency { get; set; } = "USD";

    // ── Balances ─────────────────────────────────────────────────
    public decimal Balance       { get; set; }
    public decimal DebitBalance  { get; set; }
    public decimal CreditBalance { get; set; }

    // ── Posting flags ────────────────────────────────────────────
    public bool AllowPosting       { get; set; } = true;
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant   { get; set; }
    public bool CashBox            { get; set; }

    // ── G/L Properties ───────────────────────────────────────────
    public bool Confidential   { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount    { get; set; }
    public bool Indexed        { get; set; }
    public bool RevalCurrency  { get; set; }

    // ── Tax ──────────────────────────────────────────────────────
    public bool    TaxIncome { get; set; }
    public bool    ExmIncome { get; set; }     // Exempt Income
    public string? DfltVat   { get; set; }     // Default VAT group
    public string? TaxCode   { get; set; }

    // ── Freeze ───────────────────────────────────────────────────
    public bool      Frozen     { get; set; }
    public DateTime? FrozenFrom { get; set; }
    public DateTime? FrozenTo   { get; set; }

    // ── Validity ─────────────────────────────────────────────────
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo   { get; set; }

    // ── Dimensions ───────────────────────────────────────────────
    public string? Project    { get; set; }
    public bool    PrjRelvnt  { get; set; }
    public bool    Dim1Relvnt { get; set; }
    public bool    Dim2Relvnt { get; set; }
    public bool    Dim3Relvnt { get; set; }
    public bool    Dim4Relvnt { get; set; }
    public bool    Dim5Relvnt { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }

    // ── Description ──────────────────────────────────────────────
    public string? Description { get; set; }

    // ── Closing ──────────────────────────────────────────────────
    public string? ClosingAcc { get; set; }
    public string? TransCode  { get; set; }

    // ── System ───────────────────────────────────────────────────
    public string? DataSource { get; set; } = "O";  // O = manually created
    public int?    UserSign   { get; set; }
    public int     VersionNum { get; set; } = 1;

    // ── Navigation ───────────────────────────────────────────────
    public ChartOfAccounts?              ParentAccount { get; set; }
    public ICollection<ChartOfAccounts>  ChildAccounts { get; set; } = new List<ChartOfAccounts>();
}

// ═══════════════════════════════════════════════════════════════════
// EF CONFIGURATION
// ═══════════════════════════════════════════════════════════════════
public class ChartOfAccountsConfig : IEntityTypeConfiguration<ChartOfAccounts>
{
    public void Configure(EntityTypeBuilder<ChartOfAccounts> builder)
    {
        builder.ToTable("KCOA");
        builder.HasKey(x => x.Id);

        // Core
        builder.Property(m => m.AcctCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(m => m.AcctCode).IsUnique();
        builder.Property(m => m.AcctName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.FrgnName).HasMaxLength(200);
        builder.Property(m => m.ExternalCode).HasMaxLength(50);

        // Hierarchy
        builder.Property(m => m.Level).IsRequired();
        builder.Property(m => m.FatherNum).HasMaxLength(20);

        // Classification — 1 char, A/L/E/I/C/X/N/T
        builder.Property(m => m.AcctType).HasMaxLength(1).IsRequired();
        builder.Property(m => m.IsGroup).HasDefaultValue(false);
        builder.Property(m => m.IsActive).HasDefaultValue(true);

        // Currency
        builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");

        // Balances
        builder.Property(m => m.Balance).HasPrecision(18, 6).HasDefaultValue(0);
        builder.Property(m => m.DebitBalance).HasPrecision(18, 6).HasDefaultValue(0);
        builder.Property(m => m.CreditBalance).HasPrecision(18, 6).HasDefaultValue(0);

        // Posting
        builder.Property(m => m.AllowPosting).HasDefaultValue(true);
        builder.Property(m => m.BlockManualPosting).HasDefaultValue(false);
        builder.Property(m => m.CashFlowRelevant).HasDefaultValue(false);

        // Tax
        builder.Property(m => m.DfltVat).HasMaxLength(20);
        builder.Property(m => m.TaxCode).HasMaxLength(20);

        // Dimensions
        builder.Property(m => m.Project).HasMaxLength(50);
        builder.Property(m => m.CostCenter).HasMaxLength(50);
        builder.Property(m => m.Department).HasMaxLength(50);

        // Description
        builder.Property(m => m.Description).HasMaxLength(500);

        // Closing
        builder.Property(m => m.ClosingAcc).HasMaxLength(20);
        builder.Property(m => m.TransCode).HasMaxLength(20);

        // System
        builder.Property(m => m.DataSource).HasMaxLength(1).HasDefaultValue("O");
        builder.Property(m => m.VersionNum).HasDefaultValue(1);

        // Self-referencing FK
        builder.HasOne(m => m.ParentAccount)
            .WithMany(p => p.ChildAccounts)
            .HasForeignKey(m => m.FatherNum)
            .HasPrincipalKey(p => p.AcctCode)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(m => m.FatherNum);
        builder.HasIndex(m => m.AcctType);
        builder.HasIndex(m => m.Level);
        builder.HasIndex(m => m.IsActive);
    }
}