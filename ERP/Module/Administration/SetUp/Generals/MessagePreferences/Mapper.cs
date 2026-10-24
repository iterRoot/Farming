using AutoMapper;

namespace FarmingApi.Modules.Administration.MessagePreferences;

public class MessagePreferenceMapper : Profile
{
    public MessagePreferenceMapper()
    {
        CreateMap<MessagePreference, MessagePreferenceResponse>();
        CreateMap<MessagePreferenceRequest, MessagePreference>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}