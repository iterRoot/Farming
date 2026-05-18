using AutoMapper;

namespace FarmingApi.Modules.SaleAR.ARReserveInvoice;

public class ARReserveInvoiceMapper : Profile
{
    public ARReserveInvoiceMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<ARReserveInvoiceRequest, ARReserveInvoice>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<ARReserveInvoiceLineRequest, ARReserveInvoiceLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<ARReserveInvoice, ARReserveInvoiceResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<ARReserveInvoiceLine, ARReserveInvoiceLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<ARReserveInvoiceUpdateRequest, ARReserveInvoice>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}