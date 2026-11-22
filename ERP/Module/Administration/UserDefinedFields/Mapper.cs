using AutoMapper;

namespace KamnttraApi.Modules.Administration.UserDefinedFields;

public class UserDefinedFieldMapper : Profile
{
    public UserDefinedFieldMapper()
    {
        CreateMap<UserDefinedFieldRequest, UserDefinedField>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore());

        CreateMap<UserDefinedField, UserDefinedFieldResponse>();
    }
}
