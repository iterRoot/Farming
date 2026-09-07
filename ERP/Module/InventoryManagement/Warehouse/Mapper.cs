using AutoMapper;

namespace FarmingApi.Modules.Sale.Warehouse;

public class WarehouseMapper : Profile
{
    public WarehouseMapper()
    {
        // Entity → response: IsActive is the inverse of InActive.
        CreateMap<Warehouse, WarehouseListResponse>()
            .ForMember(d => d.IsActive, o => o.MapFrom(s => !(s.InActive ?? false)));

        // Request → entity: InActive is set in the controller from IsActive.
        CreateMap<WarehouseListRequest, Warehouse>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.InActive, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        CreateMap<WarehouseUpdateRequest, Warehouse>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.InActive, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());
    }
}
