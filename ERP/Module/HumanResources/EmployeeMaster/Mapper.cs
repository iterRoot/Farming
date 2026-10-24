using AutoMapper;

namespace FarmingApi.Modules.HumanResources.EmployeeMaster;

public class EmployeeMasterMapper : Profile
{
    public EmployeeMasterMapper()
    {
        CreateMap<EmployeeMaster, EmployeeMasterResponse>()
            .ForMember(d => d.FullName,
                opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()));

        CreateMap<EmployeeMasterRequest, EmployeeMaster>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.Code,      opt => opt.Ignore())   // controller normalises + sets
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}
