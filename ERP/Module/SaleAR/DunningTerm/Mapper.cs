using AutoMapper;

namespace FarmingApi.Modules.SaleAR.DunningTerm;

public class DunningTermMapper : Profile
{
    public DunningTermMapper()
    {
        CreateMap<DunningTerm, DunningTermResponse>()
            .ForMember(d => d.Levels, o => o.MapFrom(s => s.Levels.OrderBy(l => l.Level)));
        CreateMap<DunningTermLevel, DunningTermLevelResponse>();
    }
}
