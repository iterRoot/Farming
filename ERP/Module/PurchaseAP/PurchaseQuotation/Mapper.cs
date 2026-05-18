using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.PurchaseQuotation;

public class PurchaseQuotationMapper : Profile
{
    public PurchaseQuotationMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<PurchaseQuotationRequest, PurchaseQuotation>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseQuotationLineRequest, PurchaseQuotationLine>()
            .ForMember(dest => dest.Item,                  opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,              opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,              opt => opt.Ignore())
            .ForMember(dest => dest.BaseBlanketAgreement,  opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<PurchaseQuotation, PurchaseQuotationResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseQuotationLine, PurchaseQuotationLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<PurchaseQuotationUpdateRequest, PurchaseQuotation>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}