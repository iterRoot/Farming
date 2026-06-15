using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Sales.Territories;

// ═══════════════════════════════════════════════════════════════
// TERRITORY  (KTER)
// SAP B1 equivalent: Sales Territories
// Hierarchical geographic/sales regions used to assign
// business partners and sales employees to areas.
//
// Hierarchy Example:
//   Country (KH)
//     └── Region (Central)
//           ├── Province (Phnom Penh)
//           │     └── District (Daun Penh)
//           └── Province (Kandal)
//
// Self-referencing ParentId enables unlimited depth.
// ═══════════════════════════════════════════════════════════════
public class Territory : AuditableEntity
{
    public string   Code          { get; set; } = null!;  // e.g. "TER-PP"
    public string   Name          { get; set; } = null!;  // e.g. "Phnom Penh"
    public int?     ParentId      { get; set; }           // self-ref parent territory
    public string?  ParentCode    { get; set; }           // denormalised for display
    public string?  ParentName    { get; set; }
    public int      Level         { get; set; } = 1;      // 1=Country 2=Region 3=Province 4=District
    public string?  LevelName     { get; set; }           // e.g. "Province"
    public string?  CountryCode   { get; set; }           // ISO-3166 e.g. "KH"
    public string?  Region        { get; set; }           // e.g. "Central"
    public string?  Manager       { get; set; }           // responsible sales person
    public string?  ManagerEmail  { get; set; }
    public decimal? TargetRevenue { get; set; }           // optional revenue target
    public string?  Currency      { get; set; } = "KHR";
    public string?  Description   { get; set; }
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }

    // Navigation
    public Territory?           Parent   { get; set; }
    public ICollection<Territory> Children { get; set; } = new List<Territory>();
}

public class TerritoryConfig : IEntityTypeConfiguration<Territory>
{
    public void Configure(EntityTypeBuilder<Territory> b)
    {
        b.ToTable("KTER");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.ParentCode).HasMaxLength(50);
        b.Property(x => x.ParentName).HasMaxLength(200);
        b.Property(x => x.LevelName).HasMaxLength(50);
        b.Property(x => x.CountryCode).HasMaxLength(10);
        b.Property(x => x.Region).HasMaxLength(100);
        b.Property(x => x.Manager).HasMaxLength(100);
        b.Property(x => x.ManagerEmail).HasMaxLength(200);
        b.Property(x => x.TargetRevenue).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).HasMaxLength(10);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();

        // Self-referencing hierarchy
        b.HasOne(x => x.Parent)
         .WithMany(x => x.Children)
         .HasForeignKey(x => x.ParentId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}