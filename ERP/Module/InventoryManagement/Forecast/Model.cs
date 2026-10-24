namespace FarmingApi.Modules.InventoryManagement.Forecast;

public class ForecastLineRequest
{
    public string   ItemCode     { get; set; } = null!;
    public DateTime ForecastDate { get; set; }
    public decimal  Quantity     { get; set; }
}

public class ForecastRequest
{
    public string   Name       { get; set; } = null!;
    public string   PeriodType { get; set; } = "Monthly";
    public DateTime StartDate  { get; set; }
    public DateTime EndDate    { get; set; }
    public bool     IsActive   { get; set; } = true;
    public string?  Remarks    { get; set; }
    public List<ForecastLineRequest> Lines { get; set; } = new();
}

public class ForecastLineResponse
{
    public int      Id           { get; set; }
    public int      LineNum      { get; set; }
    public string   ItemCode     { get; set; } = null!;
    public string   ItemName     { get; set; } = null!;
    public DateTime ForecastDate { get; set; }
    public decimal  Quantity     { get; set; }
}

public class ForecastResponse
{
    public int       Id         { get; set; }
    public string    Code       { get; set; } = null!;
    public string    Name       { get; set; } = null!;
    public string    PeriodType { get; set; } = "Monthly";
    public DateTime  StartDate  { get; set; }
    public DateTime  EndDate    { get; set; }
    public bool      IsActive   { get; set; }
    public string?   Remarks    { get; set; }
    public decimal   TotalQty   { get; set; }
    public DateTime  CreatedAt  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
    public List<ForecastLineResponse> Lines { get; set; } = new();
}
