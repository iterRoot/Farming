namespace FarmingApi.Modules.Financials.IternalReconciliations;

// ── Response ─────────────────────────────────────────────────────────────
public class BPReconciliationResponse
{
    public int       Id              { get; set; }
    public string    ReconcNo        { get; set; } = null!;
    public int       PartnerId       { get; set; }
    public string    PartnerCode     { get; set; } = null!;
    public string    PartnerName     { get; set; } = null!;
    public string    Currency        { get; set; } = null!;
    public DateTime  StartDate       { get; set; }
    public DateTime  EndDate         { get; set; }
    public string    Status          { get; set; } = null!;
    public decimal   OpeningBalance  { get; set; }
    public decimal   ClosingBalance  { get; set; }
    public decimal   ReconciledAmt   { get; set; }
    public decimal   UnreconciledAmt { get; set; }
    public string?   Remarks         { get; set; }
    public int       VersionNum      { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public DateTime? UpdatedAt       { get; set; }

    public int       TotalLines      { get; set; }
    public int       ReconciledLines { get; set; }
    public int       PendingLines    { get; set; }

    public List<BPReconciliationLineResponse> Lines { get; set; } = new();
}

public class BPReconciliationLineResponse
{
    public int       Id             { get; set; }
    public int       LineNum        { get; set; }
    public DateTime  TransDate      { get; set; }
    public string?   RefNo          { get; set; }
    public string?   Description    { get; set; }
    public string?   DocType        { get; set; }
    public int?      DocEntry       { get; set; }
    public string?   DocNo          { get; set; }
    public decimal   Debit          { get; set; }
    public decimal   Credit         { get; set; }
    public decimal   Balance        { get; set; }
    public bool      IsReconciled   { get; set; }
    public DateTime? ReconciledDate { get; set; }
    public string?   ReconciledRef  { get; set; }
    public string?   Remarks        { get; set; }
}

// ── Requests ─────────────────────────────────────────────────────────────
public class BPReconciliationRequest
{
    public int      PartnerId      { get; set; }
    public string   Currency       { get; set; } = "USD";
    public DateTime StartDate      { get; set; }
    public DateTime EndDate        { get; set; }
    public decimal  OpeningBalance { get; set; }
    public string?  Remarks        { get; set; }
    public List<BPReconciliationLineRequest> Lines { get; set; } = new();
}

public class BPReconciliationUpdateRequest
{
    public DateTime StartDate      { get; set; }
    public DateTime EndDate        { get; set; }
    public decimal  OpeningBalance { get; set; }
    public string?  Remarks        { get; set; }
    public List<BPReconciliationLineRequest> Lines { get; set; } = new();
}

public class BPReconciliationLineRequest
{
    public int      LineNum      { get; set; }
    public DateTime TransDate    { get; set; }
    public string?  RefNo        { get; set; }
    public string?  Description  { get; set; }
    public string?  DocType      { get; set; }
    public int?     DocEntry     { get; set; }
    public string?  DocNo        { get; set; }
    public decimal  Debit        { get; set; }
    public decimal  Credit       { get; set; }
    public bool     IsReconciled { get; set; } = false;
    public string?  Remarks      { get; set; }
}

public class ReconcileBPLineRequest
{
    public List<int> LineIds       { get; set; } = new();
    public string?    ReconciledRef { get; set; }
}

public class LoadBPTransactionsRequest
{
    public int      PartnerId { get; set; }
    public DateTime FromDate  { get; set; }
    public DateTime ToDate    { get; set; }
}
