using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Authorisations;

// SAP B1 "Authorisations" — one record per user holding a permission tree.
//
// The tree is a fixed catalogue of subjects, each set to one of three levels
// (None / Read / Full). Since it's always read and written as a whole for a
// user, the map is stored as a single jsonb document — adding a new subject
// needs no migration. FullAuthorization is a denormalised flag so the List
// can show status without parsing the document.
public class UserAuthorization : AuditableEntity
{
    public int    UserId            { get; set; }
    public bool   FullAuthorization { get; set; }
    /// <summary>{ "subjectKey": "Full" | "Read" | "None", ... }</summary>
    public string Permissions       { get; set; } = "{}";
}

public class UserAuthorizationConfig : IEntityTypeConfiguration<UserAuthorization>
{
    public void Configure(EntityTypeBuilder<UserAuthorization> b)
    {
        b.ToTable("KUAU");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.UserId).IsUnique();       // one authorisation record per user
        b.Property(x => x.Permissions).HasColumnType("jsonb").IsRequired();
    }
}
