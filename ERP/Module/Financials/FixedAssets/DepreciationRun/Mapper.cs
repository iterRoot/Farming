using AutoMapper;

namespace FarmingApi.Modules.Financials.FixedAssets.DepreciationRun;

public class DepreciationRunMapper : Profile
{
    public DepreciationRunMapper()
    {
        CreateMap<DepreciationRun, DepreciationRunResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)));
        CreateMap<DepreciationRunLine, DepreciationRunLineResponse>();
    }
}
