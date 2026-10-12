namespace FarmingApi.Modules.Financials.ConversionDifferences;

public class ConversionDifferenceLineResponse
{
    public int     Id                  { get; set; }
    public int     LineNum             { get; set; }
    public string  SourceType          { get; set; } = null!;
    public string  AccountCode         { get; set; } = null!;
    public string? AccountName         { get; set; }
    public decimal LocalBalance        { get; set; }
    public decimal HistoricalRate      { get; set; }
    public decimal CurrentRate         { get; set; }
    public decimal SystemBalanceBefore { get; set; }
    public decimal SystemBalanceAfter  { get; set; }
    public decimal Difference          { get; set; }
    public string? Remarks             { get; set; }
}

public class ConversionDifferenceResponse
{
    public int       Id             { get; set; }
    public string?   DocNo          { get; set; }
    public string    Status         { get; set; } = null!;
    public DateTime  PostingDate    { get; set; }
    public DateTime  DocumentDate   { get; set; }
    public string    LocalCurrency  { get; set; } = null!;
    public string    SystemCurrency { get; set; } = null!;
    public decimal   ConversionRate { get; set; }
    public string?   AccountFrom    { get; set; }
    public string?   AccountTo      { get; set; }
    public decimal   TotalGain      { get; set; }
    public decimal   TotalLoss      { get; set; }
    public decimal   NetDifference  { get; set; }
    public string?   Remarks        { get; set; }
    public DateTime  CreatedAt      { get; set; }
    public DateTime? UpdatedAt      { get; set; }
    public int       LineCount      { get; set; }
    public List<ConversionDifferenceLineResponse> Lines { get; set; } = new();
}

public class ConversionDifferenceLineRequest
{
    public string  SourceType     { get; set; } = "GL";
    public string  AccountCode    { get; set; } = null!;
    public string? AccountName    { get; set; }
    public decimal LocalBalance   { get; set; }
    public decimal HistoricalRate { get; set; }
    public string? Remarks        { get; set; }
}

public class ConversionDifferenceRequest
{
    public DateTime? PostingDate    { get; set; }
    public DateTime? DocumentDate   { get; set; }
    public string?   LocalCurrency  { get; set; }
    public string    SystemCurrency { get; set; } = null!;
    public decimal   ConversionRate { get; set; }
    public string?   AccountFrom    { get; set; }
    public string?   AccountTo      { get; set; }
    public string?   Remarks        { get; set; }
    public List<ConversionDifferenceLineRequest> Lines { get; set; } = new();
}
