using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.PurchaseOrder;

public class PurchaseOrderMapper : Profile
{
    public PurchaseOrderMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<PurchaseOrderRequest, PurchaseOrder>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Lines,  opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseOrderLineRequest, PurchaseOrderLine>()
            .ForMember(dest => dest.Item,            opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,        opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,        opt => opt.Ignore())
            .ForMember(dest => dest.BaseQuotation,   opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<PurchaseOrder, PurchaseOrderResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Lines));

        CreateMap<PurchaseOrderLine, PurchaseOrderLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<PurchaseOrderUpdateRequest, PurchaseOrder>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Lines,     opt => opt.MapFrom(src => src.Items));
    }
}