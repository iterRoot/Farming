using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.FinancialProjects;

public interface IFinancialProjectRepository : IRepository<FinancialProject> { }
public class FinancialProjectRepository : Repository<FinancialProject>, IFinancialProjectRepository
{
    public FinancialProjectRepository(MyDbContext context) : base(context) { }
}
