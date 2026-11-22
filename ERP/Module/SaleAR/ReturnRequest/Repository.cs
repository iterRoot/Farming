using KamnttraApi.Core;

namespace KamnttraApi.Modules.SaleAR.ReturnRequest;

public interface IReturnRequestRepository : IRepository<ReturnRequest>
{
}

public class ReturnRequestRepository : Repository<ReturnRequest>, IReturnRequestRepository
{
    public ReturnRequestRepository(MyDbContext context) : base(context)
    {
    }
}