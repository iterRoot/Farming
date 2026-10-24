using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.JournalEntry;

public interface IJournalEntryRepository : IRepository<JournalEntry>
{
}

public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(MyDbContext context) : base(context)
    {
    }
}