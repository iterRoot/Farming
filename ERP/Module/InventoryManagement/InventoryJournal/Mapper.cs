using AutoMapper;

namespace KamnttraApi.Modules.InventoryManagement.InventoryJournal;

public class InventoryJournalMapper : Profile
{
    public InventoryJournalMapper()
    {
        CreateMap<InventoryJournal, InventoryJournalResponse>();
    }
}
