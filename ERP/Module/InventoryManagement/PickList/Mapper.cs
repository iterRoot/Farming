using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.PickList;

public class PickListMapper : Profile
{
    public PickListMapper()
    {
        CreateMap<PickListLine, PickListLineDto>();

        CreateMap<PickList, PickListResponse>()
            .ForMember(d => d.LineCount, opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.Lines,     opt => opt.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)));

        CreateMap<PickListLineRequest, PickListLine>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.PickListId, opt => opt.Ignore())
            .ForMember(d => d.LineNum,    opt => opt.Ignore())
            .ForMember(d => d.PickList,   opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());
    }
}
