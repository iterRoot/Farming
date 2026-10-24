using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.IssueForProduction;

public class IssueForProductionMapper : Profile
{
    public IssueForProductionMapper()
    {
        CreateMap<IssueForProduction, IssueForProductionResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)));
        CreateMap<IssueForProductionLine, IssueForProductionLineResponse>();
    }
}
