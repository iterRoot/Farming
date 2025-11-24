// using FarmingApi.Modules.Position;

namespace FarmingApi.Modules.Company;

public class CompanyListResponse
{
	public Guid GuidId { get; set; }
	public int Id { get; set; }

	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public string? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}


public class CompanyDetailResponse
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public string? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
	// public List<PositionListResponse> Positions { get; set; } = null!;
}

public class CompanyInsertRequest
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public IFormFile? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}


public class CompanyUpdateRequest
{
	public string Name { get; set; } = null!;
	public string? ShortName { get; set; }
	public IFormFile? Logo { get; set; }
	public string? Desc { get; set; }
	public string? Address { get; set; }
	public bool? InActive { get; set; }
}