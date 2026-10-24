using AutoMapper;

namespace FarmingApi.Modules.Inventory.GoodsIssue;

public class GoodsIssueMapper : Profile
{
    public GoodsIssueMapper()
    {
        // ── Read ──────────────────────────────────────────────
        CreateMap<GoodsIssue, GoodsIssueListResponse>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        CreateMap<GoodsIssueLine, GoodsIssueLineResponse>();

        // ── Write ─────────────────────────────────────────────
        CreateMap<GoodsIssueInsertRequest, GoodsIssue>()
            .ForMember(d => d.DocumentDate,        opt => opt.MapFrom(s => s.DocDate ?? DateTime.UtcNow))
            .ForMember(d => d.PostingDate,         opt => opt.MapFrom(s => s.PostingDate ?? DateTime.UtcNow))
            .ForMember(d => d.DueDate,             opt => opt.MapFrom(s => s.DocDate ?? DateTime.UtcNow))
            .ForMember(d => d.GrandTotal,          opt => opt.MapFrom(s => s.TotalAmount))
            .ForMember(d => d.TotalBeforeDiscount, opt => opt.MapFrom(s => s.TotalAmount - s.TaxAmount + s.Discount))
            .ForMember(d => d.CustomerCode,        opt => opt.MapFrom(s => s.CustomerCode ?? ""))
            .ForMember(d => d.CustomerName,        opt => opt.MapFrom(s => s.CustomerName ?? ""))
            .ForMember(d => d.Lines,               opt => opt.MapFrom(s => s.Lines))
            .ForMember(d => d.Id,                  opt => opt.Ignore())
            .ForMember(d => d.DocNo,               opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,           opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,           opt => opt.Ignore())
            .ForMember(d => d.InActive,            opt => opt.Ignore());

        CreateMap<GoodsIssueLineRequest, GoodsIssueLine>()
            .ForMember(d => d.Qty,      opt => opt.MapFrom(s => s.Quantity))
            .ForMember(d => d.ItemCode, opt => opt.MapFrom(s => s.ItemCode ?? ""))
            .ForMember(d => d.ItemName, opt => opt.MapFrom(s => s.ItemName ?? ""))
            .ForMember(d => d.Id,       opt => opt.Ignore());

        // Update reuses insert map
        CreateMap<GoodsIssueUpdateRequest, GoodsIssue>()
            .IncludeBase<GoodsIssueInsertRequest, GoodsIssue>();
    }
}