using AutoMapper;
using System.Text.Json;

namespace FarmingApi.Modules.Administration.ReferenceFieldLink;

public class ReferenceFieldLinkMapper : Profile
{
    public ReferenceFieldLinkMapper()
    {
        CreateMap<ReferenceFieldLink, ReferenceFieldLinkResponse>()
            .ForMember(d => d.AutoFillList, opt => opt.MapFrom(s =>
                ParseAutoFill(s.AutoFillFields)));

        CreateMap<ReferenceFieldLinkRequest, ReferenceFieldLink>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());
    }

    private static List<AutoFillFieldItem> ParseAutoFill(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<AutoFillFieldItem>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new();
        }
        catch { return new(); }
    }
}