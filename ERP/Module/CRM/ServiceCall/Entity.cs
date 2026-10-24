using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.ServiceCall;

// ═══════════════════════════════════════════════════════════════
// SERVICE CALL  (KSVC)  ·  SAP B1 equivalent: OSCL
// A support ticket against a customer, optionally against one of
// their items/serials, with a real Open -> InProgress -> OnHold ->
// Closed lifecycle and a resolution written on Close.
// ═══════════════════════════════════════════════════════════════
public class ServiceCall : AuditableEntity
{
    public string   ServiceCallNo     { get; set; } = null!;  // "SVC-2026-00001"
    public string   CustomerCode      { get; set; } = null!;
    public string   CustomerName      { get; set; } = null!;
    public string?  ContactPerson     { get; set; }
    public string?  ItemCode          { get; set; }
    public string?  ItemName          { get; set; }
    public string?  SerialNumber      { get; set; }
    public string   Subject           { get; set; } = null!;
    public string?  ProblemDescription{ get; set; }
    public string   Status            { get; set; } = "Open"; // Open/InProgress/OnHold/Closed
    public string   Priority          { get; set; } = "Medium"; // Low/Medium/High/Urgent
    public string?  AssignedTo        { get; set; }
    public DateTime OpenDate          { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate          { get; set; }
    public DateTime? ClosedDate       { get; set; }
    public string?  Resolution        { get; set; }
    public int?     ServiceContractId { get; set; }          // no FK — set once Service Contracts exists
    public string?  Remarks           { get; set; }
}

public class ServiceCallConfig : IEntityTypeConfiguration<ServiceCall>
{
    public void Configure(EntityTypeBuilder<ServiceCall> b)
    {
        b.ToTable("KSVC");
        b.HasKey(x => x.Id);
        b.Property(x => x.ServiceCallNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.ServiceCallNo).IsUnique();
        b.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.CustomerName).HasMaxLength(200);
        b.Property(x => x.ContactPerson).HasMaxLength(100);
        b.Property(x => x.ItemCode).HasMaxLength(50);
        b.Property(x => x.ItemName).HasMaxLength(200);
        b.Property(x => x.SerialNumber).HasMaxLength(100);
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.ProblemDescription).HasMaxLength(2000);
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Open");
        b.Property(x => x.Priority).HasMaxLength(10).HasDefaultValue("Medium");
        b.Property(x => x.AssignedTo).HasMaxLength(100);
        b.Property(x => x.Resolution).HasMaxLength(2000);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CustomerCode);
    }
}
