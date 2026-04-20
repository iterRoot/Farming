namespace FarmingApi.Modules.Master.BusinessPartner;

public class BusinessPartnerResponse
{
    public int Id { get; set; }              // 🔥 IMPORTANT
    public string Code { get; set; }         // 🔥 IMPORTANT

    public string FirstName { get; set; }
    public string MidName { get; set; }
    public string LastName { get; set; }

    public string Note { get; set; }

    public TypeStatus TypeStatus { get; set; }
    public ActiveStatus ActiveStatus { get; set; }
}

public class BusinessPartnerCreateRequest
{
    public string Code { get; set; } = null!;

    public string FirstName { get; set; } = null!;
    public string MidName { get; set; } = string.Empty;
    public string LastName { get; set; } = null!;

    public string Note { get; set; } = string.Empty;

    public TypeStatus TypeStatus { get; set; } = TypeStatus.Customer;
    public ActiveStatus ActiveStatus { get; set; } = ActiveStatus.Active;
}


public class BusinessPartnerUpdateRequest
{
    public string FirstName { get; set; } = null!;
    public string MidName { get; set; } = string.Empty;
    public string LastName { get; set; } = null!;

    public string Note { get; set; } = string.Empty;

    public TypeStatus TypeStatus { get; set; }
    public ActiveStatus ActiveStatus { get; set; }
}


