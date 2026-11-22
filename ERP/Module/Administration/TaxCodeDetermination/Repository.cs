using KamnttraApi.Core;

namespace KamnttraApi.Modules.Administration.TaxCodeDetermination;

public interface ITaxDeterminationRuleRepository : IRepository<TaxDeterminationRule> { }
public class TaxDeterminationRuleRepository : Repository<TaxDeterminationRule>, ITaxDeterminationRuleRepository
{
    public TaxDeterminationRuleRepository(MyDbContext context) : base(context) { }
}
