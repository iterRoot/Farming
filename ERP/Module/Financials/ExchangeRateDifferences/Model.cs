namespace FarmingApi.Modules.Financials.ExchangeRateDifferences;

public class ExchangeRateDifferenceLineResponse
{
    public int     Id                 { get; set; }
    public int     LineNum            { get; set; }
    public string  SourceType         { get; set; } = null!;
    public string  AccountCode        { get; set; } = null!;
    public string? AccountName        { get; set; }
    public decimal ForeignBalance     { get; set; }
    public decimal HistoricalRate     { get; set; }
    public decimal CurrentRate        { get; set; }
    public decimal LocalBalanceBefore { get; set; }
    public decimal LocalBalanceAfter  { get; set; }
    public decimal Difference         { get; set; }
    public string? Remarks            { get; set; }
}

public class ExchangeRateDifferenceResponse
{
    public int      Id            { get; set; }
    public string?  DocNo         { get; set; }
    public string   Status        { get; set; } = null!;
    public DateTime PostingDate   { get; set; }
    public DateTime DocumentDate  { get; set; }
    public string   Currency      { get; set; } = null!;
    public string   BaseCurrency  { get; set; } = null!;
    public decimal  ClosingRate   { get; set; }
    public string?  AccountFrom   { get; set; }
    public string?  AccountTo     { get; set; }
    public decimal  TotalGain     { get; set; }
    public decimal  TotalLoss     { get; set; }
    public decimal  NetDifference { get; set; }
    public string?  Remarks       { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime? UpdatedAt    { get; set; }
    public int      LineCount     { get; set; }
    public List<ExchangeRateDifferenceLineResponse> Lines { get; set; } = new();
}

public class ExchangeRateDifferenceLineRequest
{
    public string  SourceType     { get; set; } = "GL";
    public string  AccountCode    { get; set; } = null!;
    public string? AccountName    { get; set; }
    public decimal ForeignBalance { get; set; }
    public decimal HistoricalRate { get; set; }
    public string? Remarks        { get; set; }
}

public class ExchangeRateDifferenceRequest
{
    public DateTime? PostingDate  { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string    Currency     { get; set; } = null!;
    public string?   BaseCurrency { get; set; }
    public decimal   ClosingRate  { get; set; }
    public string?   AccountFrom  { get; set; }
    public string?   AccountTo    { get; set; }
    public string?   Remarks      { get; set; }
    public List<ExchangeRateDifferenceLineRequest> Lines { get; set; } = new();
}
