using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.ItemProperties;

// ═══════════════════════════════════════════════════════════════
// ITEM PROPERTY (OITG)
// SAP B1 equivalent: Item Properties — named yes/no attributes
// used to tag/filter items (e.g. Organic, Perishable, Imported)
// ═══════════════════════════════════════════════════════════════
public class ItemProperty : AuditableEntity
{
    public int     PropertyNo  { get; set; }            // 1..64 slot number
    public string  Code        { get; set; } = null!;   // e.g. "ORGANIC"
    public string  Name        { get; set; } = null!;   // e.g. "Organic Product"
    public string? Description { get; set; }
    public string? Color       { get; set; }            // optional UI badge color hex
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class ItemPropertyConfig : IEntityTypeConfiguration<ItemProperty>
{
    public void Configure(EntityTypeBuilder<ItemProperty> builder)
    {
        builder.ToTable("KITG");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Color).HasMaxLength(20);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}