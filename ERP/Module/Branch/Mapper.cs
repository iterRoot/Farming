using AutoMapper;

namespace FarmingApi.Modules.Branch;

public class BranchMapper : Profile
{
	public BranchMapper()
	{
		CreateMap<Branch, BranchListResponse>();
        CreateMap<BranchListResponse, Branch>();

		CreateMap<BranchListRequest, Branch>();
		CreateMap<Branch, BranchListRequest>();
		// BranchUpdateRequest

		CreateMap<BranchUpdateRequest, Branch>();


	}
}