using AutoMapper;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccountsMapper : Profile
{
    public ChartOfAccountsMapper()
    {
        CreateMap<ChartOfAccounts, ChartOfAccountsResponse>();

        CreateMap<ChartOfAccountsCreateRequest, ChartOfAccounts>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Balance, opt => opt.Ignore())
            .ForMember(d => d.DebitBalance, opt => opt.Ignore())
            .ForMember(d => d.CreditBalance, opt => opt.Ignore())
            .ForMember(d => d.AllowPosting, opt => opt.MapFrom(s => !s.BlockManualPosting));

        CreateMap<ChartOfAccountsUpdateRequest, ChartOfAccounts>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.AcctCode, opt => opt.Ignore())
            .ForMember(d => d.Level, opt => opt.Ignore())
            .ForMember(d => d.FatherNum, opt => opt.Ignore())
            .ForMember(d => d.AcctType, opt => opt.Ignore())
            .ForMember(d => d.AllowPosting, opt => opt.MapFrom(s => !s.BlockManualPosting));
    }
}