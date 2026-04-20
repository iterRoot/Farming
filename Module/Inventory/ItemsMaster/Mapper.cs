using AutoMapper;

namespace FarmingApi.Modules.Master.ItemsMaster;

public class ItemsMasterMapper : Profile
{
    public ItemsMasterMapper()
    {
        CreateMap<ItemsMaster, ItemsMasterResponse>();

        CreateMap<ItemsMasterRequest, ItemsMaster>();

        CreateMap<ItemsMasterUpdateRequest, ItemsMaster>();
    }
}