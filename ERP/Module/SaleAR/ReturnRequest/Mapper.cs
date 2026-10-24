using AutoMapper;

namespace FarmingApi.Modules.SaleAR.ReturnRequest;

public class ReturnRequestMapper : Profile
{
    public ReturnRequestMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<ReturnRequestListRequest, ReturnRequest>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<ReturnRequestLineRequest, ReturnRequestLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<ReturnRequest, ReturnRequestListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))
            .ForMember(dest => dest.Items,        opt => opt.MapFrom(src => src.Items));

        CreateMap<ReturnRequestLine, ReturnRequestLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<ReturnRequestUpdateRequest, ReturnRequest>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}