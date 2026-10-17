using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.KnowledgeBase;

// ═══════════════════════════════════════════════════════════════════
// KNOWLEDGE BASE SOLUTION  (KKBA)
// SAP B1 equivalent: Service > Solutions Knowledge Base. A reusable
// symptom → cause → solution record a service agent can attach to a
// Service Call. Optionally tied to an item.
// ═══════════════════════════════════════════════════════════════════
public class KnowledgeBaseArticle : AuditableEntity
{
    public string  Code      { get; set; } = null!;   // auto: KB-0001
    public string  Title     { get; set; } = null!;
    public string? Category  { get; set; }
    public string? ItemCode  { get; set; }            // optional related item
    public string? Symptom   { get; set; }
    public string? Cause     { get; set; }
    public string  Solution  { get; set; } = null!;
    public string  Status    { get; set; } = "Draft"; // Draft | Published | Archived
    public string? Tags      { get; set; }            // comma-separated
    public int     ViewCount { get; set; }
}

public class KnowledgeBaseArticleConfig : IEntityTypeConfiguration<KnowledgeBaseArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseArticle> b)
    {
        b.ToTable("KKBA");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Category).HasMaxLength(100);
        b.Property(x => x.ItemCode).HasMaxLength(50);
        b.Property(x => x.Symptom).HasMaxLength(2000);
        b.Property(x => x.Cause).HasMaxLength(2000);
        b.Property(x => x.Solution).HasMaxLength(4000).IsRequired();
        b.Property(x => x.Status).HasMaxLength(15);
        b.Property(x => x.Tags).HasMaxLength(300);
        b.HasIndex(x => x.Status);
    }
}
