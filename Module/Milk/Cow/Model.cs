
namespace FarmingApi.Modules.MilkCow;

public class CowListResponse
{
	public Guid GuidId { get; set; }
	public int Id { get; set; }
	public string Sex { get; set; } = null!;
	public int? Age { get; set; }
	public string? Birthday { get; set; }
    public string? Variety { get; set; }
	public string? Desc { get; set; }
	public bool? InActive { get; set; }
}

public class CowListRequest
{
	// public Guid GuidId { get; set; }
	// public int Id { get; set; }
	public string Sex { get; set; } = null!;
	public int? Age { get; set; }
	public string? Birthday { get; set; }
    public string? Variety { get; set; }
	public string? Desc { get; set; }
	public bool? InActive { get; set; }
}