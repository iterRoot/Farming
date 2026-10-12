using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.CRM.Campaign;

// ═══════════════════════════════════════════════════════════════
// CAMPAIGN  (KCMP / CMP1)  ·  SAP B1 equivalent: OCPR / CPR1
// A marketing campaign with a target list of business partners and
// real per-target response tracking — not just a name and a budget.
// ═══════════════════════════════════════════════════════════════
public class Campaign : AuditableEntity
{
    public string   CampaignNo      { get; set; } = null!;  // "CAMP-2026-00001"
    public string   CampaignName    { get; set; } = null!;
    public string   CampaignType    { get; set; } = "Email"; // Email/Event/Print/Social/Phone/Other
    public string   Status          { get; set; } = "Planned"; // Planned/Active/Completed/Cancelled
    public DateTime StartDate       { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EndDate        { get; set; }
    public decimal  Budget          { get; set; }
    public decimal  ActualCost      { get; set; }
    public decimal  ExpectedRevenue { get; set; }
    public string?  Owner           { get; set; }
    public string?  Remarks         { get; set; }

    public ICollection<CampaignTarget> Targets { get; set; } = new List<CampaignTarget>();
}

// One targeted business partner and whether the campaign landed.
public class CampaignTarget : AuditableEntity
{
    public int      CampaignId    { get; set; }
    public string   BPCode        { get; set; } = null!;
    public string   BPName        { get; set; } = null!;
    public string   Status        { get; set; } = "NotContacted"; // NotContacted/Contacted/Responded/Converted/Declined
    public DateTime? ResponseDate { get; set; }
    public string?  ResponseNotes { get; set; }

    public Campaign Campaign { get; set; } = null!;
}

public class CampaignConfig : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> b)
    {
        b.ToTable("KCMP");
        b.HasKey(x => x.Id);
        b.Property(x => x.CampaignNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.CampaignNo).IsUnique();
        b.Property(x => x.CampaignName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CampaignType).HasMaxLength(30);
        b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("Planned");
        b.Property(x => x.Budget).HasPrecision(18, 2);
        b.Property(x => x.ActualCost).HasPrecision(18, 2);
        b.Property(x => x.ExpectedRevenue).HasPrecision(18, 2);
        b.Property(x => x.Owner).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.HasIndex(x => x.Status);

        b.HasMany(x => x.Targets).WithOne(t => t.Campaign)
         .HasForeignKey(t => t.CampaignId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CampaignTargetConfig : IEntityTypeConfiguration<CampaignTarget>
{
    public void Configure(EntityTypeBuilder<CampaignTarget> b)
    {
        b.ToTable("CMP1");
        b.HasKey(x => x.Id);
        b.Property(x => x.BPCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.BPName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(20).HasDefaultValue("NotContacted");
        b.Property(x => x.ResponseNotes).HasMaxLength(500);
        b.HasIndex(x => new { x.CampaignId, x.BPCode }).IsUnique();
    }
}
