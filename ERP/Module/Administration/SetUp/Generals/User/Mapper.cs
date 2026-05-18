using AutoMapper;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<User, UserListResponse>();

        CreateMap<UserListRequest, User>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.LastLogin, opt => opt.Ignore())
            .ForMember(d => d.LoginAttempts, opt => opt.Ignore())
            .ForMember(d => d.IsLocked, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore())
            .ForMember(d => d.UserGroup, opt => opt.Ignore());

        CreateMap<UserUpdateRequest, User>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.UserId, opt => opt.Ignore())
            .ForMember(d => d.PasswordHash, opt => opt.Ignore())
            .ForMember(d => d.LastLogin, opt => opt.Ignore())
            .ForMember(d => d.LoginAttempts, opt => opt.Ignore())
            .ForMember(d => d.IsLocked, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore())
            .ForMember(d => d.UserGroup, opt => opt.Ignore());
    }
}