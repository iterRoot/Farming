namespace FarmingApi.Modules.Financials.FixedAssets.DepreciationRun;

public class DepreciationPeriodRequest
{
    public int PeriodYear  { get; set; }
    public int PeriodMonth { get; set; }
}

public class DepreciationRunCreateRequest
{
    public int      PeriodYear  { get; set; }
    public int      PeriodMonth { get; set; }
    public DateTime PostingDate { get; set; }   // required, so the posting-period lock always sees it
    public string?  Remarks     { get; set; }
}

public class DepreciationPreviewLine
{
    public int     AssetId             { get; set; }
    public string  ItemNo              { get; set; } = null!;
    public string  Description         { get; set; } = null!;
    public string  Method              { get; set; } = null!;
    public decimal AcquisitionCost     { get; set; }
    public decimal SalvageValue        { get; set; }
    public decimal AccumulatedBefore   { get; set; }
    public decimal Amount              { get; set; }
    public decimal AccumulatedAfter    { get; set; }
    public decimal NetBookValueAfter   { get; set; }
    public int     RemainingLifeBefore { get; set; }
    public int     RemainingLifeAfter  { get; set; }
    public string  ExpenseAccount      { get; set; } = null!;
    public string  AccumulatedAccount  { get; set; } = null!;
}

public class DepreciationSkippedAsset
{
    public string ItemNo      { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Reason      { get; set; } = null!;
}

public class DepreciationPreviewResponse
{
    public int     PeriodYear  { get; set; }
    public int     PeriodMonth { get; set; }
    public string? BlockedReason { get; set; }   // set when this period can't be posted at all
    public decimal TotalAmount { get; set; }
    public List<DepreciationPreviewLine>  Lines   { get; set; } = new();
    public List<DepreciationSkippedAsset> Skipped { get; set; } = new();
}

public class DepreciationRunLineResponse
{
    public int     AssetId             { get; set; }
    public string  ItemNo              { get; set; } = null!;
    public string  Description         { get; set; } = null!;
    public string  Method              { get; set; } = null!;
    public decimal Amount              { get; set; }
    public decimal AccumulatedBefore   { get; set; }
    public decimal AccumulatedAfter    { get; set; }
    public int     RemainingLifeBefore { get; set; }
    public int     RemainingLifeAfter  { get; set; }
    public string  ExpenseAccount      { get; set; } = null!;
    public string  AccumulatedAccount  { get; set; } = null!;
}

public class DepreciationRunResponse
{
    public int       Id             { get; set; }
    public string    RunNo          { get; set; } = null!;
    public int       PeriodYear     { get; set; }
    public int       PeriodMonth    { get; set; }
    public DateTime  PostingDate    { get; set; }
    public string    Status         { get; set; } = null!;
    public decimal   TotalAmount    { get; set; }
    public int?      JournalEntryId { get; set; }
    public string?   Remarks        { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public List<DepreciationRunLineResponse> Lines { get; set; } = new();
}
