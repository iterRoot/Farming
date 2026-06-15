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
    public List<BarCodeLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BarCodeUpdateRequest
{
    public string?  ItemDescription { get; set; }
    public string?  UoMGroup        { get; set; }
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
    public string   Code            { get; set; } = null!;
    public string?  UoM             { get; set; }
    public string?  FreeText        { get; set; }
    public bool     IsDefault       { get; set; }
}