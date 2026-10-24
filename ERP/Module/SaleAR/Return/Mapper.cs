using AutoMapper;

namespace FarmingApi.Modules.SaleAR.Return;

public class ReturnMapper : Profile   // ✅ ReturnMapper
{
    public ReturnMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<ReturnListRequest, Return>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<ReturnLineRequest, ReturnLine>()  // ✅ ReturnLine
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<Return, ReturnListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<ReturnLine, ReturnLineResponse>();  // ✅ ReturnLine

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<ReturnUpdateRequest, Return>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}