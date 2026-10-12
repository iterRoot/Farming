using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.PriceLists;

public class PriceListMapper : Profile
{
    public PriceListMapper()
    {
        CreateMap<PriceList, PriceListResponse>()
            .ForMember(d => d.BasePriceListName, o => o.MapFrom(s => s.BasePriceList != null ? s.BasePriceList.Name : null));

        CreateMap<PriceListRequest, PriceList>()
            .ForMember(d => d.Id,            o => o.Ignore())
            .ForMember(d => d.BasePriceList, o => o.Ignore())
            .ForMember(d => d.CreatedAt,     o => o.Ignore())
            .ForMember(d => d.UpdatedAt,     o => o.Ignore())
            .ForMember(d => d.InActive,      o => o.Ignore());

        CreateMap<PriceListBatchRow, PriceList>()
            .IncludeBase<PriceListRequest, PriceList>();
    }
}
