using AutoMapper;

namespace FarmingApi.Modules.Financials.ExchangeRateDifferences;

public class ExchangeRateDifferenceMapper : Profile
{
    public ExchangeRateDifferenceMapper()
    {
        CreateMap<ExchangeRateDifferenceLine, ExchangeRateDifferenceLineResponse>();

        CreateMap<ExchangeRateDifference, ExchangeRateDifferenceResponse>()
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count));

        // Both directions are registered: CopyFrom maps a stored run back to a
        // request so it can be re-run against a new rate.
        CreateMap<ExchangeRateDifferenceLineResponse, ExchangeRateDifferenceLineRequest>();
        CreateMap<ExchangeRateDifference, ExchangeRateDifferenceRequest>();
        CreateMap<ExchangeRateDifferenceLine, ExchangeRateDifferenceLineRequest>();
    }
}
