using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Expense;

public class Expense : AuditableEntity
{
        public DateTime ServiceDate { get; set; }
        public DateTime? ExpectedCalvingDate { get; set; }
        public DateTime? ActualCalvingDate { get; set; }
        public string? Outcome { get; set; }
        public string? Notes { get; set; }
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class ExpenseConfig : IEntityTypeConfiguration<Expense>
{
	public void Configure(EntityTypeBuilder<Expense> builder)
	{
		builder.Property(m => m.ServiceDate)
			.HasMaxLength(100);
		builder.Property(m => m.ExpectedCalvingDate)
			.HasMaxLength(100);
		builder.Property(m => m.ActualCalvingDate)
			.HasMaxLength(20);
		builder.Property(m => m.Outcome)
			.HasMaxLength(255);
		builder.Property(m => m.Notes)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Expense )
		// 	.HasForeignKey(k => k.Expense Id);
	}
}