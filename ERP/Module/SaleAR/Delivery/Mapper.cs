using AutoMapper;

namespace FarmingApi.Modules.SaleAR.Delivery;

public class DeliveryMapper : Profile   // ✅ DeliveryMapper
{
    public DeliveryMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<DeliveryListRequest, Delivery>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<DeliveryLineRequest, DeliveryLine>()  // ✅ DeliveryLine
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<Delivery, DeliveryListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<DeliveryLine, DeliveryLineResponse>();  // ✅ DeliveryLine

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<DeliveryUpdateRequest, Delivery>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}