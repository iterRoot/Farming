namespace FarmingApi.Modules.Administration.Security;

public class ReadOnlyDbUserListResponse
{
    public int      Id          { get; set; }
    public string   Username    { get; set; } = null!;
    public string?  Description { get; set; }
    public bool     Enabled     { get; set; }
    public DateTime CreatedAt   { get; set; }
}

public class ReadOnlyDbUserRequest
{
    public string  Username    { get; set; } = null!;
    public string? Description { get; set; }
    public bool    Enabled     { get; set; } = true;
}
