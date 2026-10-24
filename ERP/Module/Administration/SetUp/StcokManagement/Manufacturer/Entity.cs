using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.Manufacturers;

// ═══════════════════════════════════════════════════════════════
// MANUFACTURER (OMRC)
// SAP B1 equivalent: Manufacturer setup
// Links items to their manufacturer for traceability & reporting
// ═══════════════════════════════════════════════════════════════
public class Manufacturer : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "MFG-001"
    public string  Name        { get; set; } = null!;  // e.g. "Bayer CropScience"
    public string? Country     { get; set; }           // e.g. "Germany"
    public string? Website     { get; set; }
    public string? Phone       { get; set; }
    public string? Email       { get; set; }
    public string? ContactName { get; set; }
    public string? Description { get; set; }
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class ManufacturerConfig : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.ToTable("KMRC");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.ContactName).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}