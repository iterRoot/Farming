using AutoMapper;

namespace FarmingApi.Modules.HumanResources.LeaveRequest;

public class LeaveRequestMapper : Profile
{
    public LeaveRequestMapper()
    {
        CreateMap<LeaveRequest, LeaveRequestResponse>()
            .ForMember(d => d.Paid, o => o.MapFrom(s => LeaveMath.IsPaid(s.LeaveType)));

        CreateMap<LeaveRequestRequest, LeaveRequest>()
            .ForMember(d => d.Id,           opt => opt.Ignore())
            .ForMember(d => d.RequestNo,    opt => opt.Ignore())
            .ForMember(d => d.Days,         opt => opt.Ignore())   // computed
            .ForMember(d => d.Status,       opt => opt.Ignore())
            .ForMember(d => d.DecidedBy,    opt => opt.Ignore())
            .ForMember(d => d.DecidedAt,    opt => opt.Ignore())
            .ForMember(d => d.DecisionNote, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,    opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,    opt => opt.Ignore())
            .ForMember(d => d.InActive,     opt => opt.Ignore());
    }
}
