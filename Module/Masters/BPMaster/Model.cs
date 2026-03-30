namespace FarmingApi.Modules.Master.UserMaster;

public class UserMasterResponse
{
    public string FirstName {get; set;}
    public string MidName {get; set;}
    public string LastName {get;set;}
    public string Note { get; set;}
    public TypeStatus TypeStatus {get; set;} = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}


public class UserMasterInsertrequest
{
    public string Code { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string MidName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Note { get; set; } = null!;
    public TypeStatus TypeStatus {get; set;} = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}


