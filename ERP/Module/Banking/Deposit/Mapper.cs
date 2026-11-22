using AutoMapper;

namespace KamnttraApi.Modules.Banking.Deposit;

public class DepositMapper : Profile
{
    public DepositMapper()
    {
        CreateMap<Deposit, DepositResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<DepositLine, DepositLineResponse>();
    }
}
