using AutoMapper;

namespace FarmingApi.Modules.Financials.RecurringPosting;

public class RecurringPostingMapper : Profile
{
    public RecurringPostingMapper()
    {
        CreateMap<RecurringPostingLineDto, RecurringPostingLine>()
            .ForMember(d => d.AccountName, o => o.Ignore()); // filled from Chart of Accounts

        CreateMap<RecurringPostingLine, RecurringPostingLineDto>();

        CreateMap<RecurringPostingRequest, RecurringPosting>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.Code, o => o.Ignore())
            .ForMember(d => d.NextExecution, o => o.Ignore())
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));

        CreateMap<RecurringPosting, RecurringPostingResponse>()
            .ForMember(d => d.TotalDebit,  o => o.MapFrom(s => s.Lines.Sum(l => l.Debit)))
            .ForMember(d => d.TotalCredit, o => o.MapFrom(s => s.Lines.Sum(l => l.Credit)));
    }
}
