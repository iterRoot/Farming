using AutoMapper;

namespace FarmingApi.Modules.HumanResources.Attendance;

public class AttendanceMapper : Profile
{
    public AttendanceMapper()
    {
        CreateMap<Attendance, AttendanceResponse>();

        CreateMap<AttendanceRequest, Attendance>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}
