using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

// ═══════════════════════════════════════════════════════════════
// SAVED SEARCH  (KSRCH / SRC1)  ·  SAP B1 equivalent: Formatted Search
// A reusable filter over one whitelisted entity (see Registry.cs).
// Stores structured field/operator/value rows only — never a raw
// query string — see Registry.cs for why that's the whole point.
// ═══════════════════════════════════════════════════════════════
public class SavedSearch : AuditableEntity
{
    public string  Name         { get; set; } = null!;
    public string  TargetEntity { get; set; } = null!;   // key into SearchableEntityRegistry.Targets
    public string? Description  { get; set; }
    public bool    IsActive     { get; set; } = true;

    public ICollection<SavedSearchFilter> Filters { get; set; } = new List<SavedSearchFilter>();
}

public class SavedSearchFilter
{
    public int    Id           { get; set; }
    public int    SavedSearchId{ get; set; }
    public int    LineNum      { get; set; }
    public string Field        { get; set; } = null!;   // must match a Registry field name for the parent's TargetEntity
    public string Operator     { get; set; } = null!;   // must match Registry.OperatorsByType for that field's type
    public string Value        { get; set; } = null!;

    public SavedSearch SavedSearch { get; set; } = null!;
}

public class SavedSearchConfig : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> b)
    {
        b.ToTable("KSRCH");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.TargetEntity).HasMaxLength(50).IsRequired();
        b.Property(x => x.Description).HasMaxLength(500);

        b.HasMany(x => x.Filters).WithOne(f => f.SavedSearch)
         .HasForeignKey(f => f.SavedSearchId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class SavedSearchFilterConfig : IEntityTypeConfiguration<SavedSearchFilter>
{
    public void Configure(EntityTypeBuilder<SavedSearchFilter> b)
    {
        b.ToTable("SRC1");
        b.HasKey(x => x.Id);
        b.Property(x => x.Field).HasMaxLength(50).IsRequired();
        b.Property(x => x.Operator).HasMaxLength(20).IsRequired();
        b.Property(x => x.Value).HasMaxLength(200).IsRequired();
    }
}
