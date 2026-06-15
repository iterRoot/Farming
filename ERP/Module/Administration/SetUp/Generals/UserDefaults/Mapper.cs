using AutoMapper;

namespace FarmingApi.Modules.Administration.UserDefaults;

public class UserDefaultMapper : Profile
{
    public UserDefaultMapper()
    {
        CreateMap<UserDefault, UserDefaultResponse>();
        CreateMap<UserDefaultRequest, UserDefault>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}