namespace FarmingApi.Modules.Branch;

public class BranchListResponse
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; }
}

public class BranchListRequest
{
    public string BranchCode { get; set; } = null!;
    public string BranchName { get; set; }
}
public class BranchUpdateRequest
{
    public string BranchName { get; set; }
}