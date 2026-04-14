using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.ItemsMaster;

namespace FarmingApi.Modules.Branch;

public class Branch : AuditableEntity
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Active;
}

public class BranchConfig : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branch");

        builder.HasKey(x => x.Id);
        builder.HasKey(x => x.BranchCode);


        builder.Property(m => m.BranchName)
            .HasMaxLength(100)
            .IsRequired();

        // Enum stored as int
        builder.Property(m => m.Status)
            .HasConversion<int>()
            .IsRequired();
    }
}