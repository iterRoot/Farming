using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.ApprovalProcess;

// ══════════════════════════════════════════════════════════════════════════════
// HEADER — one condition-based rule per document type (e.g. "PurchaseOrder,
// Total >= 10000"). A document routes through approval when Active and its
// Total matches Operator/Amount (both null = always matches).
// ══════════════════════════════════════════════════════════════════════════════
public class ApprovalTemplate : AuditableEntity
{
    public string   Name         { get; set; } = "";
    public string   DocumentType { get; set; } = "";   // one of ApprovalDocumentTypes.All
    public bool     Active       { get; set; } = true;
    public string?  Operator     { get; set; }          // ">" | ">=" | "<" | "<=" | "==" ; null = unconditional
    public decimal? Amount       { get; set; }
    public string?  Remarks      { get; set; }

    public ICollection<ApprovalTemplateApprover> Approvers { get; set; } = new List<ApprovalTemplateApprover>();
}

// ══════════════════════════════════════════════════════════════════════════════
// APPROVERS — single stage, any one listed user may approve or reject.
// ══════════════════════════════════════════════════════════════════════════════
public class ApprovalTemplateApprover : AuditableEntity
{
    public int              ApprovalTemplateId { get; set; }
    public ApprovalTemplate ApprovalTemplate   { get; set; } = null!;

    public int         UserId { get; set; }
    public UserEntity  User   { get; set; } = null!;
}

// ══════════════════════════════════════════════════════════════════════════════
// EF CONFIG
// ══════════════════════════════════════════════════════════════════════════════
public class ApprovalTemplateConfig : IEntityTypeConfiguration<ApprovalTemplate>
{
    public void Configure(EntityTypeBuilder<ApprovalTemplate> builder)
    {
        builder.ToTable("KAPT");   // Approval Template header
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Operator).HasMaxLength(5);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasMany(x => x.Approvers)
               .WithOne(x => x.ApprovalTemplate)
               .HasForeignKey(x => x.ApprovalTemplateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApprovalTemplateApproverConfig : IEntityTypeConfiguration<ApprovalTemplateApprover>
{
    public void Configure(EntityTypeBuilder<ApprovalTemplateApprover> builder)
    {
        builder.ToTable("APT1");   // Approval Template approvers
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
               .WithMany()
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
