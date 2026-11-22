using KamnttraApi.Core;

namespace KamnttraApi.Modules.Sales.SaleEmployeeBuyer;

public interface ISaleEmployeeBuyerRepository
    : IRepository<SaleEmployeeBuyer> { }

public class SaleEmployeeBuyerRepository
    : Repository<SaleEmployeeBuyer>, ISaleEmployeeBuyerRepository
{
    public SaleEmployeeBuyerRepository(MyDbContext ctx) : base(ctx) { }
}