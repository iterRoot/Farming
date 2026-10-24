using AutoMapper;

namespace FarmingApi.Modules.Inventory.StockTransferRequest;

public class StockTransferRequestMapper : Profile
{
    public StockTransferRequestMapper()
    {
        CreateMap<StockTransferRequest, StockTransferRequestResponse>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        CreateMap<StockTransferRequestLine, StockTransferRequestLineResponse>();

        CreateMap<StockTransferRequestCreateRequest, StockTransferRequest>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.DocNo,          opt => opt.Ignore())
            .ForMember(d => d.Status,         opt => opt.Ignore())
            .ForMember(d => d.TotalQuantity,  opt => opt.Ignore())
            .ForMember(d => d.VersionNum,     opt => opt.Ignore())
            .ForMember(d => d.UserSign,       opt => opt.Ignore())
            .ForMember(d => d.Lines,          opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,      opt => opt.Ignore())
            .ForMember(d => d.InActive,       opt => opt.Ignore());

        CreateMap<StockTransferRequestUpdateRequest, StockTransferRequest>()
            .ForMember(d => d.Id,             opt => opt.Ignore())
            .ForMember(d => d.DocNo,          opt => opt.Ignore())
            .ForMember(d => d.Series,         opt => opt.Ignore())
            .ForMember(d => d.Status,         opt => opt.Ignore())
            .ForMember(d => d.TotalQuantity,  opt => opt.Ignore())
            .ForMember(d => d.VersionNum,     opt => opt.Ignore())
            .ForMember(d => d.UserSign,       opt => opt.Ignore())
            .ForMember(d => d.Lines,          opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,      opt => opt.Ignore())
            .ForMember(d => d.InActive,       opt => opt.Ignore());

        CreateMap<StockTransferRequestLineRequest, StockTransferRequestLine>()
            .ForMember(d => d.Id,                      opt => opt.Ignore())
            .ForMember(d => d.StockTransferRequestId,  opt => opt.Ignore())
            .ForMember(d => d.LineTotal,               opt => opt.Ignore())
            .ForMember(d => d.LineStatus,              opt => opt.Ignore())
            .ForMember(d => d.StockTransferRequest,    opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,               opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,               opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,               opt => opt.Ignore())
            .ForMember(d => d.InActive,                opt => opt.Ignore());
    }
}