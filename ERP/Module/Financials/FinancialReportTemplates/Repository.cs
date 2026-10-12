using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FinancialReportTemplates;

public interface IFinancialReportTemplateRepository : IRepository<FinancialReportTemplate> { }

public class FinancialReportTemplateRepository
    : Repository<FinancialReportTemplate>, IFinancialReportTemplateRepository
{
    public FinancialReportTemplateRepository(MyDbContext context) : base(context) { }
}
