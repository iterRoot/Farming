namespace FarmingApi.Modules.Inventory.StockTransferRequest;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestResponse
{
    public int       Id              { get; set; }
    public string    DocNo           { get; set; } = null!;
    public string    Series          { get; set; } = null!;
    public string    Status          { get; set; } = null!;
    public DateTime  PostingDate     { get; set; }
    public DateTime  DueDate         { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = null!;
    public string    ToWarehouse     { get; set; } = null!;
    public string?   PriceList       { get; set; }
    public string?   ReferencedDoc   { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   PickPackRemarks { get; set; }
    public string?   Remarks         { get; set; }
    public decimal   TotalQuantity   { get; set; }
    public int       VersionNum      { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public DateTime? UpdatedAt       { get; set; }
    public List<StockTransferRequestLineResponse> Lines { get; set; } = new();
}

public class StockTransferRequestLineResponse
{
    public int      Id              { get; set; }
    public int      LineNum         { get; set; }
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  FromWarehouse   { get; set; }
    public string?  ToWarehouse     { get; set; }
    public decimal  Quantity        { get; set; }
    public string?  UoMCode        { get; set; }
    public string?  UoMName        { get; set; }
    public decimal  UnitPrice      { get; set; }
    public decimal  LineTotal      { get; set; }
    public string   LineStatus     { get; set; } = null!;
    public string?  Remarks        { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestCreateRequest
{
    public string    Series          { get; set; } = "Primary";
    public DateTime  PostingDate     { get; set; }
    public DateTime  DueDate         { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = "01";
    public string    ToWarehouse     { get; set; } = "01";
    public string?   PriceList       { get; set; } = "Last Purchase Price";
    public string?   ReferencedDoc   { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   PickPackRemarks { get; set; }
    public string?   Remarks         { get; set; }
    public List<StockTransferRequestLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestUpdateRequest
{
    public DateTime  PostingDate     { get; set; }
    public DateTime  DueDate         { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = "01";
    public string    ToWarehouse     { get; set; } = "01";
    public string?   PriceList       { get; set; }
    public string?   ReferencedDoc   { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   PickPackRemarks { get; set; }
    public string?   Remarks         { get; set; }
    public List<StockTransferRequestLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// LINE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferRequestLineRequest
{
    public int      LineNum         { get; set; }
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  FromWarehouse   { get; set; }
    public string?  ToWarehouse     { get; set; }
    public decimal  Quantity        { get; set; }
    public string?  UoMCode        { get; set; }
    public string?  UoMName        { get; set; }
    public decimal  UnitPrice      { get; set; }
    public string?  Remarks        { get; set; }
}