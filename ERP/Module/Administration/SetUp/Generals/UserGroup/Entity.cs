using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.SetUp.UserGroup;

public class UserGroup : AuditableEntity
{
    public string GroupName { get; set; } = null!;
    public string? GroupDec { get; set; }
    public string? Allowences { get; set; }
    public int? TPLId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? GroupType { get; set; }
    public int? CockpitId { get; set; }
    public int? LogInstanc { get; set; }
    public int? UserSign { get; set; }
    public int? UserSign2 { get; set; }
    public int VersionNum { get; set; } = 1;
}

public class UserGroupConfig : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> builder)
    {
        builder.ToTable("UserGroup");

        builder.HasKey(x => x.Id);

        builder.Property(m => m.GroupName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.GroupDec)
            .HasMaxLength(500);

        builder.Property(m => m.Allowences)
            .HasMaxLength(2000);

        builder.Property(m => m.GroupType)
            .HasMaxLength(50);

        builder.Property(m => m.VersionNum)
            .HasDefaultValue(1);

        builder.HasIndex(m => m.GroupName);
        builder.HasIndex(m => m.GroupType);
    }
}