using AutoMapper;

namespace FarmingApi.Modules.Administration.Licence;

public class LicensePackageMapper : Profile
{
    public LicensePackageMapper()
    {
        CreateMap<LicensePackage, LicensePackageResponse>();

        CreateMap<LicensePackageRequest, LicensePackage>()
            .ForMember(d => d.Id,           opt => opt.Ignore())
            .ForMember(d => d.BillingCycle, opt => opt.MapFrom(s => string.IsNullOrWhiteSpace(s.BillingCycle) ? "Monthly" : s.BillingCycle))
            .ForMember(d => d.Modules,      opt => opt.MapFrom(s => s.Modules ?? new List<string>()))
            .ForMember(d => d.CreatedAt,    opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,    opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,    opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,    opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,    opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,    opt => opt.Ignore())
            .ForMember(d => d.InActive,     opt => opt.Ignore());
    }
}
