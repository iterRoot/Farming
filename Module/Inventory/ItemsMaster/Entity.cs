using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.ItemsMaster;


namespace FarmingApi.Modules.Master.ItemsMaster;
public class ItemsMaster : AuditableEntity
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;

    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;

    public decimal Price { get; set; }   // ✅ FIXED
    public string PriceList { get; set; } = string.Empty;

    public string Types { get; set; } = string.Empty;

    public ItemStatus Status { get; set; } = ItemStatus.Active;
}

public class ItemsMasterConfig : IEntityTypeConfiguration<ItemsMaster>
{
    public void Configure(EntityTypeBuilder<ItemsMaster> builder)
    {
        builder.ToTable("ItemsMaster");

        builder.HasKey(x => x.Id);

        // ✅ Unique Item Code (important)
        builder.HasIndex(x => x.ItemCode).IsUnique();

        builder.Property(x => x.ItemCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ItemName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.UomName)
            .HasMaxLength(50);

        builder.Property(x => x.UomCode)
            .HasMaxLength(20);

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.PriceList)
            .HasMaxLength(50);

        builder.Property(x => x.Types)
            .HasMaxLength(50);

builder.Property(x => x.Status)
    .HasConversion(
        v => v == ItemStatus.Active ? "A" : "I",
        v => v == "A" ? ItemStatus.Active : ItemStatus.InActive
    )
    .HasMaxLength(1);
    }
}