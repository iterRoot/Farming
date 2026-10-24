using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice;

public class APDownPaymentInvoiceMapper : Profile
{
    public APDownPaymentInvoiceMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<APDownPaymentInvoiceRequest, APDownPaymentInvoice>()
            .ForMember(dest => dest.Vendor,      opt => opt.Ignore())
            .ForMember(dest => dest.BaseRequest, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<APDownPaymentInvoice, APDownPaymentInvoiceResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName));


        // ── Update ────────────────────────────────────────────────────────
        CreateMap<APDownPaymentInvoiceUpdateRequest, APDownPaymentInvoice>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore());
    }
}