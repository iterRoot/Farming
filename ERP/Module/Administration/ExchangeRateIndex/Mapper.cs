using AutoMapper;

namespace FarmingApi.Modules.Financials.ExchangeRate;

public class ExchangeRateMapper : Profile
{
    public ExchangeRateMapper()
    {
        CreateMap<ExchangeRate, ExchangeRateResponse>();
        CreateMap<ExchangeRateRequest, ExchangeRate>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());

        CreateMap<PriceIndex, PriceIndexResponse>();
        CreateMap<PriceIndexRequest, PriceIndex>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}