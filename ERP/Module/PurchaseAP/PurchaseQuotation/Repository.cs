using FarmingApi.Core;

namespace FarmingApi.Modules.PurchaseAP.PurchaseQuotation;

public interface IPurchaseQuotationRepository : IRepository<PurchaseQuotation>
{
}

public class PurchaseQuotationRepository 
    : Repository<PurchaseQuotation>, IPurchaseQuotationRepository
{
    public PurchaseQuotationRepository(MyDbContext context) : base(context)
    {
    }
}