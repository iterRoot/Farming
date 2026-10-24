using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.StockRevaluation;

public class StockRevaluationMapper : Profile
{
    public StockRevaluationMapper()
    {
        CreateMap<StockRevaluationLine, StockRevaluationLineDto>();
        CreateMap<StockRevaluationSerialLine, StockRevaluationSerialLineDto>();

        CreateMap<StockRevaluation, StockRevaluationListResponse>()
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.TotalVariance, opt => opt.MapFrom(s =>
                s.Lines.Sum(l => (l.NewCost - l.CurrentCost) * l.InStock)));

        CreateMap<StockRevaluation, StockRevaluationDetailResponse>()
            .ForMember(d => d.Lines,            opt => opt.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)))
            .ForMember(d => d.SerialBatchLines, opt => opt.MapFrom(s => s.SerialBatchLines.OrderBy(l => l.LineNum)))
            .ForMember(d => d.TotalVariance,    opt => opt.MapFrom(s =>
                s.Lines.Sum(l => (l.NewCost - l.CurrentCost) * l.InStock)));

        CreateMap<StockRevaluationLineRequest, StockRevaluationLine>()
            .ForMember(d => d.Id,                 opt => opt.Ignore())
            .ForMember(d => d.StockRevaluationId, opt => opt.Ignore())
            .ForMember(d => d.LineNum,            opt => opt.Ignore())
            .ForMember(d => d.StockRevaluation,   opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,          opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,          opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,          opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,          opt => opt.Ignore())
            .ForMember(d => d.InActive,           opt => opt.Ignore());

        CreateMap<StockRevaluationSerialLineRequest, StockRevaluationSerialLine>()
            .ForMember(d => d.Id,                 opt => opt.Ignore())
            .ForMember(d => d.StockRevaluationId, opt => opt.Ignore())
            .ForMember(d => d.LineNum,            opt => opt.Ignore())
            .ForMember(d => d.StockRevaluation,   opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,          opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,          opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,          opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,          opt => opt.Ignore())
            .ForMember(d => d.InActive,           opt => opt.Ignore());
    }
}
