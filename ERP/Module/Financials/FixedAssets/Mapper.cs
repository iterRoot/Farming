using AutoMapper;

namespace FarmingApi.Modules.Financials.FixedAssets;

public class AssetMasterProfile : Profile
{
    public AssetMasterProfile()
    {
        CreateMap<AssetMaster, AssetMasterDto>()
            .ForMember(d => d.NetBookValue,
                o => o.MapFrom(s => s.AcquisitionCost - s.AccumulatedDepreciation));

        CreateMap<AssetMasterSaveRequest, AssetMaster>()
            // never let the request touch identity / audit fields
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.InActive, o => o.Ignore());
    }
}
