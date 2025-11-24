using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.CowFeed;

public class CowFeed : AuditableEntity
{


    public string FoodType { get; set; } = null!;
    public string? FoodName { get; set; }
    public string? FoodSource { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "kg";

    public DateTime FeedingTime { get; set; }
    public string FeedingSession { get; set; } = "Morning";
    public string? Reason { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? TotalCost { get; set; }

    // public Cow Cow { get; set; } = null!;
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class CowFeedConfig : IEntityTypeConfiguration<CowFeed>
{
	public void Configure(EntityTypeBuilder<CowFeed> builder)
	{
		builder.Property(m => m.FoodType)
			.HasMaxLength(100);
		builder.Property(m => m.FoodName)
			.HasMaxLength(100);
		builder.Property(m => m.FoodSource)
			.HasMaxLength(20);
		builder.Property(m => m.FeedingTime)
			.HasMaxLength(255);
		builder.Property(m => m.FeedingSession)
			.HasMaxLength(200);
		builder.Property(m => m.Reason)
			.HasMaxLength(200);
		builder.Property(m => m.UnitPrice)
			.HasMaxLength(200);
		builder.Property(m => m.TotalCost)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.CowFeed)
		// 	.HasForeignKey(k => k.CowFeedId);
	}
}