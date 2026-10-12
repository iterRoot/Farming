using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.AlertManagement;

// ══════════════════════════════════════════════════════════════════════════════
// One row per notification in a user's inbox — either a person-to-person
// message (SenderUserId set) or a system-raised alert (SenderUserId null,
// e.g. low stock, a document number range running out, an approval waiting
// on this user). ReferenceKey lets a system check be idempotent: re-running
// it skips recipients who already have an unread alert for the same
// condition instead of spamming a new row every time.
// ══════════════════════════════════════════════════════════════════════════════
public class Alert : AuditableEntity
{
    public int         RecipientUserId { get; set; }
    public UserEntity  RecipientUser   { get; set; } = null!;

    public int?        SenderUserId    { get; set; }
    public UserEntity? SenderUser      { get; set; }

    public string  Type         { get; set; } = "Message";  // Message | LowStock | DocNumberRangeLow | ApprovalPending | ExchangeRateStale
    public string  Title        { get; set; } = "";
    public string  Body         { get; set; } = "";
    public string? Link         { get; set; }                // frontend path the alert points at
    public string? ReferenceKey { get; set; }                 // dedupe key for system-raised alerts

    public bool      IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}

public class AlertConfig : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.ToTable("KALR");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Link).HasMaxLength(300);
        builder.Property(x => x.ReferenceKey).HasMaxLength(200);

        builder.HasOne(x => x.RecipientUser)
               .WithMany()
               .HasForeignKey(x => x.RecipientUserId)
               .OnDelete(DeleteBehavior.Cascade);

        // SetNull, not Restrict — a message is not an audit record the way an
        // ApprovalRequest is; deleting the sender should orphan old messages,
        // not permanently block deleting that user (the bug fixed earlier
        // this session on ApprovalTemplate's delete path).
        builder.HasOne(x => x.SenderUser)
               .WithMany()
               .HasForeignKey(x => x.SenderUserId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false);

        builder.HasIndex(x => new { x.RecipientUserId, x.IsRead });
        builder.HasIndex(x => new { x.RecipientUserId, x.ReferenceKey });
    }
}
