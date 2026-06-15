using AutoMapper;

namespace FarmingApi.Modules.Administration.AddressFormats;

public class AddressFormatMapper : Profile
{
    public AddressFormatMapper()
    {
        CreateMap<AddressFormat, AddressFormatResponse>()
            .ForMember(d => d.Lines,
                opt => opt.MapFrom(s => s.Lines.OrderBy(l => l.LineOrder)));

        CreateMap<AddressFormatLine, AddressFormatLineResponse>();

        CreateMap<AddressFormatRequest, AddressFormat>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            .ForMember(d => d.Lines,     opt => opt.MapFrom(s => s.Lines));

        CreateMap<AddressFormatLineRequest, AddressFormatLine>()
            .ForMember(d => d.Id,              opt => opt.Ignore())
            .ForMember(d => d.AddressFormatId, opt => opt.Ignore())
            .ForMember(d => d.AddressFormat,   opt => opt.Ignore());
    }
}