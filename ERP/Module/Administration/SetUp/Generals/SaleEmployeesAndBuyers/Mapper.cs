using AutoMapper;

namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

public class SaleEmployeeBuyerMapper : Profile
{
    public SaleEmployeeBuyerMapper()
    {
        CreateMap<SaleEmployeeBuyer, SaleEmployeeBuyerResponse>()
            .ForMember(d => d.FullName,
                opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}".Trim()));

        CreateMap<SaleEmployeeBuyerRequest, SaleEmployeeBuyer>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}