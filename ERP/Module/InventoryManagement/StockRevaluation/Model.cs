namespace FarmingApi.Modules.InventoryManagement.StockRevaluation;

public class StockRevaluationLineDto
{
    public int      Id              { get; set; }
    public string?  ItemNo          { get; set; }
    public string?  ItemDescription { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  CurrentCost     { get; set; }
    public decimal  NewCost         { get; set; }
    public string?  Uom             { get; set; }
    public decimal  InStock         { get; set; }
}

public class StockRevaluationSerialLineDto
{
    public int      Id            { get; set; }
    public string?  SbNo          { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? ExpiryDate    { get; set; }
    public decimal  CurrentCost   { get; set; }
    public decimal  NewCost       { get; set; }
    public decimal  OpenQty       { get; set; }
}

public class StockRevaluationListResponse
{
    public int      Id              { get; set; }
    public string   Number          { get; set; } = null!;
    public string?  Series          { get; set; }
    public DateTime? PostingDate    { get; set; }
    public DateTime? DocumentDate   { get; set; }
    public string?  RevaluationType { get; set; }
    public string?  Ref2            { get; set; }
    public int      LineCount       { get; set; }
    public decimal  TotalVariance   { get; set; }
    public string   Status          { get; set; } = null!;
    public bool     CostsApplied    { get; set; }
    public int?     JournalEntryId  { get; set; }
    public DateTime CreatedAt       { get; set; }
}

public class StockRevaluationDetailResponse
{
    public int       Id                      { get; set; }
    public string    Number                  { get; set; } = null!;
    public string?   Series                  { get; set; }
    public DateTime? PostingDate             { get; set; }
    public DateTime? DocumentDate            { get; set; }
    public string?   RevaluationType         { get; set; }
    public string?   Ref2                    { get; set; }
    public bool      InflationBased          { get; set; }
    public int?      ReferencedDocumentId    { get; set; }
    public string?   ReferencedDocumentLabel { get; set; }
    public string    Status                  { get; set; } = null!;
    public string?   Remarks                 { get; set; }
    public string?   JournalRemark           { get; set; }
    public decimal   TotalVariance           { get; set; }
    public bool      CostsApplied            { get; set; }
    public int?      JournalEntryId          { get; set; }
    public List<StockRevaluationLineDto>       Lines            { get; set; } = new();
    public List<StockRevaluationSerialLineDto> SerialBatchLines { get; set; } = new();
}

public class StockRevaluationLineRequest
{
    public string?  ItemNo          { get; set; }
    public string?  ItemDescription { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  CurrentCost     { get; set; }
    public decimal  NewCost         { get; set; }
    public string?  Uom             { get; set; }
    public decimal  InStock         { get; set; }
}

public class StockRevaluationSerialLineRequest
{
    public string?  SbNo          { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? ExpiryDate    { get; set; }
    public decimal  CurrentCost   { get; set; }
    public decimal  NewCost       { get; set; }
    public decimal  OpenQty       { get; set; }
}

public class StockRevaluationCreateRequest
{
    public string?   Series               { get; set; }
    public DateTime? PostingDate          { get; set; }
    public DateTime? DocumentDate         { get; set; }
    public string?   RevaluationType      { get; set; }
    public string?   Ref2                 { get; set; }
    public bool      InflationBased       { get; set; }
    public int?      ReferencedDocumentId { get; set; }
    public string?   Remarks              { get; set; }
    public string?   JournalRemark        { get; set; }
    public List<StockRevaluationLineRequest>       Lines            { get; set; } = new();
    public List<StockRevaluationSerialLineRequest> SerialBatchLines { get; set; } = new();
}

// The edit screen only revises the two remark fields.
public class StockRevaluationUpdateRequest
{
    public string? Remarks       { get; set; }
    public string? JournalRemark { get; set; }
}

// One serial/batch number available to revalue.
public class SerialBatchCandidate
{
    public int      Id            { get; set; }
    public string?  SbNo          { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? ExpiryDate    { get; set; }
    public decimal  CurrentCost   { get; set; }
    public decimal  OpenQty       { get; set; }
}

// What a revaluation line starts from: the item's live cost and on-hand stock.
// Cost is one moving average per item, so the stock is the total across all
// warehouses — changing the cost revalues every warehouse at once.
public class StockRevaluationItemInfo
{
    public string  ItemCode    { get; set; } = null!;
    public string  ItemName    { get; set; } = null!;
    public string? Uom         { get; set; }
    public decimal CurrentCost { get; set; }
    public decimal InStock     { get; set; }
}
