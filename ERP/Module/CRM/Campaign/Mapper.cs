using AutoMapper;

namespace FarmingApi.Modules.CRM.Campaign;

public class CampaignMapper : Profile
{
    public CampaignMapper()
    {
        CreateMap<Campaign, CampaignResponse>()
            .ForMember(d => d.Targets,        o => o.MapFrom(s => s.Targets))
            .ForMember(d => d.TargetCount,    o => o.MapFrom(s => s.Targets.Count))
            .ForMember(d => d.RespondedCount, o => o.MapFrom(s => s.Targets.Count(t => t.Status == "Responded" || t.Status == "Converted")))
            .ForMember(d => d.ConvertedCount, o => o.MapFrom(s => s.Targets.Count(t => t.Status == "Converted")));
        CreateMap<CampaignTarget, CampaignTargetResponse>();
    }
}
