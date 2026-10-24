using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Financials.PostingTemplate;

// ═══════════════════════════════════════════════════════════════════
// POSTING TEMPLATE  (KBTT / BTT1)
// SAP B1 equivalent: Financials > Posting Templates (OBTT/BTT1). A
// reusable skeleton for journal entries that recur with the SAME
// accounts but DIFFERENT amounts. Each line names a G/L account, a side
// (Debit/Credit) and a percentage; when the template is used to key a
// Journal Entry the user enters one base amount and each line's amount
// is base * percentage. A line at 0% is filled in by hand at posting
// time. This differs from a Recurring Posting, which auto-generates an
// entry on a schedule — a template is picked manually and never posts
// on its own.
// ═══════════════════════════════════════════════════════════════════
public class PostingTemplate : AuditableEntity
{
    public string  Code        { get; set; } = null!;   // auto: PT-0001
    public string  Description { get; set; } = null!;
    public string? Remarks     { get; set; }
    public bool    IsActive    { get; set; } = true;

    public ICollection<PostingTemplateLine> Lines { get; set; } = new List<PostingTemplateLine>();
}

public class PostingTemplateLine
{
    public int     Id                { get; set; }
    public int     PostingTemplateId { get; set; }
    public int     LineNum           { get; set; }

    public string  AccountCode       { get; set; } = null!;
    public string  AccountName       { get; set; } = "";
    public string  Side              { get; set; } = "D";   // "D" = Debit, "C" = Credit
    public decimal Percentage        { get; set; }          // 0 = amount keyed by hand when the template is used
    public string? LineDescription   { get; set; }

    public PostingTemplate PostingTemplate { get; set; } = null!;
}

public class PostingTemplateConfig : IEntityTypeConfiguration<PostingTemplate>
{
    public void Configure(EntityTypeBuilder<PostingTemplate> b)
    {
        b.ToTable("KBTT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Description).HasMaxLength(200).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Lines).WithOne(l => l.PostingTemplate)
         .HasForeignKey(l => l.PostingTemplateId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PostingTemplateLineConfig : IEntityTypeConfiguration<PostingTemplateLine>
{
    public void Configure(EntityTypeBuilder<PostingTemplateLine> b)
    {
        b.ToTable("BTT1");
        b.HasKey(x => x.Id);
        b.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountName).HasMaxLength(200);
        b.Property(x => x.Side).HasMaxLength(1).IsRequired();
        b.Property(x => x.Percentage).HasPrecision(9, 4);
        b.Property(x => x.LineDescription).HasMaxLength(254);
    }
}
