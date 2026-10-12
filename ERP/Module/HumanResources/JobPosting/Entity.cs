using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.JobPosting;

// ═══════════════════════════════════════════════════════════════
// JOB POSTING  (KJOB)
// An open role recruitment is hiring for. Candidates apply against
// one of these (see the Candidate module).
// ═══════════════════════════════════════════════════════════════
public class JobPosting : AuditableEntity
{
    public string   PostingNo        { get; set; } = null!;  // "JOB-2026-00001"
    public string   Title            { get; set; } = null!;
    public string?  Department       { get; set; }
    public string   EmploymentType   { get; set; } = "Permanent"; // Permanent/Contract/Temporary/Intern
    public string   Status           { get; set; } = "Open";      // Open/OnHold/Closed/Cancelled
    public int      NumberOfOpenings { get; set; } = 1;
    public DateTime OpenDate         { get; set; } = DateTime.UtcNow.Date;
    public DateTime? TargetCloseDate { get; set; }
    public string?  HiringManager    { get; set; }
    public string?  Description      { get; set; }
    public string?  Requirements     { get; set; }
    public string?  Remarks          { get; set; }
}

public class JobPostingConfig : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> b)
    {
        b.ToTable("KJOB");
        b.HasKey(x => x.Id);
        b.Property(x => x.PostingNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.PostingNo).IsUnique();
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Department).HasMaxLength(100);
        b.Property(x => x.EmploymentType).HasMaxLength(20);
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Open");
        b.Property(x => x.HiringManager).HasMaxLength(100);
        b.Property(x => x.Description).HasMaxLength(2000);
        b.Property(x => x.Requirements).HasMaxLength(2000);
        b.Property(x => x.Remarks).HasMaxLength(1000);
        b.HasIndex(x => x.Status);
    }
}
