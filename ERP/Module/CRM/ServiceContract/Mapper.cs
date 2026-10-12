using AutoMapper;

namespace FarmingApi.Modules.CRM.ServiceContract;

public class ServiceContractMapper : Profile
{
    public ServiceContractMapper()
    {
        CreateMap<ServiceContract, ServiceContractResponse>()
            .ForMember(d => d.IsExpired, o => o.MapFrom(s => s.Status == "Active" && s.EndDate.Date < DateTime.UtcNow.Date))
            .ForMember(d => d.EffectiveStatus, o => o.MapFrom(s =>
                s.Status == "Cancelled" ? "Cancelled" :
                s.EndDate.Date < DateTime.UtcNow.Date ? "Expired" : "Active"));
    }
}
