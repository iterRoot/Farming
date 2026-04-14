// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.Breeding;

// public class Breeding : AuditableEntity
// {
//         // public int CowId { get; set; }
//         // public int? ServiceBullId { get; set; }
//         public DateTime ServiceDate { get; set; }
//         public DateTime? ExpectedCalvingDate { get; set; }
//         public DateTime? ActualCalvingDate { get; set; }
//         public string? Outcome { get; set; }
//         public string? Notes { get; set; }
// 	// public ICollection<Position.Position> Positions { get; set; } = null!;
// }


// public class BreedingConfig : IEntityTypeConfiguration<Breeding>
// {
// 	public void Configure(EntityTypeBuilder<Breeding> builder)
// 	{
// 		builder.Property(m => m.ServiceDate)
// 			.HasMaxLength(100);
// 		builder.Property(m => m.ExpectedCalvingDate)
// 			.HasMaxLength(100);
// 		builder.Property(m => m.ActualCalvingDate)
// 			.HasMaxLength(20);
// 		builder.Property(m => m.Outcome)
// 			.HasMaxLength(255);
// 		builder.Property(m => m.Notes)
// 			.HasMaxLength(200);
// 			// 		builder.Property(m => m.Status)
// 			// .HasMaxLength(200);
// 			// 		builder.Property(m => m.TankNumber)
// 			// .HasMaxLength(200);
// 			// 					builder.Property(m => m.Desc)
// 			// .HasMaxLength(200);
// 		// builder.HasMany(m => m.Positions)
// 		// 	.WithOne(o => o.Breeding)
// 		// 	.HasForeignKey(k => k.BreedingId);
// 	}
// }