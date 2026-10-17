using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.Forecast;

public class ForecastMapper : Profile
{
    public ForecastMapper()
    {
        CreateMap<Forecast, ForecastResponse>()
            .ForMember(d => d.Lines,    o => o.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)))
            .ForMember(d => d.TotalQty, o => o.MapFrom(s => s.Lines.Sum(l => l.Quantity)));
        CreateMap<ForecastLine, ForecastLineResponse>();
    }
}
