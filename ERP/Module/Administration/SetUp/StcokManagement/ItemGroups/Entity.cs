using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.ItemGroup;

// ═══════════════════════════════════════════════════════════════
// ITEM GROUP (OITB)
// SAP B1 equivalent: OITB — groups items for reporting & defaults
// ═══════════════════════════════════════════════════════════════
public class ItemGroup : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "IG-001"
    public string  Name        { get; set; } = null!;  // e.g. "Seeds"
    public string? Description { get; set; }
    public string? DefaultUom  { get; set; }           // default unit for items in group
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class ItemGroupConfig : IEntityTypeConfiguration<ItemGroup>
{
    public void Configure(EntityTypeBuilder<ItemGroup> builder)
    {
        builder.ToTable("KITB");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.DefaultUom).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}