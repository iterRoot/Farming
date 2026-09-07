using AutoMapper;

namespace FarmingApi.Modules.Administration.UserDefaults;

public class UserDefaultMapper : Profile
{
    public UserDefaultMapper()
    {
        CreateMap<UserDefault, UserDefaultResponse>();
        // Entity -> Request: used by CopyTo to clone one user's business
        // fields while leaving identity and audit columns behind.
        CreateMap<UserDefault, UserDefaultRequest>();
        CreateMap<UserDefaultRequest, UserDefault>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}