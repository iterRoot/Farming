using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

// ═══════════════════════════════════════════════════════════════
// SALES EMPLOYEE / BUYER  (KSEB)
// SAP B1 equivalent: Sales Employees / Buyers
//
// A single entity covers both roles:
//   SalesEmployee → assigned to AR invoices, quotations, orders
//   Buyer         → assigned to AP invoices, purchase orders
//   Both          → acts in both capacities
//
// Linked to:
//   • CommissionGroup (KCMG) for commission calculation
//   • Sales/Purchase documents for reporting
//   • User account (UserCode) for login mapping
// ═══════════════════════════════════════════════════════════════
public class SaleEmployeeBuyer : AuditableEntity
{
    // ── Identity ──────────────────────────────────────────────
    public string   Code          { get; set; } = null!;  // e.g. "SE-001"
    public string   FirstName     { get; set; } = null!;
    public string   LastName      { get; set; } = null!;
    public string   EmployeeType  { get; set; } = "Both"; // SalesEmployee / Buyer / Both

    // ── Contact ───────────────────────────────────────────────
    public string?  Mobile        { get; set; }
    public string?  Phone         { get; set; }
    public string?  Fax           { get; set; }
    public string?  Email         { get; set; }

    // ── Organisation ──────────────────────────────────────────
    public string?  Department    { get; set; }
    public string?  Position      { get; set; }
    public string?  SalesRegion   { get; set; }
    public string?  Territory     { get; set; }
    public int?     BranchId      { get; set; }

    // ── Commission ────────────────────────────────────────────
    public int?     CommissionGroupId   { get; set; }
    public string?  CommissionGroupCode { get; set; }
    public string?  CommissionGroupName { get; set; }

    // ── System Link ───────────────────────────────────────────
    public string?  UserCode      { get; set; }  // linked user default
    public string?  ExternalId    { get; set; }  // HR system reference

    // ── Status ────────────────────────────────────────────────
    public bool     IsActive      { get; set; } = true;
    public string?  Remarks       { get; set; }
}

public class SaleEmployeeBuyerConfig
    : IEntityTypeConfiguration<SaleEmployeeBuyer>
{
    public void Configure(EntityTypeBuilder<SaleEmployeeBuyer> b)
    {
        b.ToTable("KSEB");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        b.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        b.Property(x => x.EmployeeType).HasMaxLength(20);
        b.Property(x => x.Mobile).HasMaxLength(50);
        b.Property(x => x.Phone).HasMaxLength(50);
        b.Property(x => x.Fax).HasMaxLength(50);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Department).HasMaxLength(100);
        b.Property(x => x.Position).HasMaxLength(100);
        b.Property(x => x.SalesRegion).HasMaxLength(100);
        b.Property(x => x.Territory).HasMaxLength(100);
        b.Property(x => x.CommissionGroupCode).HasMaxLength(50);
        b.Property(x => x.CommissionGroupName).HasMaxLength(200);
        b.Property(x => x.UserCode).HasMaxLength(50);
        b.Property(x => x.ExternalId).HasMaxLength(100);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => x.Code).IsUnique();
    }
}