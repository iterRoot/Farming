using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccounts : AuditableEntity
{
    public string AcctCode { get; set; } = null!;
    public string AcctName { get; set; } = null!;
    public string? ExternalCode { get; set; }
    public int Level { get; set; }
    public string? FatherNum { get; set; }
    public string AcctType { get; set; } = "A"; // A=Assets, L=Liabilities, E=Equity, I=Income, X=Expense
    public bool IsGroup { get; set; }
    public bool IsActive { get; set; } = true;
    public string Currency { get; set; } = "USD";
    
    public decimal Balance { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }

    public bool AllowPosting { get; set; } = true;
    public bool BlockManualPosting { get; set; }
    public bool CashFlowRelevant { get; set; }
    public bool Confidential { get; set; }
    public bool ControlAccount { get; set; }
    public bool CashAccount { get; set; }
    public bool Indexed { get; set; }
    public bool RevalCurrency { get; set; }

    public string? Description { get; set; }
    public string? TaxCode { get; set; }
    public string? CostCenter { get; set; }
    public string? Department { get; set; }
    public string? Project { get; set; }

    // System parameters from your script
    public int? UserSign { get; set; }
    public int VersionNum { get; set; } = 1;
    public Guid GuidId { get; set; } = Guid.NewGuid();

    // Navigation
    public ChartOfAccounts? ParentAccount { get; set; }
    public ICollection<ChartOfAccounts> ChildAccounts { get; set; } = new List<ChartOfAccounts>();
}

public class ChartOfAccountsConfig : IEntityTypeConfiguration<ChartOfAccounts>
{
    public void Configure(EntityTypeBuilder<ChartOfAccounts> builder)
    {
        builder.ToTable("KCOA");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.AcctCode).HasMaxLength(20).IsRequired();
        builder.HasIndex(m => m.AcctCode).IsUnique();
        builder.Property(m => m.AcctName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ExternalCode).HasMaxLength(50);
        builder.Property(m => m.FatherNum).HasMaxLength(20);
        builder.Property(m => m.AcctType).HasMaxLength(1).IsRequired();

        builder.Property(m => m.Balance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.DebitBalance).HasPrecision(18, 2).HasDefaultValue(0);
        builder.Property(m => m.CreditBalance).HasPrecision(18, 2).HasDefaultValue(0);

        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.TaxCode).HasMaxLength(20);
        builder.Property(m => m.CostCenter).HasMaxLength(50);
        builder.Property(m => m.Department).HasMaxLength(50);
        builder.Property(m => m.Project).HasMaxLength(50);

        builder.HasOne(m => m.ParentAccount)
            .WithMany(p => p.ChildAccounts)
            .HasForeignKey(m => m.FatherNum)
            .HasPrincipalKey(p => p.AcctCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}