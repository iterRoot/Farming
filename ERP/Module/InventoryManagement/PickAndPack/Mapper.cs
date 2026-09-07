using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

public class PickPackMapper : Profile
{
    public PickPackMapper()
    {
        CreateMap<PickPackFilterDto, PickPackFilter>().ReverseMap();
        CreateMap<PickPackLine, PickPackLineDto>();

        CreateMap<PickPackCriteriaRequest, PickPackCriteria>()
            .ForMember(d => d.ManageSalesOrders,          o => o.MapFrom(s => s.Manage.SalesOrders))
            .ForMember(d => d.ManageReserveInvoices,      o => o.MapFrom(s => s.Manage.ReserveInvoices))
            .ForMember(d => d.ManageProductionOrders,     o => o.MapFrom(s => s.Manage.ProductionOrders))
            .ForMember(d => d.ManageStockTransferRequests,o => o.MapFrom(s => s.Manage.StockTransferRequests))
            .ForMember(d => d.Status, o => o.Ignore())
            .ForMember(d => d.Lines,  o => o.Ignore());

        CreateMap<PickPackCriteria, PickPackCriteriaResponse>()
            .ForMember(d => d.LineCount, o => o.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.Manage, o => o.MapFrom(s => new PickPackManageDto {
                SalesOrders = s.ManageSalesOrders, ReserveInvoices = s.ManageReserveInvoices,
                ProductionOrders = s.ManageProductionOrders, StockTransferRequests = s.ManageStockTransferRequests }));
    }
}
