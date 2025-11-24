
namespace FarmingApi.Modules.HouseCow;

public class HouseCowListResponse
{
	public Guid GuidId { get; set; }
	public int Id { get; set; }
    public string HouseName { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
	public string? Desc { get; set; }
}

public class HouseCowListRequest
{
	// public Guid GuidId { get; set; }
	// public int Id { get; set; }
    public string HouseName { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
	public string? Desc { get; set; }
}