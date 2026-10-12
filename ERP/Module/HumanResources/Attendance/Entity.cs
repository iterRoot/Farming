using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.HumanResources.Attendance;

// ═══════════════════════════════════════════════════════════════
// ATTENDANCE  (KATT)
// One row per employee per day. EmployeeCode points at
// EmployeeMaster.Code; EmployeeName is a display snapshot.
// ═══════════════════════════════════════════════════════════════
public class Attendance : AuditableEntity
{
    public string   EmployeeCode  { get; set; } = null!;
    public string?  EmployeeName  { get; set; }
    public DateTime Date          { get; set; }
    public string   Status        { get; set; } = "Present"; // Present / Absent / Late / HalfDay / Leave / Holiday

    public string?  CheckIn       { get; set; }   // "08:00"
    public string?  CheckOut      { get; set; }   // "17:00"
    public decimal? HoursWorked   { get; set; }
    public decimal? OvertimeHours { get; set; }

    public string?  LeaveType     { get; set; }   // Annual / Sick / Unpaid / Other
    public string?  Remarks       { get; set; }
}

public class AttendanceConfig : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> b)
    {
        b.ToTable("KATT");
        b.HasKey(x => x.Id);

        b.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired();
        b.Property(x => x.EmployeeName).HasMaxLength(200);
        b.Property(x => x.Status).HasMaxLength(20).IsRequired();
        b.Property(x => x.CheckIn).HasMaxLength(10);
        b.Property(x => x.CheckOut).HasMaxLength(10);
        b.Property(x => x.HoursWorked).HasColumnType("decimal(9,2)");
        b.Property(x => x.OvertimeHours).HasColumnType("decimal(9,2)");
        b.Property(x => x.LeaveType).HasMaxLength(30);
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasIndex(x => new { x.EmployeeCode, x.Date }).IsUnique();
    }
}
