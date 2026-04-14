using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Master.UserMaster;

public class UserMaster : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MidName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Note { get; set; } = null!;
    public string Tel1 { get; set; }
    public string? Tel2 { get; set; }
    public string? MobilePhone { get; set; }

    // Navigation
    public ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    public ICollection<UserContact> Contacts { get; set; } = new List<UserContact>();
    public ICollection<UserBankAcct> BankAccts { get; set; } = new List<UserBankAcct>();

    public TypeStatus TypeStatus { get; set; } = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}

public class UserAddress
{
    public int Id { get; set; }
    public int UserMasterId { get; set; }

    public string AddressType { get; set; } // B = Bill, S = Ship
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    public UserMaster UserMaster { get; set; }
}

public class UserContact
{
    public int Id { get; set; }
    public int UserMasterId { get; set; }

    public string ContactName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }

    public UserMaster UserMaster { get; set; }
}

public class UserBankAcct
{
    public int Id { get; set; }
    public int UserMasterId { get; set; }

    public string BankName { get; set; }
    public string BankAcct { get; set; }
    public string BankCode { get; set; }
    public string BankType { get; set; }

    public UserMaster UserMaster { get; set; }
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
public class UserAddressConfig : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddress");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.UserMaster)
               .WithMany(x => x.Addresses)
               .HasForeignKey(x => x.UserMasterId);
    }
}
public class UserContactConfig : IEntityTypeConfiguration<UserContact>
{
    public void Configure(EntityTypeBuilder<UserContact> builder)
    {
        builder.ToTable("UserContact");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.UserMaster)
               .WithMany(x => x.Contacts)
               .HasForeignKey(x => x.UserMasterId);
    }
}
public class UserBankAcctConfig : IEntityTypeConfiguration<UserBankAcct>
{
    public void Configure(EntityTypeBuilder<UserBankAcct> builder)
    {
        builder.ToTable("UserBankAcct");
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.UserMaster)
               .WithMany(x => x.BankAccts)
               .HasForeignKey(x => x.UserMasterId);
    }
}