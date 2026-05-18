using AutoMapper;

namespace FarmingApi.Modules.Administration.SetUp.UserGroup;

public class UserGroupMapper : Profile
{
    public UserGroupMapper()
    {
        CreateMap<UserGroup, UserGroupListResponse>();

        CreateMap<UserGroupListRequest, UserGroup>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.LogInstanc, opt => opt.Ignore())
            .ForMember(d => d.UserSign, opt => opt.Ignore())
            .ForMember(d => d.UserSign2, opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());

        CreateMap<UserGroupUpdateRequest, UserGroup>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.LogInstanc, opt => opt.Ignore())
            .ForMember(d => d.UserSign, opt => opt.Ignore())
            .ForMember(d => d.UserSign2, opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());
    }
}