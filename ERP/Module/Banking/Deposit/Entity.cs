using FarmingApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FarmingApi.Modules.Banking.Deposit;

// ═══════════════════════════════════════════════════════════════════
// DEPOSIT  (KDEP / DEP1)
// SAP B1 equivalent: Banking > Deposit. Bundles cash/check Incoming
// Payments that were physically taken to the bank into one transfer
// from their holding account (a Cash/Checks-for-collection GL account
// — whatever the payment's own BankAccount field resolved to) into the
// real bank account. Draft has no GL impact; Post is final.
// ═══════════════════════════════════════════════════════════════════
public class Deposit : AuditableEntity
{
    public string   DocNum      { get; set; } = null!;   // DEP-2026-00001
    public DateTime DepositDate { get; set; }
    public string   BankAccount { get; set; } = null!;   // target GL account (real bank)
    public string?  Memo        { get; set; }
    public string   Status      { get; set; } = "Draft"; // Draft | Posted | Cancelled
    public decimal  TotalAmount { get; set; }
    public string   Currency    { get; set; } = "USD";
    public int?     JournalEntryId { get; set; }

    public ICollection<DepositLine> Lines { get; set; } = new List<DepositLine>();
}

public class DepositLine
{
    public int    Id               { get; set; }
    public int    DepositId        { get; set; }
    public int    LineNum          { get; set; }
    public int    IncomingPaymentId{ get; set; }
    public string PaymentDocNum    { get; set; } = null!;  // display snapshot
    public string CardName         { get; set; } = null!;
    public string HoldingAccount   { get; set; } = null!;  // where the payment's own JE debited
    public decimal Amount          { get; set; }

    public Deposit Deposit { get; set; } = null!;
}

public class DepositConfig : IEntityTypeConfiguration<Deposit>
{
    public void Configure(EntityTypeBuilder<Deposit> b)
    {
        b.ToTable("KDEP");
        b.HasKey(x => x.Id);
        b.Property(x => x.DocNum).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.DocNum).IsUnique();
        b.Property(x => x.BankAccount).HasMaxLength(50).IsRequired();
        b.Property(x => x.Status).HasMaxLength(15).HasDefaultValue("Draft");
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("USD");
        b.Property(x => x.Memo).HasMaxLength(500);

        b.HasMany(x => x.Lines).WithOne(l => l.Deposit)
         .HasForeignKey(l => l.DepositId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class DepositLineConfig : IEntityTypeConfiguration<DepositLine>
{
    public void Configure(EntityTypeBuilder<DepositLine> b)
    {
        b.ToTable("DEP1");
        b.HasKey(x => x.Id);
        b.Property(x => x.PaymentDocNum).HasMaxLength(50).IsRequired();
        b.Property(x => x.CardName).HasMaxLength(200);
        b.Property(x => x.HoldingAccount).HasMaxLength(50).IsRequired();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.HasIndex(x => x.IncomingPaymentId);
    }
}
