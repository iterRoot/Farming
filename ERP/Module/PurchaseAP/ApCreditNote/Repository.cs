using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.APCreditNote;

public interface IAPCreditNoteRepository : IRepository<APCreditNote>
{
}

public class APCreditNoteRepository : Repository<APCreditNote>, IAPCreditNoteRepository
{
    public APCreditNoteRepository(MyDbContext context) : base(context)
    {
    }
}