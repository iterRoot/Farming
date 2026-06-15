

// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.CycleCount;

public class CycleCountMapper : Profile
{
    public CycleCountMapper()
    {
        CreateMap<CycleCountDetermination, CycleCountResponse>();
        CreateMap<CycleCountRequest, CycleCountDetermination>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}
