using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.BusinessPartners.BPProperties;

// ═══════════════════════════════════════════════════════════════
// BUSINESS PARTNER PROPERTY  (KBPP)
// SAP B1 equivalent: Business Partner Properties
// Named yes/no attributes assigned to customers and vendors
// for filtering, segmentation and reporting.
//
// e.g.  VIP Customer  |  Credit Risk  |  Exporter
//       Wholesaler    |  Organic Cert |  Government
//
// AppliesTo:
//   Customer → only shown on customer BPs
//   Vendor   → only shown on vendor BPs
//   Both     → shown on all BP types
// ═══════════════════════════════════════════════════════════════
public class BPProperty : AuditableEntity
{
    public int     PropertyNo  { get; set; }             // 1..64 slot (auto-assigned)
    public string  Code        { get; set; } = null!;    // e.g. "VIP"
    public string  Name        { get; set; } = null!;    // e.g. "VIP Customer"
    public string  AppliesTo   { get; set; } = "Both";  // Customer / Vendor / Both
    public string? Description { get; set; }
    public string? Color       { get; set; }             // badge colour hex e.g. "#f59e0b"
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class BPPropertyConfig : IEntityTypeConfiguration<BPProperty>
{
    public void Configure(EntityTypeBuilder<BPProperty> b)
    {
        b.ToTable("KBPP");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.AppliesTo).HasMaxLength(20);
        b.Property(x => x.Description).HasMaxLength(500);
        b.Property(x => x.Color).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
    }
}