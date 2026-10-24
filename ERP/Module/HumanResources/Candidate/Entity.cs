using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.Candidate;

// ═══════════════════════════════════════════════════════════════
// CANDIDATE  (KCND)
// One applicant against a Job Posting. Applied -> Screening ->
// Interviewing -> Offered -> Hired/Rejected. Hiring creates a real
// Employee Master record (EmployeeId links to it; no FK — the same
// no-Restrict-delete lesson this codebase already learned three
// times over on Landed Costs, Deposit and Production Order).
// ═══════════════════════════════════════════════════════════════
public class Candidate : AuditableEntity
{
    public string   CandidateNo     { get; set; } = null!;  // "CAND-2026-00001"
    public int      JobPostingId    { get; set; }
    public string   FirstName       { get; set; } = null!;
    public string   LastName        { get; set; } = null!;
    public string?  Email           { get; set; }
    public string?  Phone           { get; set; }
    public string   Stage           { get; set; } = "Applied"; // Applied/Screening/Interviewing/Offered/Hired/Rejected
    public DateTime AppliedDate     { get; set; } = DateTime.UtcNow;
    public decimal? ExpectedSalary  { get; set; }
    public string?  Notes           { get; set; }
    public string?  RejectionReason { get; set; }
    public int?     EmployeeId      { get; set; }             // set once Hired
    public string?  EmployeeCode    { get; set; }              // display snapshot
}

public class CandidateConfig : IEntityTypeConfiguration<Candidate>
{
    public void Configure(EntityTypeBuilder<Candidate> b)
    {
        b.ToTable("KCND");
        b.HasKey(x => x.Id);
        b.Property(x => x.CandidateNo).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.CandidateNo).IsUnique();
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Stage).HasMaxLength(15).HasDefaultValue("Applied");
        b.Property(x => x.ExpectedSalary).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.EmployeeCode).HasMaxLength(50);
        b.HasIndex(x => x.JobPostingId);
        b.HasIndex(x => x.Stage);
    }
}
