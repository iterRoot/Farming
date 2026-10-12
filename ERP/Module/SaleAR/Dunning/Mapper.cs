using AutoMapper;

namespace FarmingApi.Modules.SaleAR.Dunning;

public class DunningMapper : Profile
{
    public DunningMapper()
    {
        CreateMap<DunningLetter, DunningLetterResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<DunningLetterLine, DunningLetterLineResponse>()
            .ForMember(d => d.InvoiceId, o => o.MapFrom(s => s.ARInvoiceId));
    }
}
