using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement;

public class PurchaseBlanketAgreementMapper : Profile
{
    public PurchaseBlanketAgreementMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<PurchaseBlanketAgreementRequest, PurchaseBlanketAgreement>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseBlanketAgreementLineRequest, PurchaseBlanketAgreementLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<PurchaseBlanketAgreement, PurchaseBlanketAgreementResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseBlanketAgreementLine, PurchaseBlanketAgreementLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<PurchaseBlanketAgreementUpdateRequest, PurchaseBlanketAgreement>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}