using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.ProductionOrder;

public class ProductionOrderMapper : Profile
{
    public ProductionOrderMapper()
    {
        CreateMap<ProductionOrder, ProductionOrderResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<ProductionOrderLine, ProductionOrderLineResponse>();
    }
}
