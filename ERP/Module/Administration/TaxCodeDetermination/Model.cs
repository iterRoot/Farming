namespace FarmingApi.Modules.Administration.TaxCodeDetermination;

public class TaxDeterminationRuleDto
{
    public int      Id           { get; set; }
    public string   DocumentType { get; set; } = "All";
    public string?  BusinessArea { get; set; }
    public string?  Condition1   { get; set; }
    public string?  Value1       { get; set; }
    public string?  Condition2   { get; set; }
    public string?  Value2       { get; set; }
    public string?  Condition3   { get; set; }
    public string?  Value3       { get; set; }
    public string?  Description  { get; set; }
    public string   LineTaxCode  { get; set; } = null!;
    public int      Priority     { get; set; }
    public bool     InActive     { get; set; }
}

public class TaxDeterminationRuleSaveRequest
{
    public string   DocumentType { get; set; } = "All";
    public string?  BusinessArea { get; set; }
    public string?  Condition1   { get; set; }
    public string?  Value1       { get; set; }
    public string?  Condition2   { get; set; }
    public string?  Value2       { get; set; }
    public string?  Condition3   { get; set; }
    public string?  Value3       { get; set; }
    public string?  Description  { get; set; }
    public string   LineTaxCode  { get; set; } = null!;
    public int      Priority     { get; set; }
    public bool     InActive     { get; set; }
}
