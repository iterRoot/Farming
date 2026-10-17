using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.BinLocation;

public class BinLocationMapper : Profile
{
    public BinLocationMapper()
    {
        CreateMap<BinLocation, BinLocationResponse>();
    }
}
