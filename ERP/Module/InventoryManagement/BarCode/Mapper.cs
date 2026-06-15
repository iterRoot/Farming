using AutoMapper;

namespace FarmingApi.Modules.Inventory.BarCode;

public class BarCodeMapper : Profile
{
    public BarCodeMapper()
    {
        // Entity → Response
        CreateMap<BarCode, BarCodeResponse>()
            .ForMember(d => d.Lines,       opt => opt.MapFrom(s => s.Lines))
            .ForMember(d => d.TotalCodes,  opt => opt.MapFrom(s => s.Lines.Count))
            .ForMember(d => d.DefaultCode, opt => opt.MapFrom(s =>
                s.Lines.FirstOrDefault(l => l.IsDefault)!.Code
                ?? s.Lines.FirstOrDefault()!.Code));

        CreateMap<BarCodeLine, BarCodeLineResponse>();

        // Create Request → Entity
        CreateMap<BarCodeCreateRequest, BarCode>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.Lines,      opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());

        // Update Request → Entity
        CreateMap<BarCodeUpdateRequest, BarCode>()
            .ForMember(d => d.Id,          opt => opt.Ignore())
            .ForMember(d => d.ItemNo,      opt => opt.Ignore())
            .ForMember(d => d.VersionNum,  opt => opt.Ignore())
            .ForMember(d => d.Lines,       opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,   opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,   opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,   opt => opt.Ignore())
            .ForMember(d => d.InActive,    opt => opt.Ignore());

        // Line Request → Line Entity
        CreateMap<BarCodeLineRequest, BarCodeLine>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.BarCodeId,  opt => opt.Ignore())
            .ForMember(d => d.BarCode,    opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());
    }
}