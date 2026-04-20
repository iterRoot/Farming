using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Master.BusinessPartner;

public class BusinessPartner : AuditableEntity
{
    public string Code { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string MidName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public string Tel1 { get; set; } = string.Empty;
    public string? Tel2 { get; set; }
    public string? MobilePhone { get; set; }

    public ICollection<BPAddress> Addresses { get; set; } = new List<BPAddress>();
    public ICollection<BPContact> Contacts { get; set; } = new List<BPContact>();
    public ICollection<BPBankAccount> BankAccts { get; set; } = new List<BPBankAccount>();

    public TypeStatus TypeStatus { get; set; } = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}

public class BPAddress
{
    public int Id { get; set; }
    public int BusinessPartnerId { get; set; }

    public string AddressType { get; set; } = "B";
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public BusinessPartner BusinessPartner { get; set; }
}

public class BPContact
{
    public int Id { get; set; }
    public int BusinessPartnerId { get; set; }

    public string ContactName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public BusinessPartner BusinessPartner { get; set; }
}

public class BPBankAccount
{
    public int Id { get; set; }
    public int BusinessPartnerId { get; set; }

    public string BankName { get; set; } = string.Empty;
    public string BankAcct { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string BankType { get; set; } = string.Empty;

    public BusinessPartner BusinessPartner { get; set; }
}
public class BusinessPartnerConfig : IEntityTypeConfiguration<BusinessPartner>
{
    public void Configure(EntityTypeBuilder<BusinessPartner> builder)
    {
        builder.ToTable("BusinessPartner"); // 🔥 rename table

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Code).IsUnique();

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();

        builder.Property(x => x.TypeStatus)
            .HasConversion(
                v => v == TypeStatus.Customer ? "C" : "V",
                v => v == "C" ? TypeStatus.Customer : TypeStatus.Vendor
            );

        builder.Property(x => x.ActiveStatus)
            .HasConversion(
                v => v == ActiveStatus.Active ? "A" : "I",
                v => v == "A" ? ActiveStatus.Active : ActiveStatus.InActive
            );
    }
}
public class BPAddressConfig : IEntityTypeConfiguration<BPAddress>
{
    public void Configure(EntityTypeBuilder<BPAddress> builder)
    {
builder.ToTable("BPAddress");

builder.HasOne(x => x.BusinessPartner)
       .WithMany(x => x.Addresses)
       .HasForeignKey(x => x.BusinessPartnerId);
    }
}

public class BPContactConfig : IEntityTypeConfiguration<BPContact>
{
    public void Configure(EntityTypeBuilder<BPContact> builder)
    {
        builder.ToTable("BPContact");

        builder.HasOne(x => x.BusinessPartner)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.BusinessPartnerId);
    }
}
public class BPBankAccountConfig : IEntityTypeConfiguration<BPBankAccount>
{
    public void Configure(EntityTypeBuilder<BPBankAccount> builder)
    {
        builder.ToTable("BPBankAccount");

        builder.HasOne(x => x.BusinessPartner)
            .WithMany(x => x.BankAccts)
            .HasForeignKey(x => x.BusinessPartnerId);
    }
}