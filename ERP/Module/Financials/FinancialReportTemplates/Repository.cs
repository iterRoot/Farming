using KamnttraApi.Core;

namespace KamnttraApi.Modules.Financials.FinancialReportTemplates;

public interface IFinancialReportTemplateRepository : IRepository<FinancialReportTemplate> { }

public class FinancialReportTemplateRepository
    : Repository<FinancialReportTemplate>, IFinancialReportTemplateRepository
{
    public FinancialReportTemplateRepository(MyDbContext context) : base(context) { }
}
