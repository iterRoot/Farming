using AutoMapper;

namespace FarmingApi.Modules.Sales.CommissionGroup;

public class CommissionGroupMapper : Profile
{
    public CommissionGroupMapper()
    {
        CreateMap<CommissionGroup, CommissionGroupResponse>()
            .ForMember(d => d.Tiers, opt => opt.MapFrom(s => s.Tiers));
        CreateMap<CommissionTier, CommissionTierResponse>();

        CreateMap<CommissionGroupRequest, CommissionGroup>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            .ForMember(d => d.Tiers,     opt => opt.MapFrom(s => s.Tiers));
        CreateMap<CommissionTierRequest, CommissionTier>()
            .ForMember(d => d.Id,                opt => opt.Ignore())
            .ForMember(d => d.CommissionGroupId,  opt => opt.Ignore())
            .ForMember(d => d.CommissionGroup,    opt => opt.Ignore())
            .ForMember(d => d.TierOrder,          opt => opt.Ignore());
    }
}