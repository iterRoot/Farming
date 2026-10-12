using AutoMapper;

namespace FarmingApi.Modules.CRM.ServiceCall;

public class ServiceCallMapper : Profile
{
    public ServiceCallMapper()
    {
        CreateMap<ServiceCall, ServiceCallResponse>();
    }
}
