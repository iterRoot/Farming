using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.Sale.Warehouse;

public class Warehouse : AuditableEntity
{
    public string WhsCode { get; set; } = null!;
    public string WhsName { get; set; } = null!;
}

public class WarehouseConfig : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouse");

        builder.HasKey(x => x.Id);
        builder.HasKey(x => x.WhsCode);

        builder.Property(m => m.WhsName)
            .HasMaxLength(250)
            .IsRequired();

       
    }
}