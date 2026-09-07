using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.TaxCodeDetermination;

// SAP B1 "Tax Code Determination - Setup": a rule that resolves which tax code
// to default onto a document line, based on document type / business area and
// up to three key=value conditions. Rules are evaluated in Priority order.
public class TaxDeterminationRule : AuditableEntity
{
    public string   DocumentType { get; set; } = "All";
    public string?  BusinessArea { get; set; }
    public string?  Condition1   { get; set; }
    public string?  Value1       { get; set; }
    public string?  Condition2   { get; set; }
    public string?  Value2       { get; set; }
    public string?  Condition3   { get; set; }
    public string?  Value3       { get; set; }
    public string?  Description  { get; set; }
    public string   LineTaxCode  { get; set; } = null!;   // the tax code to apply
    public int      Priority     { get; set; }
}

public class TaxDeterminationRuleConfig : IEntityTypeConfiguration<TaxDeterminationRule>
{
    public void Configure(EntityTypeBuilder<TaxDeterminationRule> b)
    {
        b.ToTable("KTXD");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocumentType).HasMaxLength(30).IsRequired();
        b.Property(x => x.LineTaxCode).HasMaxLength(20).IsRequired();
    }
}
