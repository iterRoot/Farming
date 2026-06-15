using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.UserGroups;

// ═══════════════════════════════════════════════════════════════
// USER GROUP  (KUGR)
// SAP B1 equivalent: User Groups
// Organises users into named groups (roles). Each group carries
// a set of module-level permissions (Full / Read / None).
//
// Structure:
//   UserGroup (KUGR) 1 ──► UserGroupMember (KUG1) many  [who]
//   UserGroup (KUGR) 1 ──► UserGroupPermission (KUG2) many [what]
// ═══════════════════════════════════════════════════════════════
public class UserGroup : AuditableEntity
{
    public string   Code        { get; set; } = null!;  // e.g. "GRP-SALES"
    public string   Name        { get; set; } = null!;  // e.g. "Sales Team"
    public string?  Description { get; set; }
    public string   GroupType   { get; set; } = "Custom"; // System / Custom
    public string?  Color       { get; set; }           // badge color hex
    public bool     IsActive    { get; set; } = true;
    public string?  Remarks     { get; set; }

    public ICollection<UserGroupMember>     Members     { get; set; } = new List<UserGroupMember>();
    public ICollection<UserGroupPermission> Permissions { get; set; } = new List<UserGroupPermission>();
}

// ─────────────────────────────────────────────────────────────
// USER GROUP MEMBER  (KUG1)
// ─────────────────────────────────────────────────────────────
public class UserGroupMember
{
    public int     Id          { get; set; }
    public int     UserGroupId { get; set; }
    public string  UserCode    { get; set; } = null!;  // FK to KUDF.UserCode
    public string  FullName    { get; set; } = null!;
    public string? Email       { get; set; }
    public bool    IsGroupAdmin{ get; set; } = false;  // can manage group members

    public UserGroup? UserGroup { get; set; }
}

// ─────────────────────────────────────────────────────────────
// USER GROUP PERMISSION  (KUG2)
// Per-module access flags
// AccessLevel: Full / ReadOnly / None
// ─────────────────────────────────────────────────────────────
public class UserGroupPermission
{
    public int     Id           { get; set; }
    public int     UserGroupId  { get; set; }
    public string  Module       { get; set; } = null!;  // e.g. "Inventory"
    public string  SubModule    { get; set; } = null!;  // e.g. "ItemMaster"
    public bool    CanCreate    { get; set; } = false;
    public bool    CanRead      { get; set; } = true;
    public bool    CanUpdate    { get; set; } = false;
    public bool    CanDelete    { get; set; } = false;
    public bool    CanApprove   { get; set; } = false;
    public bool    CanExport    { get; set; } = false;

    public UserGroup? UserGroup { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class UserGroupConfig : IEntityTypeConfiguration<UserGroup>
{
    public void Configure(EntityTypeBuilder<UserGroup> b)
    {
        b.ToTable("KUGR");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.GroupType).HasMaxLength(20);
        b.Property(x => x.Color).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);
        b.HasIndex(x => x.Code).IsUnique();

        b.HasMany(x => x.Members)
         .WithOne(x => x.UserGroup)
         .HasForeignKey(x => x.UserGroupId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Permissions)
         .WithOne(x => x.UserGroup)
         .HasForeignKey(x => x.UserGroupId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserGroupMemberConfig : IEntityTypeConfiguration<UserGroupMember>
{
    public void Configure(EntityTypeBuilder<UserGroupMember> b)
    {
        b.ToTable("KUG1");
        b.HasKey(x => x.Id);
        b.Property(x => x.UserCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
    }
}

public class UserGroupPermissionConfig : IEntityTypeConfiguration<UserGroupPermission>
{
    public void Configure(EntityTypeBuilder<UserGroupPermission> b)
    {
        b.ToTable("KUG2");
        b.HasKey(x => x.Id);
        b.Property(x => x.Module).HasMaxLength(100).IsRequired();
        b.Property(x => x.SubModule).HasMaxLength(100).IsRequired();
    }
}