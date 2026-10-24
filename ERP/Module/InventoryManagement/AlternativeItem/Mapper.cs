using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.AlternativeItem;

public class AlternativeItemMapper : Profile
{
    public AlternativeItemMapper()
    {
        CreateMap<AlternativeItem, AlternativeItemResponse>();
    }
}
