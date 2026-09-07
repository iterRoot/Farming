using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FinancialProjects;

// SAP B1 "Projects - Setup" — a financial project code used to tag journal
// entries and documents for project-level reporting. `InActive` (from
// AuditableEntity) is the grid's "Active" flag (inverted).
public class FinancialProject : AuditableEntity
{
    public string   ProjectCode { get; set; } = null!;
    public string?  ProjectName { get; set; }
    public DateOnly? ValidFrom  { get; set; }
    public DateOnly? ValidTo    { get; set; }
}

public class FinancialProjectConfig : IEntityTypeConfiguration<FinancialProject>
{
    public void Configure(EntityTypeBuilder<FinancialProject> b)
    {
        b.ToTable("KPRJ");
        b.HasKey(x => x.Id);
        b.Property(x => x.ProjectCode).HasMaxLength(20).IsRequired();
        b.Property(x => x.ProjectName).HasMaxLength(200);
        b.HasIndex(x => x.ProjectCode).IsUnique();
    }
}
