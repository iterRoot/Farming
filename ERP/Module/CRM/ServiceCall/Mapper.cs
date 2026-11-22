using AutoMapper;

namespace KamnttraApi.Modules.CRM.ServiceCall;

public class ServiceCallMapper : Profile
{
    public ServiceCallMapper()
    {
        CreateMap<ServiceCall, ServiceCallResponse>();
    }
}
