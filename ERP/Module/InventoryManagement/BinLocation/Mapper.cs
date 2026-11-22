using AutoMapper;

namespace KamnttraApi.Modules.InventoryManagement.BinLocation;

public class BinLocationMapper : Profile
{
    public BinLocationMapper()
    {
        CreateMap<BinLocation, BinLocationResponse>();
    }
}
