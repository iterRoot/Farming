using AutoMapper;

namespace FarmingApi.Modules.Administration.ApprovalRequest;

public class ApprovalRequestMapper : Profile
{
    public ApprovalRequestMapper()
    {
        CreateMap<ApprovalRequest, ApprovalRequestResponse>()
            .ForMember(dest => dest.ApprovalTemplateName,
                opt => opt.MapFrom(src => src.ApprovalTemplate != null ? src.ApprovalTemplate.Name : ""))
            .ForMember(dest => dest.RequestedByName,
                opt => opt.MapFrom(src => src.RequestedByUser != null ? src.RequestedByUser.UserName : ""))
            .ForMember(dest => dest.DecidedByName,
                opt => opt.MapFrom(src => src.DecidedByUser != null ? src.DecidedByUser.UserName : null));
    }
}
