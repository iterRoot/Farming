using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Licence;

// License packages — the subscription tiers offered to companies.
// Table KLCP.
public class LicensePackage : AuditableEntity
{
    public string  Name         { get; set; } = null!;
    public string? Code         { get; set; }
    public string? Description   { get; set; }
    public decimal Price         { get; set; }
    public string  BillingCycle  { get; set; } = "Monthly";   // Monthly | Yearly | One-Time
    public int     MaxUsers      { get; set; } = 1;
    public int     TrialDays     { get; set; }
    public bool    IsActive      { get; set; } = true;
    public bool    IsFeatured    { get; set; }

    // Module names the package unlocks. Npgsql maps this to a text[] column.
    public List<string> Modules  { get; set; } = new();
}

public class LicensePackageConfig : IEntityTypeConfiguration<LicensePackage>
{
    public void Configure(EntityTypeBuilder<LicensePackage> builder)
    {
        builder.ToTable("KLCP");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Name).HasMaxLength(120).IsRequired();
        builder.Property(m => m.Code).HasMaxLength(40);
        builder.Property(m => m.Description).HasMaxLength(1000);
        builder.Property(m => m.Price).HasPrecision(18, 2);
        builder.Property(m => m.BillingCycle).HasMaxLength(20).HasDefaultValue("Monthly");
        builder.Property(m => m.MaxUsers).HasDefaultValue(1);

        builder.HasIndex(m => m.Code);
        builder.HasIndex(m => m.IsActive);
    }
}
