using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.MenuAlias;

// SAP B1 "Menu Alias for Searching": a user-defined keyword that resolves to a
// menu item, so the item can be found in the search box under a familiar name.
public class MenuAliasEntry : AuditableEntity
{
    public string  Alias    { get; set; } = null!;   // the search keyword
    public string  MenuName { get; set; } = null!;   // the real menu item
    public string? MenuPath { get; set; }            // route the item opens
    public string? Module   { get; set; }            // grouping (Administration, Sales…)
    public string? Language { get; set; }
}

public class MenuAliasEntryConfig : IEntityTypeConfiguration<MenuAliasEntry>
{
    public void Configure(EntityTypeBuilder<MenuAliasEntry> b)
    {
        b.ToTable("KMAL");
        b.HasKey(x => x.Id);
        b.Property(x => x.Alias).HasMaxLength(120).IsRequired();
        b.Property(x => x.MenuName).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Alias);
    }
}
