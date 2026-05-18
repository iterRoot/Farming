using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ARCreditNote;

public interface IARCreditNoteRepository : IRepository<ARCreditNote>
{
}

public class ARCreditNoteRepository : Repository<ARCreditNote>, IARCreditNoteRepository
{
    public ARCreditNoteRepository(MyDbContext context) : base(context)
    {
    }
}