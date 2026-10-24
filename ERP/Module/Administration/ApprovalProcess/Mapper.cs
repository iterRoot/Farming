using AutoMapper;

namespace FarmingApi.Modules.Administration.ApprovalProcess;

public class ApprovalProcessMapper : Profile
{
    public ApprovalProcessMapper()
    {
        // ── Create / Update ──────────────────────────────────────────────────
        CreateMap<ApprovalTemplateRequest, ApprovalTemplate>()
            .ForMember(dest => dest.Approvers, opt => opt.Ignore());

        CreateMap<ApprovalTemplateUpdateRequest, ApprovalTemplate>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Approvers, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────────
        CreateMap<ApprovalTemplate, ApprovalTemplateResponse>()
            .ForMember(dest => dest.Approvers, opt => opt.MapFrom(src => src.Approvers));

        CreateMap<ApprovalTemplateApprover, ApprovalTemplateApproverResponse>()
            .ForMember(dest => dest.UserId,   opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : ""));
    }
}
