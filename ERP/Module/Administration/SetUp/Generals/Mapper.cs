using AutoMapper;

namespace FarmingApi.Modules.Administration.UserGroups;

public class UserGroupMapper : Profile
{
    public UserGroupMapper()
    {
        CreateMap<UserGroup, UserGroupResponse>()
            .ForMember(d => d.Members,     opt => opt.MapFrom(s => s.Members))
            .ForMember(d => d.Permissions, opt => opt.MapFrom(s => s.Permissions));

        CreateMap<UserGroupMember,     UserGroupMemberResponse>();
        CreateMap<UserGroupPermission, UserGroupPermissionResponse>();

        CreateMap<UserGroupRequest, UserGroup>()
            .ForMember(d => d.Id,          opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,   opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,   opt => opt.Ignore())
            .ForMember(d => d.InActive,    opt => opt.Ignore())
            .ForMember(d => d.Members,     opt => opt.MapFrom(s => s.Members))
            .ForMember(d => d.Permissions, opt => opt.MapFrom(s => s.Permissions));

        CreateMap<UserGroupMemberRequest, UserGroupMember>()
            .ForMember(d => d.Id,          opt => opt.Ignore())
            .ForMember(d => d.UserGroupId, opt => opt.Ignore())
            .ForMember(d => d.UserGroup,   opt => opt.Ignore());

        CreateMap<UserGroupPermissionRequest, UserGroupPermission>()
            .ForMember(d => d.Id,          opt => opt.Ignore())
            .ForMember(d => d.UserGroupId, opt => opt.Ignore())
            .ForMember(d => d.UserGroup,   opt => opt.Ignore());
    }
}