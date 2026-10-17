namespace FarmingApi.Modules.CRM.CustomerEquipmentCard;

public class CustomerEquipmentCardRequest
{
    public string   ItemCode       { get; set; } = null!;
    public string?  ManufacturerSN { get; set; }
    public string?  InternalSN     { get; set; }
    public string   CustomerCode   { get; set; } = null!;
    public string   Status         { get; set; } = "Active";
    public DateTime? DeliveryDate  { get; set; }
    public DateTime? WarrantyStart { get; set; }
    public DateTime? WarrantyEnd   { get; set; }
    public string?  Location       { get; set; }
    public string?  Remarks        { get; set; }
}

public class CustomerEquipmentCardResponse
{
    public int       Id             { get; set; }
    public string    CardCode       { get; set; } = null!;
    public string    ItemCode       { get; set; } = null!;
    public string?   ItemName       { get; set; }
    public string?   ManufacturerSN { get; set; }
    public string?   InternalSN     { get; set; }
    public string    CustomerCode   { get; set; } = null!;
    public string?   CustomerName   { get; set; }
    public string    Status         { get; set; } = "Active";
    public DateTime? DeliveryDate   { get; set; }
    public DateTime? WarrantyStart  { get; set; }
    public DateTime? WarrantyEnd    { get; set; }
    public string?   Location       { get; set; }
    public string?   Remarks        { get; set; }
    public DateTime  CreatedAt      { get; set; }
}
