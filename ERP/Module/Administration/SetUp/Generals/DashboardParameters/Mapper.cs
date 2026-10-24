using AutoMapper;

namespace FarmingApi.Modules.Administration.DashboardParameters;

public class DashboardParameterMapper : Profile
{
    public DashboardParameterMapper()
    {
        CreateMap<DashboardParameter, DashboardParameterResponse>();

        CreateMap<DashboardParameterRequest, DashboardParameter>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}