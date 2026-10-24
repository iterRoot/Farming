using AutoMapper;

namespace FarmingApi.Modules.Administration.ServerPrintConfig;

public class ServerPrintConfigMapper : Profile
{
    public ServerPrintConfigMapper()
    {
        CreateMap<ServerPrintConfig, ServerPrintConfigResponse>()
            .ForMember(d => d.PrintPassword,
                opt => opt.MapFrom(s =>
                    !string.IsNullOrEmpty(s.PrintPassword) ? "••••••••" : null))
            .ForMember(d => d.Routes,
                opt => opt.MapFrom(s => s.Routes.OrderBy(r => r.SortOrder)));

        CreateMap<PrintDocumentRoute, PrintDocumentRouteResponse>();

        CreateMap<ServerPrintConfigRequest, ServerPrintConfig>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            .ForMember(d => d.Routes,    opt => opt.MapFrom(s => s.Routes))
            .ForMember(d => d.PrintPassword, opt => opt.Condition(
                (src, _, _) => src.PrintPassword != null && src.PrintPassword != "••••••••"));

        CreateMap<PrintDocumentRouteRequest, PrintDocumentRoute>()
            .ForMember(d => d.Id,                  opt => opt.Ignore())
            .ForMember(d => d.ServerPrintConfigId, opt => opt.Ignore())
            .ForMember(d => d.ServerPrintConfig,   opt => opt.Ignore());
    }
}