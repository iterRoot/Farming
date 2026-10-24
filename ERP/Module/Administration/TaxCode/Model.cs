namespace FarmingApi.Modules.Administration.TaxCode;

public class TaxCodeDto
{
    public int      Id                    { get; set; }
    public string   Code                  { get; set; } = null!;
    public bool     Inactive              { get; set; }
    public string?  Name                  { get; set; }
    public string   Category              { get; set; } = "Output Tax";
    public bool     AcquisitionReverse    { get; set; }
    public DateOnly? EffectiveFrom        { get; set; }
    public decimal  Rate                  { get; set; }
    public decimal  NonDeductiblePercent  { get; set; }
    public string?  TaxAccount            { get; set; }
    public string?  AcquisitionTaxAccount { get; set; }
    public string?  DeferredTaxAccount    { get; set; }
    public string?  NonDeductibleAccount  { get; set; }
    public string?  CodeDescription       { get; set; }
}

public class TaxCodeSaveRequest
{
    public string   Code                  { get; set; } = null!;
    public bool     Inactive              { get; set; }
    public string?  Name                  { get; set; }
    public string   Category              { get; set; } = "Output Tax";
    public bool     AcquisitionReverse    { get; set; }
    public DateOnly? EffectiveFrom        { get; set; }
    public decimal  Rate                  { get; set; }
    public decimal  NonDeductiblePercent  { get; set; }
    public string?  TaxAccount            { get; set; }
    public string?  AcquisitionTaxAccount { get; set; }
    public string?  DeferredTaxAccount    { get; set; }
    public string?  NonDeductibleAccount  { get; set; }
    public string?  CodeDescription       { get; set; }
}
