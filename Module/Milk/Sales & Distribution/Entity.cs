using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Sales;

public class Sales : AuditableEntity
{
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime CollectedFrom { get; set; }
        public DateTime CollectedTo { get; set; }
        public decimal TotalVolumeLiters { get; set; }
        public decimal? AvgFatPercent { get; set; }
        public decimal? AvgSnfPercent { get; set; }
        public string Status { get; set; } = "collected";
        public string? TankNumber { get; set; }
		public string? Desc{ get; set;}
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class BreedingConfig : IEntityTypeConfiguration<Sales>
{
	public void Configure(EntityTypeBuilder<Sales> builder)
	{
		builder.Property(m => m.BatchNumber)
			.HasMaxLength(100);
		builder.Property(m => m.CollectedFrom)
			.HasMaxLength(100);
		builder.Property(m => m.CollectedTo)
			.HasMaxLength(20);
		builder.Property(m => m.TotalVolumeLiters)
			.HasMaxLength(255);
		builder.Property(m => m.AvgFatPercent)
			.HasMaxLength(200);
					builder.Property(m => m.Status)
			.HasMaxLength(200);
					builder.Property(m => m.TankNumber)
			.HasMaxLength(200);
								builder.Property(m => m.Desc)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Sales)
		// 	.HasForeignKey(k => k.BreedingId);
	}
}