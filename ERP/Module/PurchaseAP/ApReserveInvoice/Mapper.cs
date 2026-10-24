using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.APReserveInvoice;

public class APReserveInvoiceMapper : Profile
{
    public APReserveInvoiceMapper()
    {
        CreateMap<APReserveInvoiceRequest, APReserveInvoice>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<APReserveInvoiceLineRequest, APReserveInvoiceLine>()
            .ForMember(dest => dest.Item,         opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,     opt => opt.Ignore())
            .ForMember(dest => dest.BaseInvoice,  opt => opt.Ignore());

        CreateMap<APReserveInvoice, APReserveInvoiceResponse>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor.CardName))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<APReserveInvoiceLine, APReserveInvoiceLineResponse>();

        CreateMap<APReserveInvoiceUpdateRequest, APReserveInvoice>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}