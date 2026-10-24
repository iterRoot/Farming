using AutoMapper;

namespace FarmingApi.Modules.Administration.PredefinedText;

public class PredefinedTextMapper : Profile
{
    public PredefinedTextMapper()
    {
        CreateMap<PredefinedText, PredefinedTextResponse>();
        CreateMap<PredefinedTextRequest, PredefinedText>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}