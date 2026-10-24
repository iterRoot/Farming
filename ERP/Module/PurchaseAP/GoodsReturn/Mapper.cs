using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.GoodsReturn;

public class GoodsReturnMapper : Profile
{
    public GoodsReturnMapper()
    {
        CreateMap<GoodsReturnRequest, GoodsReturn>()
            .ForMember(dest => dest.Vendor, opt => opt.Ignore())
            .ForMember(dest => dest.Items,  opt => opt.MapFrom(src => src.Items));

        CreateMap<GoodsReturnLineRequest, GoodsReturnLine>()
            .ForMember(dest => dest.Item,              opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode,          opt => opt.Ignore())
            .ForMember(dest => dest.ItemName,          opt => opt.Ignore())
            .ForMember(dest => dest.BaseGoodsReceipt,  opt => opt.Ignore());

        CreateMap<GoodsReturn, GoodsReturnResponse>()
            .ForMember(dest => dest.VendorCode, opt => opt.MapFrom(src => src.Vendor.Code))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src =>src.Vendor.CardName))

            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        CreateMap<GoodsReturnLine, GoodsReturnLineResponse>();

        CreateMap<GoodsReturnUpdateRequest, GoodsReturn>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Vendor,    opt => opt.Ignore())
            .ForMember(dest => dest.Items,     opt => opt.MapFrom(src => src.Items));
    }
}