using AutoMapper;

namespace FarmingApi.Modules.Master.ItemsMaster;

public class ItemsMasterMapper : Profile
{
	public ItemsMasterMapper()
	{
		CreateMap<ItemsMaster, ItemsMasterListResponse>();
        CreateMap<ItemsMasterListResponse, ItemsMaster>();

		CreateMap<ItemsMasterListRequest, ItemsMaster>();
		CreateMap<ItemsMaster, ItemsMasterListRequest>();
		// ItemsMasterUpdateRequest

		CreateMap<ItemsMasterUpdateRequest, ItemsMaster>();


	}
}