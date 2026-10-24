using AutoMapper;

namespace FarmingApi.Modules.Administration.CrystalServer;

public class CrystalServerMapper : Profile
{
    public CrystalServerMapper()
    {
        CreateMap<CrystalServerConfig, CrystalServerConfigResponse>()
            // Mask passwords on read → show "••••••••" if set, empty if not
            .ForMember(d => d.Password,     opt => opt.MapFrom(s =>
                !string.IsNullOrEmpty(s.Password)     ? "••••••••" : null))
            .ForMember(d => d.DbPassword,   opt => opt.MapFrom(s =>
                !string.IsNullOrEmpty(s.DbPassword)   ? "••••••••" : null))
            .ForMember(d => d.SmtpPassword, opt => opt.MapFrom(s =>
                !string.IsNullOrEmpty(s.SmtpPassword) ? "••••••••" : null));

        CreateMap<CrystalServerConfigRequest, CrystalServerConfig>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore())
            // Only overwrite password fields if request sends a non-masked value
            .ForMember(d => d.Password, opt => opt.Condition(
                (src, _, _) => src.Password != null && src.Password != "••••••••"))
            .ForMember(d => d.DbPassword, opt => opt.Condition(
                (src, _, _) => src.DbPassword != null && src.DbPassword != "••••••••"))
            .ForMember(d => d.SmtpPassword, opt => opt.Condition(
                (src, _, _) => src.SmtpPassword != null && src.SmtpPassword != "••••••••"));
    }
}