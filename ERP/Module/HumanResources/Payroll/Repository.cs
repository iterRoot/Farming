using FarmingApi.Core;

namespace FarmingApi.Modules.HumanResources.Payroll;

public interface IPayrollRepository : IRepository<Payroll> { }

public class PayrollRepository : Repository<Payroll>, IPayrollRepository
{
    public PayrollRepository(MyDbContext ctx) : base(ctx) { }
}

public interface IPayrollRunLogRepository : IRepository<PayrollRunLog> { }

public class PayrollRunLogRepository : Repository<PayrollRunLog>, IPayrollRunLogRepository
{
    public PayrollRunLogRepository(MyDbContext ctx) : base(ctx) { }
}
