using AutoMapper;

namespace FarmingApi.Modules.Administration.DashboardManager;

public class DashboardManagerMapper : Profile
{
    public DashboardManagerMapper()
    {
        CreateMap<DashboardConfig, DashboardConfigResponse>()
            .ForMember(d => d.Widgets, opt => opt.MapFrom(s =>
                s.Widgets.OrderBy(w => w.Position)));

        CreateMap<DashboardWidget, DashboardWidgetResponse>();

        CreateMap<DashboardConfigRequest, DashboardConfig>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            .ForMember(d => d.Widgets,   opt => opt.MapFrom(s => s.Widgets));

        CreateMap<DashboardWidgetRequest, DashboardWidget>()
            .ForMember(d => d.Id,                opt => opt.Ignore())
            .ForMember(d => d.DashboardConfigId, opt => opt.Ignore())
            .ForMember(d => d.DashboardConfig,   opt => opt.Ignore());
    }
}