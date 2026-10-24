using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.Security;

// SAP-B1-style "Read-Only DB User" screen: a registry of the read-only
// database logins your DBA has provisioned directly in Postgres for
// reporting tools (Crystal Reports, Excel, BI). This app does not create
// or drop the actual Postgres role — see Package/List.jsx for why — it
// just keeps a record of which accounts exist, for whom, and whether
// they're still meant to be active.
public class ReadOnlyDbUser : AuditableEntity
{
    public string  Username    { get; set; } = null!;
    public string? Description { get; set; }
    public bool    Enabled     { get; set; } = true;
}

public class ReadOnlyDbUserConfig : IEntityTypeConfiguration<ReadOnlyDbUser>
{
    public void Configure(EntityTypeBuilder<ReadOnlyDbUser> b)
    {
        b.ToTable("KRODBU");
        b.HasKey(x => x.Id);

        b.Property(x => x.Username).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasMaxLength(300);

        b.HasIndex(x => x.Username).IsUnique();
    }
}
