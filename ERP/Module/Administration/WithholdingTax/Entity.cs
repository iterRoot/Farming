using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.WithholdingTax;

// SAP B1 "Withholding Tax Codes - Setup" — a WHT code with its category, base
// type, rounding and posting account. `InActive` (from AuditableEntity) is the
// grid's "Inactive" flag.
public class WithholdingTaxCode : AuditableEntity
{
    public string   Code          { get; set; } = null!;
    public string?  Name          { get; set; }
    public string   Category      { get; set; } = "Payment";           // Payment | Invoice
    public DateOnly? EffectiveFrom { get; set; }
    public decimal  Rate          { get; set; }
    public string   BaseType      { get; set; } = "Net";               // Net | Gross
    public string   RoundingType  { get; set; } = "Commercial Values"; // Commercial Values | Round Up | Round Down | Truncate
    public decimal  BaseAmountPercent { get; set; } = 100m;
    public string?  OfficialCode  { get; set; }
    public string?  Account       { get; set; }
}

public class WithholdingTaxCodeConfig : IEntityTypeConfiguration<WithholdingTaxCode>
{
    public void Configure(EntityTypeBuilder<WithholdingTaxCode> b)
    {
        b.ToTable("KWHT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Category).HasMaxLength(20).IsRequired();
        b.Property(x => x.BaseType).HasMaxLength(20).IsRequired();
        b.Property(x => x.RoundingType).HasMaxLength(30).IsRequired();
        b.Property(x => x.Rate).HasPrecision(9, 4);
        b.Property(x => x.BaseAmountPercent).HasPrecision(9, 4);
        b.HasIndex(x => x.Code).IsUnique();
    }
}
