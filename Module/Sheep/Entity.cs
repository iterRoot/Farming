using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.House;


namespace FarmingApi.Modules.Sheep;

public class Sheep : AuditableEntity
{
    public string Sex { get; set; } = null!;
    public int? Age { get; set; }
    public string? Birthday { get; set; }
    public string? Variety { get; set; }
    public string? Desc { get; set; }

    public int? HouseId { get; set; }        // optional FK
    public House.House? House { get; set; }        // optional navigation

	// 	public string Sex { get; set; } = null!;
	// public int HouseId { get; set; }
	// public int? Age { get; set; }
	// public string? Birthday { get; set; }
    // public string? Variety { get; set; }
	// public string? Desc { get; set; }
	// public bool? InActive { get; set; }
}


public class SheepConfig : IEntityTypeConfiguration<Sheep>
{
    public void Configure(EntityTypeBuilder<Sheep> builder)
    {
        builder.ToTable("Sheep");
        builder.HasKey(x => x.Id);

        builder.Property(m => m.Sex).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Birthday).HasMaxLength(20);
        builder.Property(m => m.Variety).HasMaxLength(255);
        builder.Property(m => m.Desc).HasMaxLength(200);

        builder.HasOne(s => s.House)
            .WithMany(h => h.Sheep)
            .HasForeignKey(s => s.HouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

