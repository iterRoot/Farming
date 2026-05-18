using AutoMapper;

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

public class GoodsReceiptMapper : Profile
{
	public GoodsReceiptMapper()
	{
        CreateMap<GoodsReceipt, GoodsReceiptListResponse>();



	}
}