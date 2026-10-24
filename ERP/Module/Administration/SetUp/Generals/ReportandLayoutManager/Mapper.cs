using AutoMapper;

namespace FarmingApi.Modules.Administration.ReportLayoutManager;

public class ReportLayoutMapper : Profile
{
    public ReportLayoutMapper()
    {
        CreateMap<ReportLayout, ReportLayoutResponse>()
            .ForMember(d => d.Parameters,
                opt => opt.MapFrom(s => s.Parameters.OrderBy(p => p.ParamOrder)));

        CreateMap<ReportParameter, ReportParameterResponse>();

        CreateMap<ReportLayoutRequest, ReportLayout>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
            .ForMember(d => d.InActive,       opt => opt.Ignore())
            .ForMember(d => d.IsSystemLayout, opt => opt.Ignore())
            .ForMember(d => d.FileHash,       opt => opt.Ignore())
            .ForMember(d => d.FileSizeBytes,  opt => opt.Ignore())
            .ForMember(d => d.LastUploadedAt, opt => opt.Ignore())
            .ForMember(d => d.Parameters,     opt => opt.MapFrom(s => s.Parameters));

        CreateMap<ReportParameterRequest, ReportParameter>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.ReportLayoutId, opt => opt.Ignore())
            .ForMember(d => d.ReportLayout,   opt => opt.Ignore());
    }
}