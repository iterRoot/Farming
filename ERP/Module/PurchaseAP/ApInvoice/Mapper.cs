using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.APInvoice;

public class APInvoiceMapper : Profile
{
    public APInvoiceMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<APInvoiceRequest, APInvoice>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<APInvoiceLineRequest, APInvoiceLine>()
            .ForMember(dest => dest.Item,                 opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,             opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,             opt => opt.Ignore())
            .ForMember(dest => dest.BaseGoodsReceipt,     opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<APInvoice, APInvoiceResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<APInvoiceLine, APInvoiceLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<APInvoiceUpdateRequest, APInvoice>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}