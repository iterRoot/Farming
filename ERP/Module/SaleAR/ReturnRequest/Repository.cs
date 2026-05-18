using FarmingApi.Core;

namespace FarmingApi.Modules.SaleAR.ReturnRequest;

public interface IReturnRequestRepository : IRepository<ReturnRequest>
{
}

public class ReturnRequestRepository : Repository<ReturnRequest>, IReturnRequestRepository
{
    public ReturnRequestRepository(MyDbContext context) : base(context)
    {
    }
}