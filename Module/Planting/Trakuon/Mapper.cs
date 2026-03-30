using AutoMapper;

namespace FarmingApi.Modules.Trakuons;

public class TrakuonsMapper : Profile
{
	public TrakuonsMapper()
	{
		CreateMap<Trakuons, TrakuonsListResponse>();
        CreateMap<TrakuonsListResponse, Trakuons>();

		CreateMap<TrakuonsListRequest, Trakuons>();
		CreateMap<Trakuons, TrakuonsListRequest>();
		// TrakuonsUpdateRequest

		CreateMap<TrakuonsUpdateRequest, Trakuons>();


	}
}