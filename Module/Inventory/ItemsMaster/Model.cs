namespace FarmingApi.Modules.Master.ItemsMaster;

public class ItemsMasterResponse
{
    public string ItemsCode { get; set; }
    public string ItemsName {get; set;}
    public string UomName {get; set;}
    public string UomCode {get; set;}
    public string Price {get; set;}
    public string PriceList {get; set;}
    public DateTime CreatedAt {get; set;}
    public string CreatedBy {get; set;}
    public DateTime UpdatedAt {get; set;}
    public string UpdatedBy {get; set;}
    public DateTime DeletedAt {get; set;}
    public string DeletedBy {get; set;}
    public string Types {get; set;}
    public ItemStatus Status {get; set;}
}
public class ItemsMasterListResponse
{
    public string ItemsCode { get; set; }
    public string ItemsName {get; set;}
    public string ItemsFName {get; set;}
    public string Note {get; set;}
    public string UomName {get; set;}
    public string UomCode {get; set;}
    public string Price {get; set;}
    public string PriceList {get; set;}
    public DateTime CreatedAt {get; set;}
    public string CreatedBy {get; set;}
    public DateTime UpdatedAt {get; set;}
    public string UpdatedBy {get; set;}
    public DateTime DeletedAt {get; set;}
    public string DeletedBy {get; set;}
    public string Types {get; set;}
    public ItemStatus Status {get; set;}

}
public class ItemsMasterListRequest 
{
    public string ItemsCode { get; set; }
    public string ItemsName {get; set;}
    public string ItemsFName {get; set;}
    public string Note {get; set;}
    public string UomName {get; set;}
    public string UomCode {get; set;}
    public string Price {get; set;}
    public string PriceList {get; set;}
    public string Types {get; set;}
    public int LifeTime {get; set;}
    public DateTime StartDate {get; set;}
    public DateTime EndDate {get; set;}
    public ItemStatus Status {get; set;}

}

public class ItemsMasterUpdateRequest
{
    // public string ItemsCode { get; set; }
    public string ItemsName {get; set;}
    public string ItemsFName {get; set;}
    public string Note {get; set;}
    public string UomName {get; set;}
    public string UomCode {get; set;}
    public string Price {get; set;}
    public string PriceList {get; set;}
    public string Types {get; set;}
    public ItemStatus Status {get; set;}

}