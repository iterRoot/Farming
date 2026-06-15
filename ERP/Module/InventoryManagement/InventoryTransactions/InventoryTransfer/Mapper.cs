using AutoMapper;

namespace FarmingApi.Modules.Inventory.StockTransfer;

public class StockTransferMapper : Profile
{
    public StockTransferMapper()
    {
        CreateMap<StockTransfer, StockTransferResponse>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        CreateMap<StockTransferLine, StockTransferLineResponse>();

        CreateMap<StockTransferCreateRequest, StockTransfer>()
            .ForMember(d => d.Id,           opt => opt.Ignore())
            .ForMember(d => d.Number,       opt => opt.Ignore())
            .ForMember(d => d.DocNo,        opt => opt.Ignore())
            .ForMember(d => d.TotalQuantity,opt => opt.Ignore())
            .ForMember(d => d.VersionNum,   opt => opt.Ignore())
            .ForMember(d => d.UserSign,     opt => opt.Ignore())
            .ForMember(d => d.Lines,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,    opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,    opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,    opt => opt.Ignore())
            .ForMember(d => d.InActive,     opt => opt.Ignore());

        CreateMap<StockTransferUpdateRequest, StockTransfer>()
            .ForMember(d => d.Id,           opt => opt.Ignore())
            .ForMember(d => d.Number,       opt => opt.Ignore())
            .ForMember(d => d.DocNo,        opt => opt.Ignore())
            .ForMember(d => d.Series,       opt => opt.Ignore())
            .ForMember(d => d.TotalQuantity,opt => opt.Ignore())
            .ForMember(d => d.VersionNum,   opt => opt.Ignore())
            .ForMember(d => d.UserSign,     opt => opt.Ignore())
            .ForMember(d => d.BaseDocEntry, opt => opt.Ignore())
            .ForMember(d => d.BaseDocType,  opt => opt.Ignore())
            .ForMember(d => d.Lines,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,    opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,    opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,    opt => opt.Ignore())
            .ForMember(d => d.InActive,     opt => opt.Ignore());

        CreateMap<StockTransferLineRequest, StockTransferLine>()
            .ForMember(d => d.Id,              opt => opt.Ignore())
            .ForMember(d => d.StockTransferId, opt => opt.Ignore())
            .ForMember(d => d.LineTotal,       opt => opt.Ignore())
            .ForMember(d => d.StockTransfer,   opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,       opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,       opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,       opt => opt.Ignore())
            .ForMember(d => d.InActive,        opt => opt.Ignore());
    }
}