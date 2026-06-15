using AutoMapper;

namespace FarmingApi.Modules.Administration.SAPLinks;

public class SAPLinkMapper : Profile
{
    public SAPLinkMapper()
    {
        CreateMap<SAPLink, SAPLinkResponse>()
            .ForMember(d => d.IsExpired,
                opt => opt.MapFrom(s =>
                    s.ExpiresAt.HasValue && s.ExpiresAt.Value < DateTime.UtcNow));

        CreateMap<SAPLinkUsage, SAPLinkUsageResponse>();

        CreateMap<SAPLinkRequest, SAPLink>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.ClickCount,     opt => opt.Ignore())
            .ForMember(d => d.LastClickedAt,  opt => opt.Ignore())
            .ForMember(d => d.LastStatusCode, opt => opt.Ignore())
            .ForMember(d => d.LastCheckedAt,  opt => opt.Ignore())
            .ForMember(d => d.IsHealthy,      opt => opt.Ignore())
            .ForMember(d => d.Usages,         opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
            .ForMember(d => d.InActive,       opt => opt.Ignore());
    }
}