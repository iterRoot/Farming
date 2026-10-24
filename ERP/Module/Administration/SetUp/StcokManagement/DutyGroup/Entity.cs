using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Inventory.DutyGroups;

// ═══════════════════════════════════════════════════════════════
// DUTY GROUP (OCST)
// SAP B1 equivalent: Customs Groups / Duty Groups
// Groups items by import duty rate for customs calculation
// e.g. "Agricultural Seeds" → 0%, "Electronics" → 15%
// ═══════════════════════════════════════════════════════════════
public class DutyGroup : AuditableEntity
{
    public string  Code        { get; set; } = null!;  // e.g. "DG-001"
    public string  Name        { get; set; } = null!;  // e.g. "Agricultural Seeds"
    public decimal DutyRate    { get; set; }           // % import duty e.g. 5.00
    public string? Description { get; set; }
    public string? HsCode      { get; set; }           // HS/Tariff code e.g. "1001.10"
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
}

public class DutyGroupConfig : IEntityTypeConfiguration<DutyGroup>
{
    public void Configure(EntityTypeBuilder<DutyGroup> builder)
    {
        builder.ToTable("KCST");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DutyRate).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.HsCode).HasMaxLength(50);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}