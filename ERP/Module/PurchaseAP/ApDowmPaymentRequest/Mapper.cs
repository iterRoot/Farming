using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentRequest;

public class APDownPaymentRequestMapper : Profile
{
    public APDownPaymentRequestMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<APDownPaymentRequestRequest, APDownPaymentRequest>()
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.BaseOrder, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<APDownPaymentRequest, APDownPaymentRequestResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName));


        // ── Update ────────────────────────────────────────────────────────
        CreateMap<APDownPaymentRequestUpdateRequest, APDownPaymentRequest>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore());
    }
}