namespace FarmingApi.Modules.Financials.EditChartOfAccounts;

public class ChartOfAccountsEditLineResponse
{
    public int     Id         { get; set; }
    public int     LineNum    { get; set; }
    public string  AcctCode   { get; set; } = null!;
    public string? AcctName   { get; set; }
    public string  ChangeType { get; set; } = null!;
    public string? NewValue   { get; set; }
    public string? OldValue   { get; set; }
    public bool    Applied    { get; set; }
    public string? Note       { get; set; }
}

public class ChartOfAccountsEditBatchResponse
{
    public int       Id          { get; set; }
    public string?   DocNo       { get; set; }
    public string    Status      { get; set; } = null!;
    public DateTime  BatchDate   { get; set; }
    public string?   Description { get; set; }
    public DateTime? AppliedAt   { get; set; }
    public string?   AppliedBy   { get; set; }
    public string?   Remarks     { get; set; }
    public DateTime  CreatedAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public int       LineCount   { get; set; }
    public List<ChartOfAccountsEditLineResponse> Lines { get; set; } = new();
}

public class ChartOfAccountsEditLineRequest
{
    public string  AcctCode   { get; set; } = null!;
    public string  ChangeType { get; set; } = null!;
    public string? NewValue   { get; set; }
    public string? Note       { get; set; }
}

public class ChartOfAccountsEditBatchRequest
{
    public DateTime? BatchDate   { get; set; }
    public string?   Description { get; set; }
    public string?   Remarks     { get; set; }
    public List<ChartOfAccountsEditLineRequest> Lines { get; set; } = new();
}

/// <summary>Body for POST /EditChartOfAccounts/{id}/Apply.</summary>
public class ChartOfAccountsEditApplyRequest
{
    public string? AppliedBy { get; set; }
}

/// <summary>
/// One row of the account picker the edit screen browses. A trimmed
/// projection of OACT — enough to choose an account and see where it sits.
/// </summary>
public class ChartOfAccountsEditCandidate
{
    public string  AcctCode  { get; set; } = null!;
    public string  AcctName  { get; set; } = null!;
    public int     Level     { get; set; }
    public string? FatherNum { get; set; }
    public string  AcctType  { get; set; } = null!;
    public bool    IsGroup   { get; set; }
    public bool    IsActive  { get; set; }
    public bool    AllowPosting { get; set; }
}
