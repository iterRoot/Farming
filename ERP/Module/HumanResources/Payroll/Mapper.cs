using AutoMapper;

namespace FarmingApi.Modules.HumanResources.Payroll;

public class PayrollMapper : Profile
{
    public PayrollMapper()
    {
        CreateMap<Payroll, PayrollResponse>()
            .ForMember(d => d.Period,
                o => o.MapFrom(s => $"{s.PeriodYear:D4}-{s.PeriodMonth:D2}"))
            .ForMember(d => d.GrossPay,
                o => o.MapFrom(s => s.BasicSalary + (s.TaxableAllowance ?? 0m)
                                  + (s.ExemptAllowance ?? 0m) + (s.OvertimePay ?? 0m)
                                  + (s.Bonus ?? 0m)))
            .ForMember(d => d.TotalDeductions,
                o => o.MapFrom(s => (s.TaxOnSalary ?? 0m) + (s.NssfEmployee ?? 0m)
                                  + (s.UnpaidLeaveDeduction ?? 0m) + (s.OtherDeductions ?? 0m)))
            .ForMember(d => d.NetPay,
                o => o.MapFrom(s => s.BasicSalary + (s.TaxableAllowance ?? 0m)
                                  + (s.ExemptAllowance ?? 0m) + (s.OvertimePay ?? 0m)
                                  + (s.Bonus ?? 0m)
                                  - (s.TaxOnSalary ?? 0m) - (s.NssfEmployee ?? 0m)
                                  - (s.UnpaidLeaveDeduction ?? 0m) - (s.OtherDeductions ?? 0m)))
            .ForMember(d => d.EmployerCost,
                o => o.MapFrom(s => s.BasicSalary + (s.TaxableAllowance ?? 0m)
                                  + (s.ExemptAllowance ?? 0m) + (s.OvertimePay ?? 0m)
                                  + (s.Bonus ?? 0m) + (s.NssfEmployer ?? 0m)))
            .ForMember(d => d.TaxableIncomeKhr, o => o.Ignore()); // set by the controller

        CreateMap<PayrollRequest, Payroll>()
            .ForMember(d => d.Id,                   opt => opt.Ignore())
            .ForMember(d => d.ExchangeRate,         opt => opt.Ignore()) // controller resolves
            .ForMember(d => d.Dependents,           opt => opt.Ignore())
            .ForMember(d => d.OvertimePay,          opt => opt.Ignore())
            .ForMember(d => d.UnpaidLeaveDeduction, opt => opt.Ignore())
            .ForMember(d => d.TaxOnSalary,          opt => opt.Ignore())
            .ForMember(d => d.NssfEmployee,         opt => opt.Ignore())
            .ForMember(d => d.NssfEmployer,         opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,            opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,            opt => opt.Ignore())
            .ForMember(d => d.InActive,             opt => opt.Ignore());
    }
}
