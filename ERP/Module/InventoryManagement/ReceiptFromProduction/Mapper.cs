using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.ReceiptFromProduction;

public class ReceiptFromProductionMapper : Profile
{
    public ReceiptFromProductionMapper()
    {
        CreateMap<ReceiptFromProduction, ReceiptFromProductionResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)));
        CreateMap<ReceiptFromProductionLine, ReceiptFromProductionLineResponse>();
    }
}
