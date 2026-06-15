using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.UserDefaults;

// ═══════════════════════════════════════════════════════════════
// USER DEFAULTS  (KUDF)
// SAP B1 equivalent: User Defaults
// Per-user preset values that auto-fill when opening new
// documents or transactions. One record per user.
//
// Sections:
//   Identity    → which user this applies to
//   Documents   → default warehouse, price list, payment terms
//   Financials  → currency, GL account, cost centre
//   Sales       → default sales employee, series
//   Purchasing  → default buyer, series
//   Printing    → default printer, report language
//   UI          → date format, decimal separator, theme
// ═══════════════════════════════════════════════════════════════
public class UserDefault : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   UserCode         { get; set; } = null!;  // e.g. "U001"
    public string   FullName         { get; set; } = null!;  // e.g. "Dara Sok"
    public string?  Email            { get; set; }
    public string?  Role             { get; set; }           // Admin / Sales / Purchasing ...

    // ── Document Defaults ─────────────────────────────────────
    public string?  DefaultWarehouse { get; set; }
    public string?  DefaultLocation  { get; set; }
    public string?  DefaultPriceList { get; set; }
    public string?  DefaultPaymentTerms { get; set; }
    public string?  DefaultShippingType { get; set; }

    // ── Financial Defaults ────────────────────────────────────
    public string?  DefaultCurrency  { get; set; } = "KHR";
    public string?  DefaultGLAccount { get; set; }
    public string?  DefaultCostCentre{ get; set; }
    public string?  DefaultProject   { get; set; }
    public int?     DefaultBranch    { get; set; }

    // ── Sales Defaults ────────────────────────────────────────
    public string?  DefaultSalesEmployee { get; set; }
    public string?  DefaultSalesRegion   { get; set; }
    public string?  DefaultARSeries      { get; set; }  // document number series
    public string?  DefaultCustomerGroup { get; set; }

    // ── Purchasing Defaults ───────────────────────────────────
    public string?  DefaultBuyer       { get; set; }
    public string?  DefaultAPSeries    { get; set; }
    public string?  DefaultVendorGroup { get; set; }

    // ── Printing & UI ─────────────────────────────────────────
    public string?  DefaultPrinter     { get; set; }
    public string   DateFormat         { get; set; } = "DD/MM/YYYY";
    public string   DecimalSeparator   { get; set; } = ".";
    public string   ThousandSeparator  { get; set; } = ",";
    public int      DecimalPlaces      { get; set; } = 2;
    public string   Language           { get; set; } = "en";
    public string?  Theme              { get; set; } = "light";
    public bool     ShowTutorials      { get; set; } = true;
    public bool     ConfirmOnClose     { get; set; } = true;
    public bool     AutoSave           { get; set; } = false;
    public int      AutoSaveMinutes    { get; set; } = 5;

    // ── Status ────────────────────────────────────────────────
    public bool     IsActive           { get; set; } = true;
    public string?  Remarks            { get; set; }
}

public class UserDefaultConfig : IEntityTypeConfiguration<UserDefault>
{
    public void Configure(EntityTypeBuilder<UserDefault> b)
    {
        b.ToTable("KUDF");
        b.HasKey(x => x.Id);

        b.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Role).HasMaxLength(50);

        b.Property(x => x.DefaultWarehouse).HasMaxLength(50);
        b.Property(x => x.DefaultLocation).HasMaxLength(50);
        b.Property(x => x.DefaultPriceList).HasMaxLength(100);
        b.Property(x => x.DefaultPaymentTerms).HasMaxLength(100);
        b.Property(x => x.DefaultShippingType).HasMaxLength(50);

        b.Property(x => x.DefaultCurrency).HasMaxLength(10);
        b.Property(x => x.DefaultGLAccount).HasMaxLength(50);
        b.Property(x => x.DefaultCostCentre).HasMaxLength(50);
        b.Property(x => x.DefaultProject).HasMaxLength(100);

        b.Property(x => x.DefaultSalesEmployee).HasMaxLength(100);
        b.Property(x => x.DefaultSalesRegion).HasMaxLength(100);
        b.Property(x => x.DefaultARSeries).HasMaxLength(50);
        b.Property(x => x.DefaultCustomerGroup).HasMaxLength(50);

        b.Property(x => x.DefaultBuyer).HasMaxLength(100);
        b.Property(x => x.DefaultAPSeries).HasMaxLength(50);
        b.Property(x => x.DefaultVendorGroup).HasMaxLength(50);

        b.Property(x => x.DefaultPrinter).HasMaxLength(200);
        b.Property(x => x.DateFormat).HasMaxLength(20);
        b.Property(x => x.DecimalSeparator).HasMaxLength(5);
        b.Property(x => x.ThousandSeparator).HasMaxLength(5);
        b.Property(x => x.Language).HasMaxLength(10);
        b.Property(x => x.Theme).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.UserCode).IsUnique();
    }
}