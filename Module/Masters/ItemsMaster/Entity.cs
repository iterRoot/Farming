using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.ItemsMaster;


namespace FarmingApi.Modules.Master.ItemsMaster;
public class ItemsMaster : AuditableEntity 
{
    public string ItemsCode { get; set; }
    public string ItemsName {get; set;}
    public string UomName {get; set;}
    public string UomCode {get; set;}
    public string Price {get; set;}
    public string PriceList {get; set;}
    // public DateTime CreatedAt {get; set;}
    // public string CreatedBy {get; set;}
    // public DateTime UpdatedAt {get; set;}
    // public string UpdatedBy {get; set;}
    // public DateTime DeletedAt {get; set;}
    // public string DeletedBy {get; set;}
    public string Types {get; set;}
    public ItemStatus Status {get; set;} = ItemStatus.Active;
}

public class ItemsMasterConfig : IEntityTypeConfiguration<ItemsMaster>
{
    public void Configure(EntityTypeBuilder<ItemsMaster> builder)
    {
        builder.ToTable("ItemsMaster");
        builder.HasKey(x => x.Id);
        builder.HasKey(x => x.ItemsCode);


        builder.Property(m => m.ItemsCode).HasMaxLength(100).IsRequired();
        builder.Property(m => m.ItemsName).HasMaxLength(20);
        builder.Property(m => m.UomName).HasMaxLength(255);
        builder.Property(m => m.UomCode).HasMaxLength(200);
        builder.Property(m => m.Price).HasMaxLength(10);

        builder.Property(m => m.PriceList).HasMaxLength(200);
        builder.Property(m => m.Types).HasMaxLength(200);

    }
}