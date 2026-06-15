using AutoMapper;

namespace FarmingApi.Modules.BusinessPartners.BPProperties;

public class BPPropertyMapper : Profile
{
    public BPPropertyMapper()
    {
        CreateMap<BPProperty, BPPropertyResponse>();
        CreateMap<BPPropertyRequest, BPProperty>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}