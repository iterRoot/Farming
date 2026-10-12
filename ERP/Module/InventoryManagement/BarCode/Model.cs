namespace FarmingApi.Modules.Inventory.BarCode;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class BarCodeResponse
{
    public int      Id              { get; set; }
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  UoMGroup        { get; set; }
    public string   BarCodeType     { get; set; } = "Sale";
    public int      VersionNum      { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime? UpdatedAt      { get; set; }
    public List<BarCodeLineResponse> Lines { get; set; } = new();

    // Computed
    public string?  DefaultCode     { get; set; }   // The default barcode
    public int      TotalCodes      { get; set; }
}

public class BarCodeLineResponse
{
    public int     Id        { get; set; }
    public int     LineNum   { get; set; }
    public string? UoM       { get; set; }
    public string  Code      { get; set; } = null!;
    public string? FreeText  { get; set; }
    public bool    IsDefault { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BarCodeCreateRequest
{
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  UoMGroup        { get; set; }
    public string?  BarCodeType     { get; set; }   // Sale | Purchase | Inventory (default Sale)
    public List<BarCodeLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BarCodeUpdateRequest
{
    public string?  ItemDescription { get; set; }
    public string?  UoMGroup        { get; set; }
    public string?  BarCodeType     { get; set; }   // Sale | Purchase | Inventory
    public List<BarCodeLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// LINE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BarCodeLineRequest
{
    public int     LineNum   { get; set; }
    public string? UoM       { get; set; }
    public string  Code      { get; set; } = null!;
    public string? FreeText  { get; set; }
    public bool    IsDefault { get; set; } = false;
}

// ═══════════════════════════════════════════════════════════════════
// SEARCH — find by barcode value
// ═══════════════════════════════════════════════════════════════════
public class BarCodeSearchResponse
{
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  UoMGroup        { get; set; }
    public string   BarCodeType     { get; set; } = "Sale";
    public string   Code            { get; set; } = null!;
    public string?  UoM             { get; set; }
    public string?  FreeText        { get; set; }
    public bool     IsDefault       { get; set; }

    // What the barcode stands for right now — read live on every scan.
    public string?  StockUoM        { get; set; }   // unit stock is counted in
    public decimal  UnitPrice       { get; set; }   // selling price per unit
    public string?  Currency        { get; set; }
    public decimal  OnHand          { get; set; }   // all warehouses
    public List<BarCodeStockRow> Warehouses { get; set; } = new();
}

public class BarCodeStockRow
{
    public string  WhsCode   { get; set; } = "";
    public string? WhsName   { get; set; }
    public decimal Qty       { get; set; }
    public string? UoM       { get; set; }
    public decimal UnitPrice { get; set; }
}