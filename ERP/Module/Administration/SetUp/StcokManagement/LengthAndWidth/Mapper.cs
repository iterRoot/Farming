
// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace KamnttraApi.Modules.Inventory.LengthWidth;

public class LengthWidthUomMapper : Profile
{
    public LengthWidthUomMapper()
    {
        CreateMap<LengthWidthUom, LengthWidthUomResponse>();
        CreateMap<LengthWidthUomRequest, LengthWidthUom>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}
