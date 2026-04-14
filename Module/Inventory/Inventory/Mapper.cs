using AutoMapper;

namespace FarmingApi.Modules.Sale.Inventory;

public class InventoryMapper : Profile
{
	public InventoryMapper()
	{
		CreateMap<Inventory, InventoryListResponse>();
        CreateMap<InventoryListResponse, Inventory>();

		CreateMap<InventoryListRequest, Inventory>();
		CreateMap<Inventory, InventoryListRequest>();
		// InventoryUpdateRequest

		CreateMap<InventoryUpdateRequest, Inventory>();


	}
}