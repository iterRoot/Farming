namespace FarmingApi.Modules.Financials.FixedAssets;

// Full shape returned by GET and accepted by POST/PUT. Audit fields are set
// server-side, never taken from the request.
public class AssetMasterDto
{
    public int      Id            { get; set; }
    public string   ItemNo        { get; set; } = null!;
    public string   Description   { get; set; } = null!;
    public string?  ForeignName   { get; set; }
    public string   ItemType      { get; set; } = "Fixed Assets";
    public string?  ItemGroup     { get; set; }
    public string?  UomGroup      { get; set; }
    public string?  Barcode       { get; set; }
    public string?  PriceList     { get; set; }
    public decimal  UnitPrice     { get; set; }

    public bool     WithholdingTaxLiable     { get; set; }
    public bool     DoNotApplyDiscountGroups { get; set; }
    public bool     DoNotApplyDiscount       { get; set; }
    public string?  Manufacturer  { get; set; }
    public string?  AdditionalIdentifier { get; set; }
    public string?  ShippingType  { get; set; }
    public string   ManageItemBy  { get; set; } = "None";
    public bool     Active        { get; set; } = true;
    public string?  LinkedToResource { get; set; }
    public string?  StandardItemIdentification { get; set; }
    public string?  CommodityClassification { get; set; }

    public string?  AssetClass    { get; set; }
    public string?  AssetGroup    { get; set; }
    public DateOnly? CapitalizationDate { get; set; }
    public int      UsefulLifeMonths      { get; set; }
    public int      RemainingLifeMonths   { get; set; }
    public string   DepreciationMethod    { get; set; } = "Straight Line";
    public decimal  AcquisitionCost       { get; set; }
    public decimal  SalvageValue          { get; set; }
    public decimal  AccumulatedDepreciation { get; set; }
    public string   AssetStatus   { get; set; } = "New";
    public string?  DepreciationAccount   { get; set; }
    public string?  AssetAccount          { get; set; }

    // Net book value is derived, not stored.
    public decimal  NetBookValue  { get; set; }
}

// Same business fields as the DTO minus Id / derived values.
public class AssetMasterSaveRequest
{
    public string   ItemNo        { get; set; } = null!;
    public string   Description   { get; set; } = null!;
    public string?  ForeignName   { get; set; }
    public string   ItemType      { get; set; } = "Fixed Assets";
    public string?  ItemGroup     { get; set; }
    public string?  UomGroup      { get; set; }
    public string?  Barcode       { get; set; }
    public string?  PriceList     { get; set; }
    public decimal  UnitPrice     { get; set; }

    public bool     WithholdingTaxLiable     { get; set; }
    public bool     DoNotApplyDiscountGroups { get; set; }
    public bool     DoNotApplyDiscount       { get; set; }
    public string?  Manufacturer  { get; set; }
    public string?  AdditionalIdentifier { get; set; }
    public string?  ShippingType  { get; set; }
    public string   ManageItemBy  { get; set; } = "None";
    public bool     Active        { get; set; } = true;
    public string?  LinkedToResource { get; set; }
    public string?  StandardItemIdentification { get; set; }
    public string?  CommodityClassification { get; set; }

    public string?  AssetClass    { get; set; }
    public string?  AssetGroup    { get; set; }
    public DateOnly? CapitalizationDate { get; set; }
    public int      UsefulLifeMonths      { get; set; }
    public int      RemainingLifeMonths   { get; set; }
    public string   DepreciationMethod    { get; set; } = "Straight Line";
    public decimal  AcquisitionCost       { get; set; }
    public decimal  SalvageValue          { get; set; }
    public decimal  AccumulatedDepreciation { get; set; }
    public string   AssetStatus   { get; set; } = "New";
    public string?  DepreciationAccount   { get; set; }
    public string?  AssetAccount          { get; set; }
}
