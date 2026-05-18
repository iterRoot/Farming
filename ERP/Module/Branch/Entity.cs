using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Branch;

public class Branch : AuditableEntity
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; }

}

public class BranchConfig : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("KBRN");

        builder.HasKey(x => x.Id);
        builder.HasKey(x => x.BranchCode);


        builder.Property(m => m.BranchName)
            .HasMaxLength(100)
            .IsRequired();

    }
}