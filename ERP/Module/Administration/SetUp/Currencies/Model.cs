namespace FarmingApi.Modules.Administration.SetUp.Currencies;

// ══════════════════════════════════════════════════════════════════════════════
// REQUEST (UI → API)
// ══════════════════════════════════════════════════════════════════════════════
public class CurrencyRequest
{
    public string   Code                 { get; set; } = "";
    public string   CurrencyName         { get; set; } = "";
    public string?  IntlCode             { get; set; }
    public string?  IntlDescription      { get; set; }
    public string?  HundredthName        { get; set; }
    public string?  English              { get; set; }
    public string?  EnglishHundredthName { get; set; }
    public string?  IsoCurrencyCode      { get; set; }
    public decimal? IncomingAmtDiff      { get; set; }
    public decimal? OutgoingAmtDiff      { get; set; }
    public decimal? IncomingPctDiff      { get; set; }
    public decimal? OutgoingPctDiff      { get; set; }
    public string   Rounding             { get; set; } = "No Rounding";
    public string   Decimals             { get; set; } = "Default";
    public bool     RoundingInPayment    { get; set; }
}

// ══════════════════════════════════════════════════════════════════════════════
// RESPONSE (API → UI)
// ══════════════════════════════════════════════════════════════════════════════
public class CurrencyResponse
{
    public int      Id                   { get; set; }
    public string   Code                 { get; set; } = "";
    public string   CurrencyName         { get; set; } = "";
    public string?  IntlCode             { get; set; }
    public string?  IntlDescription      { get; set; }
    public string?  HundredthName        { get; set; }
    public string?  English              { get; set; }
    public string?  EnglishHundredthName { get; set; }
    public string?  IsoCurrencyCode      { get; set; }
    public decimal? IncomingAmtDiff      { get; set; }
    public decimal? OutgoingAmtDiff      { get; set; }
    public decimal? IncomingPctDiff      { get; set; }
    public decimal? OutgoingPctDiff      { get; set; }
    public string   Rounding             { get; set; } = "";
    public string   Decimals             { get; set; } = "";
    public bool     RoundingInPayment    { get; set; }
    public DateTime CreatedAt            { get; set; }
}
