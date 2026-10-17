using AutoMapper;

namespace FarmingApi.Modules.Financials.PostingTemplate;

public class PostingTemplateMapper : Profile
{
    public PostingTemplateMapper()
    {
        CreateMap<PostingTemplate, PostingTemplateResponse>()
            .ForMember(d => d.Lines,     o => o.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)))
            .ForMember(d => d.DebitPct,  o => o.MapFrom(s => s.Lines.Where(l => l.Side == "D").Sum(l => l.Percentage)))
            .ForMember(d => d.CreditPct, o => o.MapFrom(s => s.Lines.Where(l => l.Side == "C").Sum(l => l.Percentage)));

        CreateMap<PostingTemplateLine, PostingTemplateLineResponse>();
    }
}
