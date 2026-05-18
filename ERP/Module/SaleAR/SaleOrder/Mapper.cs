using AutoMapper;

namespace FarmingApi.Modules.SaleAR.SaleOrder;

public class SaleOrderMapper : Profile
{
    public SaleOrderMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<SaleOrderListRequest, SaleOrder>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleOrderLineRequest, SaleOrderLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<SaleOrder, SaleOrderListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))
            .ForMember(dest => dest.Items,        opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleOrderLine, SaleOrderLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<SaleOrderUpdateRequest, SaleOrder>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}