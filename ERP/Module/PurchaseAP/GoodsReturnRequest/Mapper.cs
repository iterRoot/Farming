using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturnRequest;

public class GoodsReturnRequestMapper : Profile
{
    public GoodsReturnRequestMapper()
    {
        CreateMap<GoodsReturnRequestRequest, GoodsReturnRequest>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<GoodsReturnRequestLineRequest, GoodsReturnRequestLine>()
            .ForMember(dest => dest.Item,             opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,         opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,         opt => opt.Ignore())
            .ForMember(dest => dest.BaseGoodsReceipt, opt => opt.Ignore());

        CreateMap<GoodsReturnRequest, GoodsReturnRequestResponse>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<GoodsReturnRequestLine, GoodsReturnRequestLineResponse>();

        CreateMap<GoodsReturnRequestUpdateRequest, GoodsReturnRequest>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}