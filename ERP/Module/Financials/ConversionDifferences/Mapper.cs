using AutoMapper;

namespace FarmingApi.Modules.Financials.ConversionDifferences;

public class ConversionDifferenceMapper : Profile
{
    public ConversionDifferenceMapper()
    {
        CreateMap<ConversionDifferenceLine, ConversionDifferenceLineResponse>();

        CreateMap<ConversionDifference, ConversionDifferenceResponse>()
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count));

        // Both directions are registered: a stored run can be mapped back to a
        // request so it can be re-run against a new conversion rate.
        CreateMap<ConversionDifferenceLineResponse, ConversionDifferenceLineRequest>();
        CreateMap<ConversionDifference, ConversionDifferenceRequest>();
        CreateMap<ConversionDifferenceLine, ConversionDifferenceLineRequest>();
    }
}
