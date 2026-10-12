using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.ServiceContract;

// ═══════════════════════════════════════════════════════════════
// SERVICE CONTRACT  (KSCN)  ·  SAP B1 equivalent: OCTR
// A coverage agreement for a customer (optionally one item/serial)
// that a Service Call can be checked against — Status is only ever
// set here (Active/Cancelled); "expired" is computed from EndDate
// at read time, not a background job flipping a stored flag.
// ═══════════════════════════════════════════════════════════════
public class ServiceContract : AuditableEntity
{
    public string   ContractNo        { get; set; } = null!;  // "SC-2026-00001"
    public string   CustomerCode      { get; set; } = null!;
    public string   CustomerName      { get; set; } = null!;
    public string?  ItemCode          { get; set; }
    public string?  ItemName          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   ContractType      { get; set; } = "Standard"; // Standard/Extended/Premium/Custom
    public string   Status            { get; set; } = "Active";   // Active/Cancelled (Expired is computed)
    public DateTime StartDate         { get; set; } = DateTime.UtcNow.Date;
    public DateTime EndDate           { get; set; }
    public int      ResponseTimeHours { get; set; } = 24;
    public decimal  Value             { get; set; }
    public string?  CoverageTerms     { get; set; }
    public string?  Remarks           { get; set; }
}

public class ServiceContractConfig : IEntityTypeConfiguration<ServiceContract>
{
    public void Configure(EntityTypeBuilder<ServiceContract> b)
    {
        b.ToTable("KSCN");
        b.HasKey(x => x.Id);
        b.Property(x => x.ContractNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.ContractNo).IsUnique();
        b.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.ItemCode).HasMaxLength(50);
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.SerialNumber).HasMaxLength(100);
        b.Property(x => x.ContractType).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Active");
        b.Property(x => x.Value).HasPrecision(18, 2);
        b.Property(x => x.CoverageTerms).HasMaxLength(2000);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CustomerCode);
    }
}
