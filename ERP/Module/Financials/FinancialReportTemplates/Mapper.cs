using AutoMapper;

namespace FarmingApi.Modules.Financials.FinancialReportTemplates;

public class FinancialReportTemplateMapper : Profile
{
    public FinancialReportTemplateMapper()
    {
        CreateMap<FinancialReportTemplateRow, FinancialReportTemplateRowResponse>();

        CreateMap<FinancialReportTemplate, FinancialReportTemplateResponse>()
            .ForMember(d => d.RowCount, o => o.MapFrom(s => s.Rows.Count));

        // Registered so Duplicate can copy a stored template through a request.
        CreateMap<FinancialReportTemplateRow, FinancialReportTemplateRowRequest>();
        CreateMap<FinancialReportTemplate, FinancialReportTemplateRequest>();
    }
}
