using AutoMapper;

namespace FarmingApi.Modules.PurchaseAP.LandingCost;

public class LandedCostMapper : Profile
{
    public LandedCostMapper()
    {
        CreateMap<LandedCost, LandedCostResponse>()
            .ForMember(d => d.VendorName, opt => opt.MapFrom(s => s.Vendor != null ? s.Vendor.CardName : null))
            .ForMember(d => d.BaseDocs,  opt => opt.MapFrom(s => s.BaseDocs))
            .ForMember(d => d.CostLines, opt => opt.MapFrom(s => s.CostLines));

        CreateMap<LandedCostBaseDoc, LandedCostBaseDocResponse>()
            .ForMember(d => d.DocNum, opt => opt.MapFrom(s => s.GoodsReceiptPO != null ? s.GoodsReceiptPO.DocNum : null))
            .ForMember(d => d.Total,  opt => opt.MapFrom(s => s.GoodsReceiptPO != null ? (decimal?)s.GoodsReceiptPO.Total : null));

        CreateMap<LandedCostLine, LandedCostLineResponse>();
    }
}
