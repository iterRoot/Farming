using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.SaleAR.DunningTerm;

// ═══════════════════════════════════════════════════════════════
// DUNNING TERM  (KDNT / DNT1)  ·  SAP B1 equivalent: OCDN/CDN1
// A reusable escalation ladder: each level says how many days past
// due an invoice must be before that level's letter applies. The
// Dunning wizard (see ../Dunning) reads this to pick a level; it
// never invents its own thresholds.
// ═══════════════════════════════════════════════════════════════
public class DunningTerm : AuditableEntity
{
    public string  Code     { get; set; } = null!;
    public string  Name     { get; set; } = null!;
    public bool    IsActive { get; set; } = true;
    public string? Remarks  { get; set; }

    public ICollection<DunningTermLevel> Levels { get; set; } = new List<DunningTermLevel>();
}

public class DunningTermLevel
{
    public int    Id            { get; set; }
    public int    DunningTermId { get; set; }
    public int    Level         { get; set; }          // 1, 2, 3... escalating
    public int    DaysOverdue   { get; set; }           // minimum days past due date to reach this level
    public string LetterText    { get; set; } = null!;  // merged with customer/invoice data at run time

    public DunningTerm DunningTerm { get; set; } = null!;
}

public class DunningTermConfig : IEntityTypeConfiguration<DunningTerm>
{
    public void Configure(EntityTypeBuilder<DunningTerm> b)
    {
        b.ToTable("KDNT");
        b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Remarks).HasMaxLength(500);

        b.HasMany(x => x.Levels).WithOne(l => l.DunningTerm)
         .HasForeignKey(l => l.DunningTermId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DunningTermLevelConfig : IEntityTypeConfiguration<DunningTermLevel>
{
    public void Configure(EntityTypeBuilder<DunningTermLevel> b)
    {
        b.ToTable("DNT1");
        b.HasKey(x => x.Id);
        b.Property(x => x.LetterText).HasMaxLength(4000).IsRequired();
        b.HasIndex(x => new { x.DunningTermId, x.Level }).IsUnique();
    }
}
