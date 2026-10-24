
// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.UomGroup;

public class UomGroupMapper : Profile
{
    public UomGroupMapper()
    {
        CreateMap<UomGroup, UomGroupResponse>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));
        CreateMap<UomGroupLine, UomGroupLineResponse>();

        CreateMap<UomGroupRequest, UomGroup>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            .ForMember(d => d.Lines,     opt => opt.MapFrom(s => s.Lines));
        CreateMap<UomGroupLineRequest, UomGroupLine>()
            .ForMember(d => d.Id, opt => opt.Ignore());
    }
}

