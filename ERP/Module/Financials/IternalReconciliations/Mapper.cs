using AutoMapper;

namespace FarmingApi.Modules.Financials.IternalReconciliations;

public class BPReconciliationMapper : Profile
{
    public BPReconciliationMapper()
    {
        CreateMap<BPReconciliation, BPReconciliationResponse>()
            .ForMember(d => d.Lines,          opt => opt.MapFrom(s => s.Lines))
            .ForMember(d => d.TotalLines,     opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.ReconciledLines,opt => opt.MapFrom(s => s.Lines.Count(l => l.IsReconciled)))
            .ForMember(d => d.PendingLines,   opt => opt.MapFrom(s => s.Lines.Count(l => !l.IsReconciled)));

        CreateMap<BPReconciliationLine, BPReconciliationLineResponse>();

        CreateMap<BPReconciliationLineRequest, BPReconciliationLine>()
            .ForMember(d => d.Id,                 opt => opt.Ignore())
            .ForMember(d => d.BPReconciliationId,  opt => opt.Ignore())
            .ForMember(d => d.Balance,             opt => opt.Ignore())
            .ForMember(d => d.ReconciledDate,      opt => opt.Ignore())
            .ForMember(d => d.ReconciledRef,       opt => opt.Ignore())
            .ForMember(d => d.BPReconciliation,    opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,           opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,           opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,           opt => opt.Ignore())
            .ForMember(d => d.InActive,            opt => opt.Ignore());
    }
}
