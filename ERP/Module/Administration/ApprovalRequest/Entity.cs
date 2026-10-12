using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalProcess;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.ApprovalRequest;

// ══════════════════════════════════════════════════════════════════════════════
// One row per document submitted for approval. DocumentId is deliberately NOT a
// database foreign key — polymorphic, like the BaseEntry/BaseType pattern used
// elsewhere: DocumentType names which of the 23 document tables DocumentId
// points into, resolved in application code rather than a single hard-coded FK.
// ══════════════════════════════════════════════════════════════════════════════
public class ApprovalRequest : AuditableEntity
{
    public string  DocumentType { get; set; } = "";
    public int     DocumentId   { get; set; }
    public string  DocNum       { get; set; } = "";   // snapshot at submission time
    public decimal Total        { get; set; }         // snapshot at submission time

    public int              ApprovalTemplateId { get; set; }
    public ApprovalTemplate ApprovalTemplate   { get; set; } = null!;

    public string Status { get; set; } = "Pending";   // Pending | Approved | Rejected

    public int        RequestedBy     { get; set; }
    public UserEntity  RequestedByUser { get; set; } = null!;
    public DateTime   RequestedAt     { get; set; }

    public int?        DecidedBy     { get; set; }
    public UserEntity? DecidedByUser { get; set; }
    public DateTime?   DecidedAt     { get; set; }
    public string?     Remarks       { get; set; }
}

public class ApprovalRequestConfig : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("KAPR");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DocNum).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Total).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Remarks).HasMaxLength(500);

        builder.HasOne(x => x.ApprovalTemplate)
               .WithMany()
               .HasForeignKey(x => x.ApprovalTemplateId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.RequestedByUser)
               .WithMany()
               .HasForeignKey(x => x.RequestedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DecidedByUser)
               .WithMany()
               .HasForeignKey(x => x.DecidedBy)
               .OnDelete(DeleteBehavior.Restrict)
               .IsRequired(false);

        builder.HasIndex(x => new { x.DocumentType, x.DocumentId, x.Status });
    }
}
