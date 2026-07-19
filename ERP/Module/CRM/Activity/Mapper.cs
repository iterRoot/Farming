using AutoMapper;

namespace FarmingApi.Modules.CRM.Activity;

public class ActivityMapper : Profile
{
    public ActivityMapper()
    {
        CreateMap<Activity, ActivityResponse>()
            .ForMember(d => d.DurationDisplay, o => o.MapFrom(s => FormatMinutes(s.DurationMinutes)))
            .ForMember(d => d.Status, o => o.MapFrom(s =>
                s.IsInactive ? "Inactive" : s.IsClosed ? "Closed" : "Open"))
            .ForMember(d => d.Attachments, o => o.MapFrom(s => s.Attachments.OrderBy(a => a.LineNum)));

        CreateMap<ActivityAttachment, ActivityAttachmentDto>();

        CreateMap<ActivityRequest, Activity>()
            .ForMember(d => d.Id,               o => o.Ignore())
            .ForMember(d => d.ActivityNo,        o => o.Ignore())
            .ForMember(d => d.AssignedBy,        o => o.Ignore())
            .ForMember(d => d.DurationMinutes,   o => o.Ignore())
            .ForMember(d => d.ClosedAt,          o => o.Ignore())
            .ForMember(d => d.CostItem,          o => o.Ignore())
            .ForMember(d => d.ProjectNo,         o => o.Ignore())
            .ForMember(d => d.SubprojectNo,      o => o.Ignore())
            .ForMember(d => d.SourceObjectType,  o => o.Ignore())
            .ForMember(d => d.SourceObjectNo,    o => o.Ignore())
            .ForMember(d => d.PreviousActivityNo,o => o.Ignore())
            .ForMember(d => d.CreatedAt,         o => o.Ignore())
            .ForMember(d => d.UpdatedAt,         o => o.Ignore())
            .ForMember(d => d.InActive,          o => o.Ignore())
            .ForMember(d => d.Attachments,       o => o.MapFrom(s => s.Attachments));

        CreateMap<ActivityAttachmentRequest, ActivityAttachment>()
            .ForMember(d => d.Id,         o => o.Ignore())
            .ForMember(d => d.ActivityId, o => o.Ignore())
            .ForMember(d => d.Activity,   o => o.Ignore());
    }

    private static string FormatMinutes(int minutes)
    {
        if (minutes <= 0) return "0 Minutes";
        if (minutes < 60) return $"{minutes} Minutes";
        var h = minutes / 60;
        var m = minutes % 60;
        return m == 0 ? $"{h}h" : $"{h}h {m}m";
    }
}