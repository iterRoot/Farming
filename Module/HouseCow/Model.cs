
namespace FarmingApi.Modules.House;

public class HouseListResponse
{
	// public Guid GuidId { get; set; }
	public int Id { get; set; }
	public string Sex { get; set; } = null!;
	public int? Age { get; set; }
	public string? Birthday { get; set; }
    public string? Variety { get; set; }
	public string? Desc { get; set; }
	public bool? InActive { get; set; }
}

public class HouseListRequest
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
public class HouseUpdateRequest
{
	public string Sex { get; set; } = null!;
	public int? Age { get; set; }
	public string? Birthday { get; set; }
    public string? Variety { get; set; }
	public string? Desc { get; set; }
	public bool? InActive { get; set; }

}