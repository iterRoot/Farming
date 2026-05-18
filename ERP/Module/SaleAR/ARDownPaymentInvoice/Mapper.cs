using AutoMapper;

namespace FarmingApi.Modules.SaleAR.ARDownPaymentInvoice;

public class ARDownPaymentMapper : Profile  // ✅ renamed
{
    public ARDownPaymentMapper()
    {
        CreateMap<ARDownPaymentListRequest, ARDownPaymentInvoice>()
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.Items,    opt => opt.MapFrom(src => src.Items));

        CreateMap<ARDownPaymentLineRequest, ARDownPaymentLine>()  // ✅ renamed
            .ForMember(dest => dest.Item,     opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.Ignore())
            .ForMember(dest => dest.ItemName, opt => opt.Ignore());

        CreateMap<ARDownPaymentInvoice, ARDownPaymentListResponse>()
            .ForMember(dest => dest.CustomerId,   opt => opt.MapFrom(src => src.CustomerId))
            .ForMember(dest => dest.CustomerCode, opt => opt.MapFrom(src => src.Customer.Code))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<ARDownPaymentLine, ARDownPaymentLineResponse>();  // ✅ renamed

        CreateMap<ARDownPaymentUpdateRequest, ARDownPaymentInvoice>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Customer,  opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}