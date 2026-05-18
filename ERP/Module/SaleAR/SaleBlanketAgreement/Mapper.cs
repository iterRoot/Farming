using AutoMapper;

namespace FarmingApi.Modules.SaleAR.SaleBlanketAgreement;

public class SaleBlanketAgreementMapper : Profile
{
    public SaleBlanketAgreementMapper()
    {
        // ── Create ────────────────────────────────────────────────────────
        CreateMap<SaleBlanketAgreementListRequest, SaleBlanketAgreement>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleBlanketAgreementLineRequest, SaleBlanketAgreementLine>()
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        // ── Read ──────────────────────────────────────────────────────────
        CreateMap<SaleBlanketAgreement, SaleBlanketAgreementListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))
            .ForMember(dest => dest.Items,        opt => opt.MapFrom(src => src.Items));

        CreateMap<SaleBlanketAgreementLine, SaleBlanketAgreementLineResponse>();

        // ── Update ────────────────────────────────────────────────────────
        CreateMap<SaleBlanketAgreementUpdateRequest, SaleBlanketAgreement>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}