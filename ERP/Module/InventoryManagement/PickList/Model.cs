namespace FarmingApi.Modules.InventoryManagement.PickList;

public class PickListLineDto
{
    public int      Id              { get; set; }
    public int      LineNum         { get; set; }
    public string?  TransType       { get; set; }
    public string?  DocNo           { get; set; }
    public string?  BpCode          { get; set; }
    public DateTime? DeliveryDueDate { get; set; }
    public string?  ItemNo          { get; set; }
    public string?  Description     { get; set; }
    public string?  UomCode         { get; set; }
    public string?  UomName         { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  OrderQty        { get; set; }
    public decimal  OpenQty         { get; set; }
    public decimal  PickedQty       { get; set; }
}

public class PickListResponse
{
    public int      Id         { get; set; }
    public string   PickNumber { get; set; } = null!;
    public DateTime? PickDate  { get; set; }
    public string?  User       { get; set; }
    public string?  Picker     { get; set; }
    public string   Status     { get; set; } = null!;
    public string?  Remarks    { get; set; }
    public int      LineCount  { get; set; }
    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PickListLineDto> Lines { get; set; } = new();
}

public class PickListLineRequest
{
    public string?  TransType       { get; set; }
    public string?  DocNo           { get; set; }
    public string?  BpCode          { get; set; }
    public DateTime? DeliveryDueDate { get; set; }
    public string?  ItemNo          { get; set; }
    public string?  Description     { get; set; }
    public string?  UomCode         { get; set; }
    public string?  UomName         { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  OrderQty        { get; set; }
    public decimal  OpenQty         { get; set; }
    public decimal  PickedQty       { get; set; }
}

public class PickListCreateRequest
{
    public DateTime? PickDate { get; set; }
    public string?   User     { get; set; }
    public string?   Picker   { get; set; }
    public string?   Status   { get; set; }   // defaults to Released
    public string?   Remarks  { get; set; }
    public List<PickListLineRequest> Lines { get; set; } = new();
}

// The edit screen only revises the header and the picked quantities.
public class PickListUpdateRequest
{
    public string?  User    { get; set; }
    public string?  Picker  { get; set; }
    public string?  Status  { get; set; }
    public string?  Remarks { get; set; }
    public List<PickListUpdateLine> Lines { get; set; } = new();
}

public class PickListUpdateLine
{
    public int     Id        { get; set; }
    public decimal PickedQty { get; set; }
}

// One open document line available to be picked.
public class PickCandidate
{
    public int      Id              { get; set; }   // source line id
    public string   TransType       { get; set; } = "Sales Order";
    public string?  DocNo           { get; set; }
    public string?  BpCode          { get; set; }
    public DateTime? DeliveryDueDate { get; set; }
    public string?  ItemNo          { get; set; }
    public string?  Description     { get; set; }
    public string?  UomCode         { get; set; }
    public string?  UomName         { get; set; }
    public string?  WhsCode         { get; set; }
    public decimal  OrderQty        { get; set; }
    public decimal  OpenQty         { get; set; }
}
