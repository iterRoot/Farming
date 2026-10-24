using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Branch;

// SAP B1 style "Branches - Setup" matrix
public class Branch : AuditableEntity
{
    public string  BranchCode        { get; set; } = null!;
    public string  BranchName        { get; set; } = null!;
    public string? BranchNameForeign { get; set; }
    public string? BranchRegNo       { get; set; }
    public string? Address           { get; set; }
    public string? AddressForeign    { get; set; }

    public bool    MainBranch        { get; set; }
    public string? TaxOfficeNo       { get; set; }
    public bool    Disabled          { get; set; }

    // ── Defaults (free-text references for now) ────────────────
    public string? DefaultCustomerId          { get; set; }
    public string? DefaultSupplierId          { get; set; }
    public string? DefaultWarehouseId         { get; set; }
    public string? DefaultResourceWarehouseId { get; set; }

    // ── Address block ──────────────────────────────────────────
    public string? AliasName            { get; set; }
    public string? AddressType          { get; set; }
    public string? Street               { get; set; }
    public string? StreetNo             { get; set; }
    public string? BuildingFloorRoom    { get; set; }
    public string? Postcode             { get; set; }
    public string? Block                { get; set; }
    public string? City                 { get; set; }
    public string? State                { get; set; }
    public string? County               { get; set; }
    public string? CountryRegion        { get; set; }
    public string? GlobalLocationNumber { get; set; }
}

public class BranchConfig : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("KBRN");

        // NOTE: HasKey was previously called twice — the second call silently
        // replaced the first, making BranchCode the PK. Id is the real key
        // (from AuditableEntity); BranchCode is a unique business key.
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.BranchCode).IsUnique();

        builder.Property(m => m.BranchCode).HasMaxLength(50).IsRequired();
        builder.Property(m => m.BranchName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.BranchNameForeign).HasMaxLength(100);
        builder.Property(m => m.BranchRegNo).HasMaxLength(50);
        builder.Property(m => m.Address).HasMaxLength(250);
        builder.Property(m => m.AddressForeign).HasMaxLength(250);
        builder.Property(m => m.TaxOfficeNo).HasMaxLength(50);

        builder.Property(m => m.DefaultCustomerId).HasMaxLength(50);
        builder.Property(m => m.DefaultSupplierId).HasMaxLength(50);
        builder.Property(m => m.DefaultWarehouseId).HasMaxLength(50);
        builder.Property(m => m.DefaultResourceWarehouseId).HasMaxLength(50);

        builder.Property(m => m.AliasName).HasMaxLength(100);
        builder.Property(m => m.AddressType).HasMaxLength(50);
        builder.Property(m => m.Street).HasMaxLength(100);
        builder.Property(m => m.StreetNo).HasMaxLength(50);
        builder.Property(m => m.BuildingFloorRoom).HasMaxLength(150);
        builder.Property(m => m.Postcode).HasMaxLength(20);
        builder.Property(m => m.Block).HasMaxLength(50);
        builder.Property(m => m.City).HasMaxLength(100);
        builder.Property(m => m.State).HasMaxLength(100);
        builder.Property(m => m.County).HasMaxLength(100);
        builder.Property(m => m.CountryRegion).HasMaxLength(100);
        builder.Property(m => m.GlobalLocationNumber).HasMaxLength(50);
    }
}
