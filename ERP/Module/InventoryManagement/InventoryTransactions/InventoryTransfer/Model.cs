namespace FarmingApi.Modules.Inventory.StockTransfer;

// ═══════════════════════════════════════════════════════════════════
// RESPONSE
// ═══════════════════════════════════════════════════════════════════
public class StockTransferResponse
{
    public int       Id              { get; set; }
    public int       Number          { get; set; }
    public string    DocNo           { get; set; } = null!;
    public string    Series          { get; set; } = null!;
    public DateTime  PostingDate     { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = null!;
    public string    ToWarehouse     { get; set; } = null!;
    public string?   ToBinLocation   { get; set; }
    public string?   PriceList       { get; set; }
    public string?   ReferencedDoc   { get; set; }
    public int?      BaseDocEntry    { get; set; }
    public string?   BaseDocType     { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   Remarks         { get; set; }
    public decimal   TotalQuantity   { get; set; }
    public int       VersionNum      { get; set; }
    public DateTime  CreatedAt       { get; set; }
    public DateTime? UpdatedAt       { get; set; }
    public List<StockTransferLineResponse> Lines { get; set; } = new();
}

public class StockTransferLineResponse
{
    public int      Id              { get; set; }
    public int      LineNum         { get; set; }
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  FromWarehouse   { get; set; }
    public string?  FromBinLocation { get; set; }
    public string?  ToWarehouse     { get; set; }
    public string?  ToBinLocation   { get; set; }
    public decimal  Quantity        { get; set; }
    public string?  UoMCode        { get; set; }
    public string?  UoMName        { get; set; }
    public decimal  UnitPrice      { get; set; }
    public decimal  FirstPrice     { get; set; }
    public decimal  LineTotal      { get; set; }
    public string?  Remarks        { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferCreateRequest
{
    public string    Series          { get; set; } = "Primary";
    public DateTime  PostingDate     { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = "01";
    public string    ToWarehouse     { get; set; } = "01";
    public string?   ToBinLocation   { get; set; }
    public string?   PriceList       { get; set; } = "Last Purchase Price";
    public string?   ReferencedDoc   { get; set; }
    public int?      BaseDocEntry    { get; set; }
    public string?   BaseDocType     { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   Remarks         { get; set; }
    public List<StockTransferLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferUpdateRequest
{
    public DateTime  PostingDate     { get; set; }
    public DateTime  DocumentDate    { get; set; }
    public string?   CardCode        { get; set; }
    public string?   CardName        { get; set; }
    public string?   ContactPerson   { get; set; }
    public string?   ShipTo          { get; set; }
    public string    FromWarehouse   { get; set; } = "01";
    public string    ToWarehouse     { get; set; } = "01";
    public string?   ToBinLocation   { get; set; }
    public string?   PriceList       { get; set; }
    public string?   ReferencedDoc   { get; set; }
    public string?   SalesEmployee   { get; set; }
    public string?   JournalRemarks  { get; set; }
    public string?   Remarks         { get; set; }
    public List<StockTransferLineRequest> Lines { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// LINE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class StockTransferLineRequest
{
    public int      LineNum         { get; set; }
    public string   ItemNo          { get; set; } = null!;
    public string?  ItemDescription { get; set; }
    public string?  FromWarehouse   { get; set; }
    public string?  FromBinLocation { get; set; }
    public string?  ToWarehouse     { get; set; }
    public string?  ToBinLocation   { get; set; }
    public decimal  Quantity        { get; set; }
    public string?  UoMCode        { get; set; }
    public string?  UoMName        { get; set; }
    public decimal  UnitPrice      { get; set; }
    public decimal  FirstPrice     { get; set; }
    public string?  Remarks        { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// COPY FROM REQUEST — copy lines from STR or another doc
// ═══════════════════════════════════════════════════════════════════
public class CopyFromRequest
{
    public string   SourceDocType  { get; set; } = "KSTR";  // KSTR=Stock Transfer Request
    public int      SourceDocEntry { get; set; }
}