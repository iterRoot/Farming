

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Master.UserMaster;

namespace FarmingApi.Modules.Master.UserMaster;
public class UserMaster : AuditableEntity

{
    public string Code { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MidName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Note { get; set; } = null!;

    public TypeStatus TypeStatus { get; set; } = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}

public class UserMasterConfig : IEntityTypeConfiguration<UserMaster>
{
    public void Configure(EntityTypeBuilder<UserMaster> builder)
    {
        builder.ToTable("UserMaster");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(m => m.Code).HasMaxLength(50).IsRequired();
        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.MidName).HasMaxLength(100);
        builder.Property(m => m.LastName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Note).HasMaxLength(255);

        builder.Property(x => x.TypeStatus)
            .HasConversion(
                v => v == TypeStatus.Customer ? "C" : "V",
                v => v == "C" ? TypeStatus.Customer : TypeStatus.Vendor
            )
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(x => x.ActiveStatus)
            .HasConversion(
                v => v == ActiveStatus.Active ? "A" : "I",
                v => v == "A" ? ActiveStatus.Active : ActiveStatus.InActive
            )
            .HasMaxLength(1)
            .IsRequired();
    }
}
