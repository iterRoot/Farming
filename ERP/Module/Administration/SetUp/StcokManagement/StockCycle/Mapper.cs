

// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.StockCycles;

public class StockCycleMapper : Profile
{
    public StockCycleMapper()
    {
        CreateMap<StockCycle, StockCycleResponse>();
        CreateMap<StockCycleRequest, StockCycle>()
            .ForMember(d => d.Id,            opt => opt.Ignore())
            .ForMember(d => d.CompletedDate, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,     opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,     opt => opt.Ignore())
            .ForMember(d => d.InActive,      opt => opt.Ignore());
    }
}
