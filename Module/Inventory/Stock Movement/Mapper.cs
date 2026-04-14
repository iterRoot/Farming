using AutoMapper;

namespace FarmingApi.Modules.Sale.StockMovement;

public class StockMovementMapper : Profile
{
	public StockMovementMapper()
	{
		CreateMap<StockMovement, StockMovementListResponse>();
        CreateMap<StockMovementListResponse, StockMovement>();

		CreateMap<StockMovementListRequest, StockMovement>();
		CreateMap<StockMovement, StockMovementListRequest>();
		// StockMovementUpdateRequest

		CreateMap<StockMovementUpdateRequest, StockMovement>();


	}
}