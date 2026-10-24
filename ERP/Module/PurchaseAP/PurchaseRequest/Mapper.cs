using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.PurchaseRequest;

public class PurchaseRequestMapper : Profile
{
    public PurchaseRequestMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<PurchaseRequestRequest, PurchaseRequest>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseRequestLineRequest, PurchaseRequestLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        // Vendor is optional, so VendorCode/VendorName resolve to null when
        // no vendor has been chosen yet — AutoMapper null-checks each step
        // of a MapFrom member path, so src.Vendor.Code is safe here.
        CreateMap<PurchaseRequest, PurchaseRequestResponse>()
            .ForMember(dest => dest.VendorId,   opt => opt.MapFrom(src => src.VendorId))
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.Code : null))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.CardName : null))
            .ForMember(dest => dest.Items,      opt => opt.MapFrom(src => src.Items));

        CreateMap<PurchaseRequestLine, PurchaseRequestLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<PurchaseRequestUpdateRequest, PurchaseRequest>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}
