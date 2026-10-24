using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.Currencies;

// ══════════════════════════════════════════════════════════════════
// CURRENCIES — SETUP  (SAP B1: Administration → Setup → Financials → Currencies)
// ══════════════════════════════════════════════════════════════════
public class Currencies : AuditableEntity
{
    public string   Code                 { get; set; } = null!;   // e.g. "USD"
    public string   CurrencyName         { get; set; } = null!;   // e.g. "US Dollar"
    public string?  IntlCode             { get; set; }
    public string?  IntlDescription      { get; set; }
    public string?  HundredthName        { get; set; }
    public string?  English              { get; set; }
    public string?  EnglishHundredthName { get; set; }
    public string?  IsoCurrencyCode      { get; set; }
    public decimal? IncomingAmtDiff      { get; set; }
    public decimal? OutgoingAmtDiff      { get; set; }
    public decimal? IncomingPctDiff      { get; set; }
    public decimal? OutgoingPctDiff      { get; set; }
    public string   Rounding             { get; set; } = "No Rounding";
    public string   Decimals             { get; set; } = "Default";
    public bool     RoundingInPayment    { get; set; }
}

public class CurrenciesConfig : IEntityTypeConfiguration<Currencies>
{
    public void Configure(EntityTypeBuilder<Currencies> builder)
    {
        builder.ToTable("KCUR");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Code).HasMaxLength(10).IsRequired();
        builder.HasIndex(m => m.Code).IsUnique();

        builder.Property(m => m.CurrencyName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.IntlCode).HasMaxLength(20);
        builder.Property(m => m.IntlDescription).HasMaxLength(200);
        builder.Property(m => m.HundredthName).HasMaxLength(100);
        builder.Property(m => m.English).HasMaxLength(100);
        builder.Property(m => m.EnglishHundredthName).HasMaxLength(100);
        builder.Property(m => m.IsoCurrencyCode).HasMaxLength(10);
        builder.Property(m => m.IncomingAmtDiff).HasPrecision(18, 2);
        builder.Property(m => m.OutgoingAmtDiff).HasPrecision(18, 2);
        builder.Property(m => m.IncomingPctDiff).HasPrecision(9, 4);
        builder.Property(m => m.OutgoingPctDiff).HasPrecision(9, 4);
        builder.Property(m => m.Rounding).HasMaxLength(30);
        builder.Property(m => m.Decimals).HasMaxLength(10);
    }
}
