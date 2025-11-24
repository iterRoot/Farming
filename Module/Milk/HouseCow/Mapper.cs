using AutoMapper;

namespace FarmingApi.Modules.HouseCow;

public class HouseCowMapper : Profile
{
	public HouseCowMapper()
	{
		CreateMap<HouseCow, HouseCowListResponse>();
        CreateMap<HouseCowListResponse, HouseCow>();

		CreateMap<HouseCowListRequest, HouseCow>();
		CreateMap<HouseCow, HouseCowListRequest>();

	}
}