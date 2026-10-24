using AutoMapper;

namespace FarmingApi.Modules.Administration.SetUp.Currencies;

public class CurrenciesMapper : Profile
{
    public CurrenciesMapper()
    {
        CreateMap<Currencies, CurrencyResponse>();

        CreateMap<CurrencyRequest, Currencies>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.Code.Trim().ToUpper()))
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());
    }
}
