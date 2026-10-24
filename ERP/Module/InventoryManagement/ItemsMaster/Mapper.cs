using AutoMapper;

namespace FarmingApi.Modules.Inventory.ItemsMaster;

public class ItemsMasterProfile : Profile
{
    public ItemsMasterProfile()
    {
        // Entity → Response
        CreateMap<ItemsMaster, ItemsMasterResponse>();

        // Create
        CreateMap<ItemsMasterRequest, ItemsMaster>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src =>
                    src.Status == 0 ? ItemStatus.Active : ItemStatus.InActive));

        // Update
        CreateMap<ItemsMasterUpdateRequest, ItemsMaster>();
    }
}