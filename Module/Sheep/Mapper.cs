using AutoMapper;

namespace FarmingApi.Modules.Sheep;

public class SheepMapper : Profile
{
	public SheepMapper()
	{
		CreateMap<Sheep, SheepListResponse>();
        CreateMap<SheepListResponse, Sheep>();

		CreateMap<SheepListRequest, Sheep>();
		CreateMap<Sheep, SheepListRequest>();
		// SheepUpdateRequest

		CreateMap<SheepUpdateRequest, Sheep>();


	}
}