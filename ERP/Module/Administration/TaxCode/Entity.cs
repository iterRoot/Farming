using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.TaxCode;

// SAP B1 "Tax Codes - Setup" — a VAT/tax code with its rate, category and the
// G/L accounts postings are directed to. `InActive` (from AuditableEntity) is
// the grid's "Inactive" flag.
public class TaxCodeEntry : AuditableEntity
{
    public string   Code                 { get; set; } = null!;
    public string?  Name                 { get; set; }
    public string   Category             { get; set; } = "Output Tax"; // Output Tax | Input Tax
    public bool     AcquisitionReverse   { get; set; }
    public DateOnly? EffectiveFrom       { get; set; }
    public decimal  Rate                 { get; set; }
    public decimal  NonDeductiblePercent { get; set; }
    public string?  TaxAccount           { get; set; }
    public string?  AcquisitionTaxAccount { get; set; }
    public string?  DeferredTaxAccount   { get; set; }
    public string?  NonDeductibleAccount { get; set; }
    public string?  CodeDescription      { get; set; }
}

public class TaxCodeEntryConfig : IEntityTypeConfiguration<TaxCodeEntry>
{
    public void Configure(EntityTypeBuilder<TaxCodeEntry> b)
    {
        b.ToTable("KTAX");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.Property(x => x.Category).HasMaxLength(20).IsRequired();
        b.Property(x => x.Rate).HasPrecision(9, 4);
        b.Property(x => x.NonDeductiblePercent).HasPrecision(9, 4);
        b.HasIndex(x => x.Code).IsUnique();
    }
}
