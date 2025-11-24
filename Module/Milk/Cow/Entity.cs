using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.MilkCow;

public class Cow : AuditableEntity
{
	public string Sex { get; set; } = null!;
	public int? Age { get; set; }
	public string? Birthday { get; set; }
    public string? Variety { get; set; }
	public string? Desc { get; set; }
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class CowConfig : IEntityTypeConfiguration<Cow>
{
	public void Configure(EntityTypeBuilder<Cow> builder)
	{
		builder.Property(m => m.Sex)
			.HasMaxLength(100);
		builder.Property(m => m.Age)
			.HasMaxLength(100);
		builder.Property(m => m.Birthday)
			.HasMaxLength(20);
		builder.Property(m => m.Variety)
			.HasMaxLength(255);
		builder.Property(m => m.Desc)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Cow)
		// 	.HasForeignKey(k => k.CowId);
	}
}