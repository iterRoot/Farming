using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.HouseCow;

public class HouseCow : AuditableEntity
{
    public string? HouseName { get; set; }
    public string? Location { get; set; }
    public int Capacity { get; set; }
	public string? Desc { get; set; }
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class HouseCowConfig : IEntityTypeConfiguration<HouseCow>
{
	public void Configure(EntityTypeBuilder<HouseCow> builder)
	{
		builder.Property(m => m.HouseName)
			.HasMaxLength(100);
		builder.Property(m => m.Location)
			.HasMaxLength(100);
		builder.Property(m => m.Capacity)
			.HasMaxLength(20);
		builder.Property(m => m.Desc)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.HouseCow)
		// 	.HasForeignKey(k => k.HouseCowId);
	}
}