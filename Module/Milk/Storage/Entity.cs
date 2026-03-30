using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Storage;

public class Storage : AuditableEntity
{
		public string TargetType { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public DateTime TestedAt { get; set; }
        public string TestType { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public string? ResultJson { get; set; }
        public string? Analyst { get; set; }
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class StorageConfig : IEntityTypeConfiguration<Storage>
{
	public void Configure(EntityTypeBuilder<Storage> builder)
	{
		builder.Property(m => m.TargetType)
			.HasMaxLength(100);
		builder.Property(m => m.TargetId)
			.HasMaxLength(100);
		builder.Property(m => m.TestedAt)
			.HasMaxLength(255);
		builder.Property(m => m.TestType)
			.HasMaxLength(200);
					builder.Property(m => m.Passed)
			.HasMaxLength(200);
					builder.Property(m => m.ResultJson)
			.HasMaxLength(200);
								builder.Property(m => m.Analyst)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Storage)
		// 	.HasForeignKey(k => k.StorageId);
	}
}