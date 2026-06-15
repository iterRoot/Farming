

// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.UnitOfMeasure;

public class UnitOfMeasureMapper : Profile
{
    public UnitOfMeasureMapper()
    {
        CreateMap<UnitOfMeasure, UnitOfMeasureResponse>();
        CreateMap<UnitOfMeasureRequest, UnitOfMeasure>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}

