using AutoMapper;

namespace KamnttraApi.Modules.InventoryManagement.AlternativeItem;

public class AlternativeItemMapper : Profile
{
    public AlternativeItemMapper()
    {
        CreateMap<AlternativeItem, AlternativeItemResponse>();
    }
}
