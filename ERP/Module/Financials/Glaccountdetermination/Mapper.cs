// ═══════════════════════════════════════════════════════════════
// FILE: Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

public class GLAccountDeterminationMapper : Profile
{
    public GLAccountDeterminationMapper()
    {
        CreateMap<GLAccountDetermination, GLAccountDeterminationResponse>();
        CreateMap<GLAccountDeterminationRequest, GLAccountDetermination>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}


