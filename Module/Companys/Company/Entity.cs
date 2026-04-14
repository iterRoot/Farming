using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Company;

public class Company : AuditableEntity
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public string? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	// public ICollection<Position.Position> Positions { get; set; } = null!;
}


public class CompanyConfig : IEntityTypeConfiguration<Company>
{
	public void Configure(EntityTypeBuilder<Company> builder)
	{
		builder.Property(m => m.Logo)
			.HasMaxLength(100);
		builder.Property(m => m.Name)
			.HasMaxLength(100);
		builder.Property(m => m.ShortName)
			.HasMaxLength(20);
		builder.Property(m => m.Desc)
			.HasMaxLength(255);
		builder.Property(m => m.Address)
			.HasMaxLength(200);
		// builder.HasMany(m => m.Positions)
		// 	.WithOne(o => o.Company)
		// 	.HasForeignKey(k => k.CompanyId);
	}
}