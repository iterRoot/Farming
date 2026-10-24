// using FarmingApi.Modules.Position;

namespace FarmingApi.Modules.Company.Staff;

public class StaffListResponse
{
	public Guid GuidId { get; set; }
	public int Id { get; set; }

	public string StaffName { get; set; } = null!;
	public string? StaffCode { get; set; }
	public string? Position { get; set; }
	public string? CompanyId { get; set; }
	public bool? InActive { get; set; }
}
public class StaffInsertRequest
{

	public string StaffName { get; set; } = null!;
	public string? StaffCode { get; set; }
	public string? Position { get; set; }
	public string? CompanyId { get; set; }
	public bool? InActive { get; set; }
}

public class StaffUpdateRequest
{

	public string StaffName { get; set; } = null!;
	public string? StaffCode { get; set; }
	public string? Position { get; set; }
	public string? CompanyId { get; set; }
	public bool? InActive { get; set; }
}
