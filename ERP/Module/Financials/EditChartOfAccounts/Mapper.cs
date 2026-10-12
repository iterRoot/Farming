using AutoMapper;
using FarmingApi.Modules.Financials.ChartOfAccounts;

namespace FarmingApi.Modules.Financials.EditChartOfAccounts;

public class ChartOfAccountsEditMapper : Profile
{
    public ChartOfAccountsEditMapper()
    {
        CreateMap<ChartOfAccountsEditLine, ChartOfAccountsEditLineResponse>();

        CreateMap<ChartOfAccountsEditBatch, ChartOfAccountsEditBatchResponse>()
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count));

        // The account picker projects straight from OACT.
        CreateMap<ChartOfAccounts.ChartOfAccounts, ChartOfAccountsEditCandidate>();
    }
}
