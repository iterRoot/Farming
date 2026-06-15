using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.ExchangeRate;

// ═══════════════════════════════════════════════════════════════
// EXCHANGE RATE  (KRTT)
// 1 Currency = Rate BaseCurrency
// e.g. 1 USD = 4,100 KHR
// ═══════════════════════════════════════════════════════════════
public class ExchangeRate : AuditableEntity
{
    public string   Currency      { get; set; } = null!;  // "USD"
    public string   CurrencyName  { get; set; } = null!;  // "US Dollar"
    public string   BaseCurrency  { get; set; } = "KHR";
    public decimal  Rate          { get; set; }           // e.g. 4100.000000
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }           // "NBC","Bloomberg","Manual"
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; } = true;
}

// ═══════════════════════════════════════════════════════════════
// PRICE INDEX  (KPRI)
// Commodity / material price index for farming operations
// e.g. Rice Price Index, Fertiliser Index, Fuel Price
// ═══════════════════════════════════════════════════════════════
public class PriceIndex : AuditableEntity
{
    public string   IndexCode     { get; set; } = null!;  // "RICE-IDX"
    public string   IndexName     { get; set; } = null!;  // "Rice Price Index"
    public string?  Category      { get; set; }           // "Commodity","Fuel","Labor"
    public decimal  IndexValue    { get; set; }
    public string?  Unit          { get; set; }           // "USD/ton","KHR/kg"
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; } = true;
}

// ─── EF Configurations ───────────────────────────────────────
public class ExchangeRateConfig : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> b)
    {
        b.ToTable("KRTT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Currency).HasMaxLength(10).IsRequired();
        b.Property(x => x.CurrencyName).HasMaxLength(100).IsRequired();
        b.Property(x => x.BaseCurrency).HasMaxLength(10);
        b.Property(x => x.Rate).HasColumnType("decimal(18,6)");
        b.Property(x => x.Source).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}

public class PriceIndexConfig : IEntityTypeConfiguration<PriceIndex>
{
    public void Configure(EntityTypeBuilder<PriceIndex> b)
    {
        b.ToTable("KPRI");
        b.HasKey(x => x.Id);
        b.Property(x => x.IndexCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.IndexName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(100);
        b.Property(x => x.IndexValue).HasColumnType("decimal(18,6)");
        b.Property(x => x.Unit).HasMaxLength(50);
        b.Property(x => x.Source).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}