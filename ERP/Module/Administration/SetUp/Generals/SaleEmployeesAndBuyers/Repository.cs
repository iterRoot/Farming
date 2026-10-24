using FarmingApi.Core;

namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

public interface ISaleEmployeeBuyerRepository
    : IRepository<SaleEmployeeBuyer> { }

public class SaleEmployeeBuyerRepository
    : Repository<SaleEmployeeBuyer>, ISaleEmployeeBuyerRepository
{
    public SaleEmployeeBuyerRepository(MyDbContext ctx) : base(ctx) { }
}