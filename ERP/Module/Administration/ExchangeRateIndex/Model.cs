namespace FarmingApi.Modules.Financials.ExchangeRate;

// ── Exchange Rate ─────────────────────────────────────────────
public class ExchangeRateResponse
{
    public int      Id            { get; set; }
    public string   Currency      { get; set; } = null!;
    public string   CurrencyName  { get; set; } = null!;
    public string   BaseCurrency  { get; set; } = null!;
    public decimal  Rate          { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class ExchangeRateRequest
{
    public string   Currency      { get; set; } = null!;
    public string   CurrencyName  { get; set; } = null!;
    public string   BaseCurrency  { get; set; } = "KHR";
    public decimal  Rate          { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; } = true;
}

// ── Price Index ───────────────────────────────────────────────
public class PriceIndexResponse
{
    public int      Id            { get; set; }
    public string   IndexCode     { get; set; } = null!;
    public string   IndexName     { get; set; } = null!;
    public string?  Category      { get; set; }
    public decimal  IndexValue    { get; set; }
    public string?  Unit          { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; }
    public DateTime CreatedAt     { get; set; }
    public DateTime UpdatedAt     { get; set; }
}

public class PriceIndexRequest
{
    public string   IndexCode     { get; set; } = null!;
    public string   IndexName     { get; set; } = null!;
    public string?  Category      { get; set; }
    public decimal  IndexValue    { get; set; }
    public string?  Unit          { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string?  Source        { get; set; }
    public string?  Remarks       { get; set; }
    public bool     IsActive      { get; set; } = true;
}

// ── Auto-update settings ──────────────────────────────────────
public class ExchangeRateSettingsDto
{
    public bool      AutoFetchEnabled { get; set; }
    public string    BaseCurrency     { get; set; } = "KHR";
    public string    Currencies       { get; set; } = "USD";
    public int       RunAtHourUtc     { get; set; }
    public DateTime? LastAttemptAt    { get; set; }
    public DateTime? LastSuccessAt    { get; set; }
    public string?   LastStatus       { get; set; }
    public string?   LastMessage      { get; set; }
}

public class ExchangeRateSettingsRequest
{
    public bool    AutoFetchEnabled { get; set; }
    public string? BaseCurrency     { get; set; }
    public string? Currencies       { get; set; }
    public int     RunAtHourUtc     { get; set; }
}
