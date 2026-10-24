using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.LeaveRequest;

// ═══════════════════════════════════════════════════════════════
// LEAVE REQUEST  (KLVR)
//
// A request is a plan; attendance is the record. Approving a request
// writes the Leave days into Attendance, which is what Payroll reads
// — so approving unpaid leave produces the deduction automatically
// and nobody has to key the same days twice.
// ═══════════════════════════════════════════════════════════════
public class LeaveRequest : AuditableEntity
{
    public string   RequestNo    { get; set; } = null!;   // LVR-2026-00001
    public string   EmployeeCode { get; set; } = null!;
    public string?  EmployeeName { get; set; }

    public string   LeaveType    { get; set; } = "Annual"; // Annual/Sick/Unpaid/Maternity/Other
    public DateTime FromDate     { get; set; }
    public DateTime ToDate       { get; set; }
    /// <summary>Working days in the range — Sundays excluded. Halved for a half-day request.</summary>
    public decimal  Days         { get; set; }
    public bool     HalfDay      { get; set; }
    public string?  Reason       { get; set; }

    public string   Status       { get; set; } = "Pending"; // Pending/Approved/Rejected/Cancelled
    public string?  DecidedBy    { get; set; }
    public DateTime? DecidedAt   { get; set; }
    public string?  DecisionNote { get; set; }
}

public class LeaveRequestConfig : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> b)
    {
        b.ToTable("KLVR");
        b.HasKey(x => x.Id);

        b.Property(x => x.RequestNo).HasMaxLength(30).IsRequired();
        b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.EmployeeName).HasMaxLength(200);
        b.Property(x => x.LeaveType).HasMaxLength(20).IsRequired();
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.Property(x => x.DecidedBy).HasMaxLength(100);
        b.Property(x => x.DecisionNote).HasMaxLength(1000);
        b.Property(x => x.Days).HasColumnType("decimal(9,2)");

        b.HasIndex(x => x.RequestNo).IsUnique();
        b.HasIndex(x => new { x.EmployeeCode, x.FromDate });
    }
}

public static class LeaveMath
{
    public static readonly string[] Types =
        { "Annual", "Sick", "Unpaid", "Maternity", "Other" };

    public static readonly string[] Statuses =
        { "Pending", "Approved", "Rejected", "Cancelled" };

    /// <summary>Cambodian statutory annual leave: 1.5 days per month worked.</summary>
    public const decimal AnnualEntitlementDays = 18m;

    /// <summary>
    /// Working days between two dates, inclusive. Sundays are treated as
    /// non-working, which matches the payroll divisor of 26 days a month
    /// (a six-day week). Change both together if the work week changes.
    /// </summary>
    public static List<DateTime> WorkingDays(DateTime from, DateTime to)
    {
        var days = new List<DateTime>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Sunday)
                days.Add(d);
        return days;
    }

    public static bool IsPaid(string leaveType) =>
        !string.Equals(leaveType, "Unpaid", StringComparison.OrdinalIgnoreCase);
}
