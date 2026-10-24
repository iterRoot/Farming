using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

public class GoodsReceiptPOMapper : Profile
{
    public GoodsReceiptPOMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<GoodsReceiptPORequest, GoodsReceiptPO>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Lines,  opt => opt.MapFrom(src => src.Items));

        CreateMap<GoodsReceiptPOLineRequest, GoodsReceiptPOLine>()
            .ForMember(dest => dest.Item,       opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,   opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,   opt => opt.Ignore());
            // .ForMember(dest => dest.BaseOrder,  opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<GoodsReceiptPO, GoodsReceiptPOResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Lines));

        CreateMap<GoodsReceiptPOLine, GoodsReceiptPOLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<GoodsReceiptPOUpdateRequest, GoodsReceiptPO>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Lines,     opt => opt.MapFrom(src => src.Items));
    }
}