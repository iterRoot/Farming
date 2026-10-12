using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.PurchaseAP.LandedCostType;

// The Landed Cost document (Modules.PurchaseAP.LandingCost.LandedCostLine)
// stores CostType as free text, with only a code-comment listing the five
// values it expects (Freight | CustomsDuty | Insurance | Handling | Other).
// This is the master list an admin curates under Administration › Set Up ›
// Purchasing › Landed Costs, so a company can add its own cost types
// instead of being limited to that hardcoded set.
public class LandedCostType : AuditableEntity
{
    public string  Name   { get; set; } = null!;   // e.g. "Freight" — matches LandedCostLine.CostType
    public string? Remarks { get; set; }
    public bool    Active { get; set; } = true;
}

public class LandedCostTypeConfig : IEntityTypeConfiguration<LandedCostType>
{
    public void Configure(EntityTypeBuilder<LandedCostType> b)
    {
        b.ToTable("KLCTYP");
        b.HasKey(x => x.Id);
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(300);
        b.HasIndex(x => x.Name).IsUnique();
    }
}
