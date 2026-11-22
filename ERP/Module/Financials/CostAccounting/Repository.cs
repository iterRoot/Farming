using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.CostAccounting;

public interface ICostCenterRepository : IRepository<CostCenter> { }
public class CostCenterRepository : Repository<CostCenter>, ICostCenterRepository
{
    public CostCenterRepository(MyDbContext context) : base(context) { }
}

public interface IDistributionRuleRepository : IRepository<DistributionRule> { }
public class DistributionRuleRepository : Repository<DistributionRule>, IDistributionRuleRepository
{
    public DistributionRuleRepository(MyDbContext context) : base(context) { }
}
