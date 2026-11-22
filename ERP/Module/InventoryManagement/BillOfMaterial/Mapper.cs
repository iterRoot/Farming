using AutoMapper;

namespace KamnttraApi.Modules.InventoryManagement.BillOfMaterial;

public class BillOfMaterialMapper : Profile
{
    public BillOfMaterialMapper()
    {
        CreateMap<BillOfMaterial, BillOfMaterialResponse>()
            .ForMember(d => d.Lines, o => o.MapFrom(s => s.Lines));
        CreateMap<BillOfMaterialLine, BillOfMaterialLineResponse>();
    }
}
