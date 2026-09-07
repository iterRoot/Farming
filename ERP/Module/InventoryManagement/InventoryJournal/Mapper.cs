using AutoMapper;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

public class InventoryJournalMapper : Profile
{
    public InventoryJournalMapper()
    {
        CreateMap<InventoryJournal, InventoryJournalResponse>();
    }
}
