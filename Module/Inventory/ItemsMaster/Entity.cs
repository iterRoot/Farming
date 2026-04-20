using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Master.ItemsMaster;

public class ItemsMaster : AuditableEntity
{
    public int Id { get; set; }

    public string ItemCode { get; set; } = null!;
    public string ItemName { get; set; } = null!;

    public string UomName { get; set; } = string.Empty;
    public string UomCode { get; set; } = string.Empty;

    public decimal Price { get; set; }
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

        builder.HasIndex(x => x.ItemCode).IsUnique();

        builder.Property(x => x.ItemCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ItemName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Status)
            .HasConversion(
                v => v == ItemStatus.Active ? "A" : "I",
                v => v == "A" ? ItemStatus.Active : ItemStatus.InActive
            )
            .HasMaxLength(1);

        // 🔥 prevent empty string
        builder.HasCheckConstraint(
            "CK_ItemsMaster_ItemCode_NotEmpty",
            "\"ItemCode\" <> ''"
        );
    }
}