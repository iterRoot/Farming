using AutoMapper;

namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

public class SavedSearchMapper : Profile
{
    public SavedSearchMapper()
    {
        CreateMap<SavedSearch, SavedSearchResponse>()
            .ForMember(d => d.Filters, o => o.MapFrom(s => s.Filters));
        CreateMap<SavedSearchFilter, SavedSearchFilterResponse>();
    }
}
