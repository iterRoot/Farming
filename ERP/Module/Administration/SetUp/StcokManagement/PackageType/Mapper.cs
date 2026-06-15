

// ═══════════════════════════════════════════════════════════════
// Mapper.cs
// ═══════════════════════════════════════════════════════════════
using AutoMapper;

namespace FarmingApi.Modules.Inventory.PackageTypes;

public class PackageTypeMapper : Profile
{
    public PackageTypeMapper()
    {
        CreateMap<PackageType, PackageTypeResponse>();
        CreateMap<PackageTypeRequest, PackageType>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }
}

