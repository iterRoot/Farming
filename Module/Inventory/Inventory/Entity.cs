using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.ItemsMaster;

namespace FarmingApi.Modules.Sale.Inventory;

public class Inventory : AuditableEntity
{
    public string ItemsCode { get; set; } = null!;
    public string WhsCode { get; set; }  = null!;
    public decimal InQty { get; set; }
    public decimal OutQty { get; set; }
    public decimal Balance { get; set; }
}

public class InventoryConfig : IEntityTypeConfiguration<Inventory>
{
    public void Configure(EntityTypeBuilder<Inventory> builder)
    {
        builder.ToTable("Inventory");

        builder.HasKey(x => x.Id);

        builder.Property(m => m.ItemsCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.WhsCode)
            .HasMaxLength(200);

        builder.Property(m => m.InQty)
            .HasMaxLength(255);

        builder.Property(m => m.OutQty)
            .HasMaxLength(200);

        builder.Property(m => m.Balance)
            .HasMaxLength(50);

       
    }
}