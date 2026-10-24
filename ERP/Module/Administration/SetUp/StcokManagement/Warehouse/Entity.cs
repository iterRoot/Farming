// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.Financials.BankReconciliation;

// // ═══════════════════════════════════════════════════════════════════
// // BANK RECONCILIATION HEADER (KBNR)
// //
// // PURPOSE: Match your GL (book) balance against bank statement balance
// // to find: Outstanding cheques, deposits in transit, bank fees, errors
// //
// // Flow:
// //   1. Get bank statement from your bank
// //   2. Get GL balance from Chart of Accounts
// //   3. Match each transaction → mark as Reconciled
// //   4. Difference should = 0 when complete
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliation : AuditableEntity
// {
//     public string   ReconcNo       { get; set; } = null!;  // Auto: BR-2026-00001
//     public string   AccountCode    { get; set; } = null!;  // GL bank/cash account
//     public string   AccountName    { get; set; } = string.Empty;
//     public string?  BankName       { get; set; }
//     public string?  BankBranch     { get; set; }
//     public string?  BankAccountNo  { get; set; }           // Actual bank account number
//     public string   Currency       { get; set; } = "USD";
//     public DateTime StatementDate  { get; set; }           // Bank statement date
//     public DateTime StartDate      { get; set; }           // Reconciliation period start
//     public DateTime EndDate        { get; set; }           // Reconciliation period end
//     public string   Status         { get; set; } = "O";   // O=Open, C=Closed/Reconciled

//     // GL (Book) side — from Chart of Accounts
//     public decimal  GlOpeningBal   { get; set; }           // GL balance at start
//     public decimal  GlClosingBal   { get; set; }           // GL balance at end

//     // Bank Statement side — from bank
//     public decimal  BankOpeningBal { get; set; }           // Bank statement opening
//     public decimal  BankClosingBal { get; set; }           // Bank statement closing (user enters)

//     // Reconciled amounts
//     public decimal  ReconciledAmt  { get; set; }           // Total matched
//     public decimal  UnreconciledAmt{ get; set; }           // Still unmatched

//     // Final difference (should be 0 when complete)
//     public decimal  Difference     { get; set; }

//     public string?  Remarks        { get; set; }
//     public int      VersionNum     { get; set; } = 1;
//     public int?     UserSign       { get; set; }

//     public ICollection<BankReconciliationLine> Lines { get; set; } = new List<BankReconciliationLine>();
// }

// // ═══════════════════════════════════════════════════════════════════
// // BANK RECONCILIATION LINE (BNR1)
// // Each line = one transaction being matched
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationLine : AuditableEntity
// {
//     public int      BankReconciliationId { get; set; }
//     public int      LineNum              { get; set; }

//     // Transaction details
//     public string   TransType            { get; set; } = "GL"; // GL=from books, BS=bank statement
//     public DateTime TransDate            { get; set; }
//     public string?  RefNo                { get; set; }         // Doc reference
//     public string?  Description          { get; set; }

//     // Source document link (optional)
//     public string?  DocType              { get; set; }   // KPYI, KPYO, KJRN etc.
//     public int?     DocEntry             { get; set; }
//     public string?  DocNo                { get; set; }   // IP-2026-00001, OP-2026-00001

//     // Amounts
//     public decimal  Debit                { get; set; }   // Money IN
//     public decimal  Credit               { get; set; }   // Money OUT
//     public decimal  Balance              { get; set; }   // Running balance

//     // Reconciliation status
//     public bool     IsReconciled         { get; set; } = false;
//     public DateTime? ReconciledDate      { get; set; }
//     public string?  ReconciledRef        { get; set; }  // Matched to which bank line

//     public string?  Remarks              { get; set; }

//     public BankReconciliation BankReconciliation { get; set; } = null!;
// }

// // ═══════════════════════════════════════════════════════════════════
// // EF CONFIGURATIONS
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationConfig : IEntityTypeConfiguration<BankReconciliation>
// {
//     public void Configure(EntityTypeBuilder<BankReconciliation> builder)
//     {
//         builder.ToTable("KBNR");
//         builder.HasKey(x => x.Id);

//         builder.Property(m => m.ReconcNo).HasMaxLength(50).IsRequired();
//         builder.HasIndex(m => m.ReconcNo).IsUnique();
//         builder.Property(m => m.AccountCode).HasMaxLength(20).IsRequired();
//         builder.Property(m => m.AccountName).HasMaxLength(200).HasDefaultValue(string.Empty);
//         builder.Property(m => m.BankName).HasMaxLength(200);
//         builder.Property(m => m.BankBranch).HasMaxLength(200);
//         builder.Property(m => m.BankAccountNo).HasMaxLength(50);
//         builder.Property(m => m.Currency).HasMaxLength(3).HasDefaultValue("USD");
//         builder.Property(m => m.Status).HasMaxLength(1).HasDefaultValue("O");
//         builder.Property(m => m.Remarks).HasMaxLength(500);
//         builder.Property(m => m.VersionNum).HasDefaultValue(1);

//         builder.Property(m => m.GlOpeningBal).HasPrecision(18, 2);
//         builder.Property(m => m.GlClosingBal).HasPrecision(18, 2);
//         builder.Property(m => m.BankOpeningBal).HasPrecision(18, 2);
//         builder.Property(m => m.BankClosingBal).HasPrecision(18, 2);
//         builder.Property(m => m.ReconciledAmt).HasPrecision(18, 2);
//         builder.Property(m => m.UnreconciledAmt).HasPrecision(18, 2);
//         builder.Property(m => m.Difference).HasPrecision(18, 2);

//         builder.HasIndex(m => m.AccountCode);
//         builder.HasIndex(m => m.StatementDate);
//         builder.HasIndex(m => m.Status);
//     }
// }

// public class BankReconciliationLineConfig : IEntityTypeConfiguration<BankReconciliationLine>
// {
//     public void Configure(EntityTypeBuilder<BankReconciliationLine> builder)
//     {
//         builder.ToTable("BNR1");
//         builder.HasKey(x => x.Id);

//         builder.Property(m => m.TransType).HasMaxLength(5).HasDefaultValue("GL");
//         builder.Property(m => m.RefNo).HasMaxLength(100);
//         builder.Property(m => m.Description).HasMaxLength(500);
//         builder.Property(m => m.DocType).HasMaxLength(10);
//         builder.Property(m => m.DocNo).HasMaxLength(50);
//         builder.Property(m => m.ReconciledRef).HasMaxLength(100);
//         builder.Property(m => m.Remarks).HasMaxLength(200);

//         builder.Property(m => m.Debit).HasPrecision(18, 2);
//         builder.Property(m => m.Credit).HasPrecision(18, 2);
//         builder.Property(m => m.Balance).HasPrecision(18, 2);

//         builder.HasOne(m => m.BankReconciliation)
//             .WithMany(b => b.Lines)
//             .HasForeignKey(m => m.BankReconciliationId)
//             .OnDelete(DeleteBehavior.Cascade);

//         builder.HasIndex(m => m.BankReconciliationId);
//         builder.HasIndex(m => m.IsReconciled);
//         builder.HasIndex(m => new { m.DocType, m.DocEntry });
//     }
// }