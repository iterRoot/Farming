// using AutoMapper;

// namespace FarmingApi.Modules.BusinessPartner.Activity;

// public class ActivityMapper : Profile
// {
//     public ActivityMapper()
//     {
//         // Entity → Response
//         CreateMap<Activity, ActivityResponse>()
//             .ForMember(d => d.StartTime, opt => opt.MapFrom(s =>
//                 s.StartTime.ToString(@"hh\:mm")))
//             .ForMember(d => d.EndTime, opt => opt.MapFrom(s =>
//                 s.EndTime.ToString(@"hh\:mm")));

//         // Create Request → Entity
//         CreateMap<ActivityCreateRequest, Activity>()
//             .ForMember(d => d.Id,         opt => opt.Ignore())
//             .ForMember(d => d.Number,     opt => opt.Ignore())   // auto-generated
//             .ForMember(d => d.Inactive,   opt => opt.Ignore())
//             .ForMember(d => d.Closed,     opt => opt.Ignore())
//             .ForMember(d => d.ClosedDate, opt => opt.Ignore())
//             .ForMember(d => d.FollowUpId, opt => opt.Ignore())
//             .ForMember(d => d.StartTime,  opt => opt.MapFrom(s =>
//                 TimeSpan.TryParse(s.StartTime, out var t) ? t : TimeSpan.Zero))
//             .ForMember(d => d.EndTime,    opt => opt.MapFrom(s =>
//                 TimeSpan.TryParse(s.EndTime, out var t) ? t : TimeSpan.Zero))
//             .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
//             .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
//             .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
//             .ForMember(d => d.InActive,   opt => opt.Ignore());

//         // Update Request → Entity
//         CreateMap<ActivityUpdateRequest, Activity>()
//             .ForMember(d => d.Id,         opt => opt.Ignore())
//             .ForMember(d => d.Number,     opt => opt.Ignore())
//             .ForMember(d => d.ClosedDate, opt => opt.Ignore())
//             .ForMember(d => d.FollowUpId, opt => opt.Ignore())
//             .ForMember(d => d.StartTime,  opt => opt.MapFrom(s =>
//                 TimeSpan.TryParse(s.StartTime, out var t) ? t : TimeSpan.Zero))
//             .ForMember(d => d.EndTime,    opt => opt.MapFrom(s =>
//                 TimeSpan.TryParse(s.EndTime, out var t) ? t : TimeSpan.Zero))
//             .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
//             .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
//             .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
//             .ForMember(d => d.InActive,   opt => opt.Ignore());
//     }
// }