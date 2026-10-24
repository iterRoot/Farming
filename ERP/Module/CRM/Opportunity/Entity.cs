using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.Opportunity;

// ═══════════════════════════════════════════════════════════════
// OPPORTUNITY  (KOPP)  ·  SAP B1 equivalent: OOPR / OPR1...
// Sales/Purchasing pipeline tracking with stages, partners,
// competitors, interest range and attachments.
//
// Status: Open / Won / Lost
// ═══════════════════════════════════════════════════════════════
public class Opportunity : AuditableEntity
{
    // ── Header ────────────────────────────────────────────────
    public string   OpportunityNo    { get; set; } = null!;  // "OPP-2026-00001"
    public string   OpportunityName  { get; set; } = null!;
    public string   OpportunityType  { get; set; } = "Sales"; // Sales / Purchasing
    public string   Status           { get; set; } = "Open";  // Open / Won / Lost

    // ── Business Partner ──────────────────────────────────────
    public string?  BPCode           { get; set; }
    public string?  BPName           { get; set; }
    public string?  ContactPerson    { get; set; }
    public decimal  TotalAmountInvoiced { get; set; } = 0;   // read-only / computed
    public string?  BPTerritory      { get; set; }
    public string?  SalesEmployee    { get; set; }
    public string?  Owner            { get; set; }
    public bool     DisplayInSystemCurrency { get; set; } = false;

    // ── Dates / Progress ──────────────────────────────────────
    public DateTime StartDate        { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ClosingDate     { get; set; }
    public int      OpenActivities   { get; set; } = 0;
    public int      ClosingPercent   { get; set; } = 0;     // 0-100

    // ── Potential tab ─────────────────────────────────────────
    public int?     PredictedClosingIn      { get; set; }
    public string?  PredictedClosingUnit    { get; set; } = "Days"; // Days/Weeks/Months
    public DateTime? PredictedClosingDate   { get; set; }
    public decimal  PotentialAmount         { get; set; } = 0;
    public decimal  WeightedAmount          { get; set; } = 0;
    public decimal  GrossProfitPercent      { get; set; } = 0;
    public decimal  GrossProfitTotal        { get; set; } = 0;
    public string?  LevelOfInterest         { get; set; }   // Low/Medium/High

    // ── General tab ───────────────────────────────────────────
    public string?  BPChannelCode    { get; set; }
    public string?  BPChannelName    { get; set; }
    public string?  BPChannelContact { get; set; }
    public string?  BPProject        { get; set; }
    public string?  InformationSource{ get; set; }
    public string?  Industry         { get; set; }
    public string?  Remarks          { get; set; }

    // ── Summary tab ───────────────────────────────────────────
    public string?  DocumentType         { get; set; }
    public string?  DocumentNo           { get; set; }
    public bool     ShowDocsRelatedToBP  { get; set; } = false;

    // Navigation — child grids
    public ICollection<OpportunityInterest>   InterestRanges { get; set; } = new List<OpportunityInterest>();
    public ICollection<OpportunityStage>      Stages         { get; set; } = new List<OpportunityStage>();
    public ICollection<OpportunityPartner>    Partners       { get; set; } = new List<OpportunityPartner>();
    public ICollection<OpportunityCompetitor> Competitors    { get; set; } = new List<OpportunityCompetitor>();
    public ICollection<OpportunityReason>     Reasons        { get; set; } = new List<OpportunityReason>();
    public ICollection<OpportunityAttachment> Attachments    { get; set; } = new List<OpportunityAttachment>();
}

// ── Potential tab → Interest Range grid (KOPI) ─────────────────
public class OpportunityInterest
{
    public int     Id            { get; set; }
    public int     OpportunityId { get; set; }
    public int     LineNum       { get; set; }
    public string? Description   { get; set; }
    public bool    IsPrimary     { get; set; } = false;

    public Opportunity? Opportunity { get; set; }
}

// ── Stages tab grid (KOPS) ──────────────────────────────────────
public class OpportunityStage
{
    public int       Id              { get; set; }
    public int       OpportunityId   { get; set; }
    public int       LineNum         { get; set; }
    public DateTime  StartDate       { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ClosingDate     { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   Stage           { get; set; }   // Lead/Qualification/Proposal/Negotiation/...
    public decimal   Percent         { get; set; } = 0;
    public decimal   PotentialAmount { get; set; } = 0;
    public decimal   WeightedAmount  { get; set; } = 0;
    public bool      ShowBPsDocs     { get; set; } = false;
    public string?   DocumentType    { get; set; }
    public string?   DocNo           { get; set; }
    public string?   Owner           { get; set; }

    public Opportunity? Opportunity { get; set; }
}

// ── Partners tab grid (KOPR) ─────────────────────────────────────
public class OpportunityPartner
{
    public int     Id            { get; set; }
    public int     OpportunityId { get; set; }
    public int     LineNum       { get; set; }
    public string? Name          { get; set; }
    public string? Relationship  { get; set; }
    public string? RelatedBP     { get; set; }
    public string? Remarks       { get; set; }

    public Opportunity? Opportunity { get; set; }
}

// ── Competitors tab grid (KOPC) ──────────────────────────────────
public class OpportunityCompetitor
{
    public int     Id            { get; set; }
    public int     OpportunityId { get; set; }
    public int     LineNum       { get; set; }
    public string? Name          { get; set; }
    public string? ThreatLevel   { get; set; }   // Low/Medium/High
    public string? Remarks       { get; set; }
    public bool    Won           { get; set; } = false;

    public Opportunity? Opportunity { get; set; }
}

// ── Summary tab → Reasons grid (KOPN) ────────────────────────────
public class OpportunityReason
{
    public int     Id            { get; set; }
    public int     OpportunityId { get; set; }
    public int     LineNum       { get; set; }
    public string? Description   { get; set; }

    public Opportunity? Opportunity { get; set; }
}

// ── Attachments tab grid (KOPA) ──────────────────────────────────
public class OpportunityAttachment
{
    public int       Id              { get; set; }
    public int       OpportunityId   { get; set; }
    public int       LineNum         { get; set; }
    public string?   TargetPath      { get; set; }
    public string?   FileName        { get; set; }
    public DateTime? AttachmentDate  { get; set; }
    public string?   FreeText        { get; set; }

    public Opportunity? Opportunity { get; set; }
}

// ─── EF Configurations ───────────────────────────────────────
public class OpportunityConfig : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> b)
    {
        b.ToTable("KOPP");
        b.HasKey(x => x.Id);

        b.Property(x => x.OpportunityNo).HasMaxLength(50).IsRequired();
        b.Property(x => x.OpportunityName).HasMaxLength(200).IsRequired();
        b.Property(x => x.OpportunityType).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(10);

        b.Property(x => x.BPCode).HasMaxLength(50);
        b.Property(x => x.BPName).HasMaxLength(200);
        b.Property(x => x.ContactPerson).HasMaxLength(100);
        b.Property(x => x.TotalAmountInvoiced).HasColumnType("decimal(18,2)");
        b.Property(x => x.BPTerritory).HasMaxLength(100);
        b.Property(x => x.SalesEmployee).HasMaxLength(100);
        b.Property(x => x.Owner).HasMaxLength(100);

        b.Property(x => x.PredictedClosingUnit).HasMaxLength(10);
        b.Property(x => x.PotentialAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.WeightedAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.GrossProfitPercent).HasColumnType("decimal(5,2)");
        b.Property(x => x.GrossProfitTotal).HasColumnType("decimal(18,2)");
        b.Property(x => x.LevelOfInterest).HasMaxLength(20);

        b.Property(x => x.BPChannelCode).HasMaxLength(50);
        b.Property(x => x.BPChannelName).HasMaxLength(200);
        b.Property(x => x.BPChannelContact).HasMaxLength(100);
        b.Property(x => x.BPProject).HasMaxLength(100);
        b.Property(x => x.InformationSource).HasMaxLength(100);
        b.Property(x => x.Industry).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(1000);

        b.Property(x => x.DocumentType).HasMaxLength(50);
        b.Property(x => x.DocumentNo).HasMaxLength(50);

        b.HasIndex(x => x.OpportunityNo).IsUnique();

        b.HasMany(x => x.InterestRanges).WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Stages)        .WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Partners)      .WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Competitors)   .WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Reasons)       .WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Attachments)   .WithOne(x => x.Opportunity).HasForeignKey(x => x.OpportunityId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OpportunityInterestConfig : IEntityTypeConfiguration<OpportunityInterest>
{
    public void Configure(EntityTypeBuilder<OpportunityInterest> b)
    {
        b.ToTable("KOPI");
        b.HasKey(x => x.Id);
        b.Property(x => x.Description).HasMaxLength(500);
    }
}

public class OpportunityStageConfig : IEntityTypeConfiguration<OpportunityStage>
{
    public void Configure(EntityTypeBuilder<OpportunityStage> b)
    {
        b.ToTable("KOPS");
        b.HasKey(x => x.Id);
        b.Property(x => x.SalesEmployee).HasMaxLength(100);
        b.Property(x => x.Stage).HasMaxLength(50);
        b.Property(x => x.Percent).HasColumnType("decimal(5,2)");
        b.Property(x => x.PotentialAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.WeightedAmount).HasColumnType("decimal(18,2)");
        b.Property(x => x.DocumentType).HasMaxLength(50);
        b.Property(x => x.DocNo).HasMaxLength(50);
        b.Property(x => x.Owner).HasMaxLength(100);
    }
}

public class OpportunityPartnerConfig : IEntityTypeConfiguration<OpportunityPartner>
{
    public void Configure(EntityTypeBuilder<OpportunityPartner> b)
    {
        b.ToTable("KOPR");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.Relationship).HasMaxLength(100);
        b.Property(x => x.RelatedBP).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}

public class OpportunityCompetitorConfig : IEntityTypeConfiguration<OpportunityCompetitor>
{
    public void Configure(EntityTypeBuilder<OpportunityCompetitor> b)
    {
        b.ToTable("KOPC");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(200);
        b.Property(x => x.ThreatLevel).HasMaxLength(20);
        b.Property(x => x.Remarks).HasMaxLength(500);
    }
}

public class OpportunityReasonConfig : IEntityTypeConfiguration<OpportunityReason>
{
    public void Configure(EntityTypeBuilder<OpportunityReason> b)
    {
        b.ToTable("KOPN");
        b.HasKey(x => x.Id);
        b.Property(x => x.Description).HasMaxLength(500);
    }
}

public class OpportunityAttachmentConfig : IEntityTypeConfiguration<OpportunityAttachment>
{
    public void Configure(EntityTypeBuilder<OpportunityAttachment> b)
    {
        b.ToTable("KOPA");
        b.HasKey(x => x.Id);
        b.Property(x => x.TargetPath).HasMaxLength(500);
        b.Property(x => x.FileName).HasMaxLength(200);
        b.Property(x => x.FreeText).HasMaxLength(500);
    }
}