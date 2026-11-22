using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.JournalEntry;

public interface IJournalEntryRepository : IRepository<JournalEntry>
{
}

public class JournalEntryRepository : Repository<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(MyDbContext context) : base(context)
    {
    }
}