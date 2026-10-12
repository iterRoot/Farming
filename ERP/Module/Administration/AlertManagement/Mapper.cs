using AutoMapper;

namespace FarmingApi.Modules.Administration.AlertManagement;

public class AlertManagementMapper : Profile
{
    public AlertManagementMapper()
    {
        CreateMap<Alert, AlertResponse>()
            .ForMember(dest => dest.SenderName,
                opt => opt.MapFrom(src => src.SenderUser != null ? src.SenderUser.UserName : null));
    }
}
