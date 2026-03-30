using AutoMapper;

namespace FarmingApi.Modules.House;

public class HouseMapper : Profile
{
	public HouseMapper()
	{
		CreateMap<House, HouseListResponse>();
        CreateMap<HouseListResponse, House>();

		CreateMap<HouseListRequest, House>();
		CreateMap<House, HouseListRequest>();
		// CowUpdateRequest

		CreateMap<HouseUpdateRequest, House>();


	}
}