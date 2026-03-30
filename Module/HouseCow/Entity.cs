using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;
using FarmingApi.Modules.Sheep;

namespace FarmingApi.Modules.House;


public class House : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
    public int? Capacity { get; set; }
    public string? Description { get; set; }

    public ICollection<Sheep.Sheep>? Sheep { get; set; } = new List<Sheep.Sheep>();
}

public class HouseConfig : IEntityTypeConfiguration<House>
{
    public void Configure(EntityTypeBuilder<House> builder)
    {
        builder.ToTable("Houses");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Name).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Location).HasMaxLength(100);
        builder.Property(m => m.Description).HasMaxLength(255);
        builder.Property(m => m.Capacity);
    }
}