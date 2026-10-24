using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.EmployeeMaster;

// ═══════════════════════════════════════════════════════════════
// EMPLOYEE MASTER  (KEMP)
// SAP B1 equivalent: OHEM — the HR register of people employed by
// the company. Distinct from Sales Employees / Buyers (KSEB), which
// only exists to stamp sales & purchasing documents.
// ═══════════════════════════════════════════════════════════════
public class EmployeeMaster : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string  Code         { get; set; } = null!;   // e.g. "EMP-001"
    public string  FirstName    { get; set; } = null!;
    public string  LastName     { get; set; } = null!;
    public string  EmployeeType { get; set; } = "Permanent"; // Permanent / Contract / Temporary / Intern

    // ── Organisation ──────────────────────────────────────────
    public string? JobTitle     { get; set; }
    public string? Department    { get; set; }
    public string? Branch        { get; set; }
    public string? Manager       { get; set; }   // manager's employee code or name

    // ── Personal ──────────────────────────────────────────────
    public string?   Gender        { get; set; }  // Male / Female / Other
    public DateTime? DateOfBirth   { get; set; }
    public string?   Nationality   { get; set; }
    public string?   MaritalStatus { get; set; }
    public string?   NationalId    { get; set; }

    /// <summary>Non-working spouse + minor children — 150,000 KHR TOS relief each.</summary>
    public int       Dependents    { get; set; }

    // ── Contact ───────────────────────────────────────────────
    public string? Mobile   { get; set; }
    public string? Phone    { get; set; }
    public string? Email    { get; set; }
    public string? Address  { get; set; }

    // ── Employment ────────────────────────────────────────────
    public DateTime? HireDate        { get; set; }
    public DateTime? TerminationDate { get; set; }
    public decimal?  Salary          { get; set; }
    public string?   BankName        { get; set; }
    public string?   BankAccountNo   { get; set; }

    // ── System link ───────────────────────────────────────────
    public string? UserCode   { get; set; }  // linked application login
    public string? ExternalId { get; set; }  // external HR / payroll reference

    // ── Status ────────────────────────────────────────────────
    public bool    IsActive { get; set; } = true;
    public string? Remarks  { get; set; }
}

public class EmployeeMasterConfig : IEntityTypeConfiguration<EmployeeMaster>
{
    public void Configure(EntityTypeBuilder<EmployeeMaster> b)
    {
        b.ToTable("KEMP");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.EmployeeType).HasMaxLength(20);

        b.Property(x => x.JobTitle).HasMaxLength(100);
        b.Property(x => x.Department).HasMaxLength(100);
        b.Property(x => x.Branch).HasMaxLength(100);
        b.Property(x => x.Manager).HasMaxLength(100);

        b.Property(x => x.Gender).HasMaxLength(10);
        b.Property(x => x.Nationality).HasMaxLength(100);
        b.Property(x => x.MaritalStatus).HasMaxLength(20);
        b.Property(x => x.NationalId).HasMaxLength(50);

        b.Property(x => x.Mobile).HasMaxLength(50);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(300);

        b.Property(x => x.Salary).HasColumnType("decimal(18,2)");
        b.Property(x => x.BankName).HasMaxLength(150);
        b.Property(x => x.BankAccountNo).HasMaxLength(50);

        b.Property(x => x.UserCode).HasMaxLength(50);
        b.Property(x => x.ExternalId).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
    }
}
