using AutoMapper;

namespace FarmingApi.Modules.Financials.Budget;

public class BudgetMapper : Profile
{
    public BudgetMapper()
    {
        // Entity → Response
        CreateMap<Budget, BudgetResponse>()
            .ForMember(d => d.Lines,       opt => opt.MapFrom(s => s.Lines))
            .ForMember(d => d.TotalIncome, opt => opt.Ignore())   // computed in controller
            .ForMember(d => d.TotalExpense,opt => opt.Ignore())
            .ForMember(d => d.NetBudget,   opt => opt.Ignore());

        CreateMap<BudgetLine, BudgetLineResponse>();

        // Create Request → Entity
        CreateMap<BudgetCreateRequest, Budget>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.BudgetNo,   opt => opt.Ignore())   // generated
            .ForMember(d => d.Status,     opt => opt.Ignore())   // "D" by default
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.UserSign,   opt => opt.Ignore())
            .ForMember(d => d.IsActive,   opt => opt.Ignore())
            .ForMember(d => d.Lines,      opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());

        // Update Request → Entity
        CreateMap<BudgetUpdateRequest, Budget>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.BudgetNo,   opt => opt.Ignore())
            .ForMember(d => d.FiscalYear, opt => opt.Ignore())
            .ForMember(d => d.Status,     opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.UserSign,   opt => opt.Ignore())
            .ForMember(d => d.IsActive,   opt => opt.Ignore())
            .ForMember(d => d.Lines,      opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());

        // Line Request → Line Entity
        CreateMap<BudgetLineRequest, BudgetLine>()
            .ForMember(d => d.Id,         opt => opt.Ignore())
            .ForMember(d => d.BudgetId,   opt => opt.Ignore())
            .ForMember(d => d.AcctName,   opt => opt.MapFrom(s => s.AcctName ?? string.Empty))
            .ForMember(d => d.AcctType,   opt => opt.MapFrom(s => s.AcctType ?? "A"))
            .ForMember(d => d.Budget,     opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,  opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,  opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,  opt => opt.Ignore())
            .ForMember(d => d.InActive,   opt => opt.Ignore());
    }
}