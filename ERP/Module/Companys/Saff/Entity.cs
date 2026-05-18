using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Company.Staff;

public class Staff : AuditableEntity
{
	public string StaffName { get; set; } = null!;
	public string? StaffCode { get; set; }
	public string? Position { get; set; }
	public string? CompanyId { get; set; }
	public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}


public class StaffConfig : IEntityTypeConfiguration<Staff>
{
	public void Configure(EntityTypeBuilder<Staff> builder)
	{
		builder.Property(m => m.StaffName)
			.HasMaxLength(100);
		builder.Property(m => m.StaffCode)
			.HasMaxLength(100);
		builder.Property(m => m.Position)
			.HasMaxLength(20);
		builder.Property(m => m.CompanyId)
			.HasMaxLength(255);
		builder.Property(m => m.InActive)
			.HasMaxLength(200);
			        builder.Property(x => x.ActiveStatus)
            .HasConversion(
                v => v == ActiveStatus.Active ? "A" : "I",
                v => v == "A" ? ActiveStatus.Active : ActiveStatus.InActive
            );
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Company)
		// 	.HasForeignKey(k => k.CompanyId);
	}
}