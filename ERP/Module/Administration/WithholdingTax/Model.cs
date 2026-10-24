namespace FarmingApi.Modules.Administration.WithholdingTax;

public class WithholdingTaxCodeDto
{
    public int      Id                { get; set; }
    public string   Code              { get; set; } = null!;
    public bool     Inactive          { get; set; }
    public string?  Name              { get; set; }
    public string   Category          { get; set; } = "Payment";
    public DateOnly? EffectiveFrom    { get; set; }
    public decimal  Rate              { get; set; }
    public string   BaseType          { get; set; } = "Net";
    public string   RoundingType      { get; set; } = "Commercial Values";
    public decimal  BaseAmountPercent { get; set; } = 100m;
    public string?  OfficialCode      { get; set; }
    public string?  Account           { get; set; }
}

public class WithholdingTaxCodeSaveRequest
{
    public string   Code              { get; set; } = null!;
    public bool     Inactive          { get; set; }
    public string?  Name              { get; set; }
    public string   Category          { get; set; } = "Payment";
    public DateOnly? EffectiveFrom    { get; set; }
    public decimal  Rate              { get; set; }
    public string   BaseType          { get; set; } = "Net";
    public string   RoundingType      { get; set; } = "Commercial Values";
    public decimal  BaseAmountPercent { get; set; } = 100m;
    public string?  OfficialCode      { get; set; }
    public string?  Account           { get; set; }
}
