

// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.StockCycles;

public class StockCycleMapper : Profile
{
    public StockCycleMapper()
    {
        CreateMap<StockCycle, StockCycleResponse>()
            .ForMember(d => d.LineCount,    opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.CountedCount, opt => opt.MapFrom(s => s.Lines.Count(l => l.CountedQty != null)));
        CreateMap<StockCycleRequest, StockCycle>()
            .ForMember(d => d.Id,            opt => opt.Ignore())
            .ForMember(d => d.Lines,         opt => opt.Ignore())
            .ForMember(d => d.JournalEntryId, opt => opt.Ignore())
            .ForMember(d => d.CompletedDate, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,     opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,     opt => opt.Ignore())
            .ForMember(d => d.InActive,      opt => opt.Ignore());
    }
}
