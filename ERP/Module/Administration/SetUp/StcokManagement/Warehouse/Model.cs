// namespace FarmingApi.Modules.Financials.BankReconciliation;

// // ═══════════════════════════════════════════════════════════════════
// // RESPONSE
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationResponse
// {
//     public int       Id              { get; set; }
//     public string    ReconcNo        { get; set; } = null!;
//     public string    AccountCode     { get; set; } = null!;
//     public string    AccountName     { get; set; } = null!;
//     public string?   BankName        { get; set; }
//     public string?   BankBranch      { get; set; }
//     public string?   BankAccountNo   { get; set; }
//     public string    Currency        { get; set; } = null!;
//     public DateTime  StatementDate   { get; set; }
//     public DateTime  StartDate       { get; set; }
//     public DateTime  EndDate         { get; set; }
//     public string    Status          { get; set; } = null!;
//     public decimal   GlOpeningBal    { get; set; }
//     public decimal   GlClosingBal    { get; set; }
//     public decimal   BankOpeningBal  { get; set; }
//     public decimal   BankClosingBal  { get; set; }
//     public decimal   ReconciledAmt   { get; set; }
//     public decimal   UnreconciledAmt { get; set; }
//     public decimal   Difference      { get; set; }
//     public string?   Remarks         { get; set; }
//     public int       VersionNum      { get; set; }
//     public DateTime  CreatedAt       { get; set; }
//     public DateTime? UpdatedAt       { get; set; }

//     // Computed
//     public int       TotalLines      { get; set; }
//     public int       ReconciledLines { get; set; }
//     public int       PendingLines    { get; set; }

//     public List<BankReconciliationLineResponse> Lines { get; set; } = new();
// }

// public class BankReconciliationLineResponse
// {
//     public int       Id              { get; set; }
//     public int       LineNum         { get; set; }
//     public string    TransType       { get; set; } = null!;
//     public DateTime  TransDate       { get; set; }
//     public string?   RefNo           { get; set; }
//     public string?   Description     { get; set; }
//     public string?   DocType         { get; set; }
//     public int?      DocEntry        { get; set; }
//     public string?   DocNo           { get; set; }
//     public decimal   Debit           { get; set; }
//     public decimal   Credit          { get; set; }
//     public decimal   Balance         { get; set; }
//     public bool      IsReconciled    { get; set; }
//     public DateTime? ReconciledDate  { get; set; }
//     public string?   ReconciledRef   { get; set; }
//     public string?   Remarks         { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // CREATE REQUEST
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationRequest
// {
//     public string   AccountCode    { get; set; } = null!;
//     public string?  AccountName    { get; set; }
//     public string?  BankName       { get; set; }
//     public string?  BankBranch     { get; set; }
//     public string?  BankAccountNo  { get; set; }
//     public string   Currency       { get; set; } = "USD";
//     public DateTime StatementDate  { get; set; }
//     public DateTime StartDate      { get; set; }
//     public DateTime EndDate        { get; set; }
//     public decimal  BankOpeningBal { get; set; }
//     public decimal  BankClosingBal { get; set; }  // From bank statement
//     public string?  Remarks        { get; set; }
//     public List<BankReconciliationLineRequest> Lines { get; set; } = new();
// }

// // ═══════════════════════════════════════════════════════════════════
// // UPDATE REQUEST
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationUpdateRequest
// {
//     public string?  BankName       { get; set; }
//     public string?  BankBranch     { get; set; }
//     public string?  BankAccountNo  { get; set; }
//     public DateTime StatementDate  { get; set; }
//     public DateTime StartDate      { get; set; }
//     public DateTime EndDate        { get; set; }
//     public decimal  BankOpeningBal { get; set; }
//     public decimal  BankClosingBal { get; set; }
//     public string?  Remarks        { get; set; }
//     public List<BankReconciliationLineRequest> Lines { get; set; } = new();
// }

// // ═══════════════════════════════════════════════════════════════════
// // LINE REQUEST
// // ═══════════════════════════════════════════════════════════════════
// public class BankReconciliationLineRequest
// {
//     public int      LineNum      { get; set; }
//     public string   TransType    { get; set; } = "GL";  // GL or BS
//     public DateTime TransDate    { get; set; }
//     public string?  RefNo        { get; set; }
//     public string?  Description  { get; set; }
//     public string?  DocType      { get; set; }
//     public int?     DocEntry     { get; set; }
//     public string?  DocNo        { get; set; }
//     public decimal  Debit        { get; set; }
//     public decimal  Credit       { get; set; }
//     public bool     IsReconciled { get; set; } = false;
//     public string?  Remarks      { get; set; }
// }

// // ═══════════════════════════════════════════════════════════════════
// // RECONCILE REQUEST — mark specific lines as reconciled
// // ═══════════════════════════════════════════════════════════════════
// public class ReconcileLineRequest
// {
//     public List<int> LineIds       { get; set; } = new();  // Line IDs to reconcile
//     public string?   ReconciledRef { get; set; }           // Reference for matching
// }

// // ═══════════════════════════════════════════════════════════════════
// // LOAD TRANSACTIONS REQUEST — pull from existing payments/JE
// // ═══════════════════════════════════════════════════════════════════
// public class LoadTransactionsRequest
// {
//     public string   AccountCode { get; set; } = null!;
//     public DateTime FromDate    { get; set; }
//     public DateTime ToDate      { get; set; }
// }