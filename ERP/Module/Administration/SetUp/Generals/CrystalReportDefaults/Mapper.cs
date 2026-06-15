using AutoMapper;

namespace FarmingApi.Modules.Administration.CrystalReportDefaults;

public class CrystalReportDefaultMapper : Profile
{
    public CrystalReportDefaultMapper()
    {
        CreateMap<CrystalReportDefault, CrystalReportDefaultResponse>();
        CreateMap<CrystalReportDefaultRequest, CrystalReportDefault>()
            .ForMember(d => d.Id,              opt => opt.Ignore())
            .ForMember(d => d.IsSystemElement, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,       opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,       opt => opt.Ignore())
            .ForMember(d => d.InActive,        opt => opt.Ignore());
    }
}