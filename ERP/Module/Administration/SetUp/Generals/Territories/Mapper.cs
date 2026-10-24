using AutoMapper;

namespace FarmingApi.Modules.Sales.Territories;

public class TerritoryMapper : Profile
{
    public TerritoryMapper()
    {
        CreateMap<Territory, TerritoryResponse>()
            .ForMember(d => d.ChildCount,
                opt => opt.MapFrom(s => s.Children.Count))
            .ForMember(d => d.Children,
                opt => opt.MapFrom(s => s.Children));

        CreateMap<Territory, TerritoryFlatResponse>()
            .ForMember(d => d.ChildCount,
                opt => opt.MapFrom(s => s.Children.Count))
            .ForMember(d => d.FullPath,
                opt => opt.Ignore());   // computed in controller

        CreateMap<TerritoryRequest, Territory>()
            .ForMember(d => d.Id,          opt => opt.Ignore())
            .ForMember(d => d.ParentCode,  opt => opt.Ignore())
            .ForMember(d => d.ParentName,  opt => opt.Ignore())
            .ForMember(d => d.Parent,      opt => opt.Ignore())
            .ForMember(d => d.Children,    opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,   opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,   opt => opt.Ignore())
            .ForMember(d => d.InActive,    opt => opt.Ignore());
    }
}