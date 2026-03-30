using AutoMapper;

namespace FarmingApi.Modules.MilkCow;

public class CowMapper : Profile
{
	public CowMapper()
	{
		CreateMap<Cow, CowListResponse>();
        CreateMap<CowListResponse, Cow>();

		CreateMap<CowListRequest, Cow>();
		CreateMap<Cow, CowListRequest>();
		// CowUpdateRequest

		CreateMap<CowUpdateRequest, Cow>();


	}
}