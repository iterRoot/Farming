// ═══════════════════════════════════════════════════════════════
// FILE: Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace KamnttraApi.Modules.Financials.GLAccountDetermination;

public class GLAccountDeterminationMapper : Profile
{
    public GLAccountDeterminationMapper()
    {
        CreateMap<GLAccountDetermination, GLAccountDeterminationResponse>();
        CreateMap<GLAccountDeterminationRequest, GLAccountDetermination>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            // A field the caller didn't send (null) leaves the saved account alone; an explicit ""
            // clears it. Without this an empty or partial request blanked every mapping, and
            // invoices then stopped posting journal entries.
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}


