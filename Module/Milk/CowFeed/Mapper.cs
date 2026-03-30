using AutoMapper;

namespace FarmingApi.Modules.CowFeed;

public class CowFeedMapper : Profile
{
	public CowFeedMapper()
	{
		CreateMap<CowFeed, CowFeedListResponse>();
        CreateMap<CowFeedListResponse, CowFeed>();

		CreateMap<CowFeedListRequest, CowFeed>();
		CreateMap<CowFeed, CowFeedListRequest>();

		CreateMap<CowFeedUpdateRequest, CowFeed>();

	}
}