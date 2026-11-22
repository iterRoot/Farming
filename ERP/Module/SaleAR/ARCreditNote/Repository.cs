using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.ARCreditNote;

public interface IARCreditNoteRepository : IRepository<ARCreditNote>
{
}

public class ARCreditNoteRepository : Repository<ARCreditNote>, IARCreditNoteRepository
{
    public ARCreditNoteRepository(MyDbContext context) : base(context)
    {
    }
}