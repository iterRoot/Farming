using AutoMapper;

namespace FarmingApi.Modules.CRM.Opportunity;

public class OpportunityMapper : Profile
{
    public OpportunityMapper()
    {
        CreateMap<Opportunity, OpportunityResponse>()
            .ForMember(d => d.InterestRanges, o => o.MapFrom(s => s.InterestRanges.OrderBy(x => x.LineNum)))
            .ForMember(d => d.Stages,         o => o.MapFrom(s => s.Stages.OrderBy(x => x.LineNum)))
            .ForMember(d => d.Partners,       o => o.MapFrom(s => s.Partners.OrderBy(x => x.LineNum)))
            .ForMember(d => d.Competitors,    o => o.MapFrom(s => s.Competitors.OrderBy(x => x.LineNum)))
            .ForMember(d => d.Reasons,        o => o.MapFrom(s => s.Reasons.OrderBy(x => x.LineNum)))
            .ForMember(d => d.Attachments,    o => o.MapFrom(s => s.Attachments.OrderBy(x => x.LineNum)));

        CreateMap<OpportunityInterest,   OpportunityInterestDto>();
        CreateMap<OpportunityStage,      OpportunityStageDto>();
        CreateMap<OpportunityPartner,    OpportunityPartnerDto>();
        CreateMap<OpportunityCompetitor, OpportunityCompetitorDto>();
        CreateMap<OpportunityReason,     OpportunityReasonDto>();
        CreateMap<OpportunityAttachment, OpportunityAttachmentDto>();

        CreateMap<OpportunityRequest, Opportunity>()
            .ForMember(d => d.Id,                  o => o.Ignore())
            .ForMember(d => d.OpportunityNo,        o => o.Ignore())
            .ForMember(d => d.Status,               o => o.Ignore())
            .ForMember(d => d.TotalAmountInvoiced,  o => o.Ignore())
            .ForMember(d => d.WeightedAmount,       o => o.Ignore())
            .ForMember(d => d.GrossProfitTotal,     o => o.Ignore())
            .ForMember(d => d.CreatedAt,            o => o.Ignore())
            .ForMember(d => d.UpdatedAt,            o => o.Ignore())
            .ForMember(d => d.InActive,             o => o.Ignore())
            .ForMember(d => d.InterestRanges, o => o.MapFrom(s => s.InterestRanges))
            .ForMember(d => d.Stages,         o => o.MapFrom(s => s.Stages))
            .ForMember(d => d.Partners,       o => o.MapFrom(s => s.Partners))
            .ForMember(d => d.Competitors,    o => o.MapFrom(s => s.Competitors))
            .ForMember(d => d.Reasons,        o => o.MapFrom(s => s.Reasons))
            .ForMember(d => d.Attachments,    o => o.MapFrom(s => s.Attachments));

        CreateMap<OpportunityInterestRequest, OpportunityInterest>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());

        CreateMap<OpportunityStageRequest, OpportunityStage>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.WeightedAmount, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());

        CreateMap<OpportunityPartnerRequest, OpportunityPartner>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());

        CreateMap<OpportunityCompetitorRequest, OpportunityCompetitor>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());

        CreateMap<OpportunityReasonRequest, OpportunityReason>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());

        CreateMap<OpportunityAttachmentRequest, OpportunityAttachment>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.OpportunityId, o => o.Ignore())
            .ForMember(d => d.Opportunity, o => o.Ignore());
    }
}