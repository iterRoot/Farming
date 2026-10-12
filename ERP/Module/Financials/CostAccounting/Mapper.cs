using AutoMapper;

namespace FarmingApi.Modules.Financials.CostAccounting;

public class CostAccountingMapper : Profile
{
    public CostAccountingMapper()
    {
        // ── Cost Centre ───────────────────────────────────────────
        CreateMap<CostCenter, CostCenterResponse>();

        CreateMap<CostCenterRequest, CostCenter>()
            .ForMember(d => d.Id,               opt => opt.Ignore())
            .ForMember(d => d.VersionNum,       opt => opt.Ignore())
            .ForMember(d => d.TotalDebit,       opt => opt.Ignore())
            .ForMember(d => d.TotalCredit,      opt => opt.Ignore())
            .ForMember(d => d.Balance,          opt => opt.Ignore())
            .ForMember(d => d.ParentCostCenter, opt => opt.Ignore())
            .ForMember(d => d.ChildCostCenters, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,        opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,        opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,        opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,        opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,        opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,        opt => opt.Ignore())
            .ForMember(d => d.InActive,         opt => opt.Ignore());

        CreateMap<CostCenterUpdateRequest, CostCenter>()
            .ForMember(d => d.Id,               opt => opt.Ignore())
            .ForMember(d => d.CostCenterCode,   opt => opt.Ignore())
            .ForMember(d => d.VersionNum,       opt => opt.Ignore())
            .ForMember(d => d.TotalDebit,       opt => opt.Ignore())
            .ForMember(d => d.TotalCredit,      opt => opt.Ignore())
            .ForMember(d => d.Balance,          opt => opt.Ignore())
            .ForMember(d => d.ParentCostCenter, opt => opt.Ignore())
            .ForMember(d => d.ChildCostCenters, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,        opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,        opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,        opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,        opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,        opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,        opt => opt.Ignore())
            .ForMember(d => d.InActive,         opt => opt.Ignore());

        // ── Distribution Rule ─────────────────────────────────────
        CreateMap<DistributionRule, DistributionRuleResponse>()
            .ForMember(d => d.Lines,    opt => opt.MapFrom(s => s.Lines.OrderBy(l => l.LineNum)))
            .ForMember(d => d.TotalPct, opt => opt.MapFrom(s => s.Lines.Sum(l => l.Percentage)));

        CreateMap<DistributionRuleLine, DistributionRuleLineResponse>();

        CreateMap<DistributionRuleRequest, DistributionRule>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.InUse,     opt => opt.Ignore())
            .ForMember(d => d.VersionNum,opt => opt.Ignore())
            .ForMember(d => d.Lines,     opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedBy, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());

        CreateMap<DistributionRuleUpdateRequest, DistributionRule>()
            .ForMember(d => d.Id,        opt => opt.Ignore())
            .ForMember(d => d.RuleCode,  opt => opt.Ignore())
            .ForMember(d => d.InUse,     opt => opt.Ignore())
            .ForMember(d => d.VersionNum,opt => opt.Ignore())
            .ForMember(d => d.Lines,     opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.CreatedBy, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedBy, opt => opt.Ignore())
            .ForMember(d => d.InActive,  opt => opt.Ignore());

        CreateMap<DistributionRuleLineRequest, DistributionRuleLine>()
            .ForMember(d => d.Id,                 opt => opt.Ignore())
            .ForMember(d => d.DistributionRuleId, opt => opt.Ignore())
            .ForMember(d => d.CostCenterName,     opt => opt.MapFrom(s => s.CostCenterName ?? string.Empty))
            .ForMember(d => d.DistributionRule,   opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,          opt => opt.Ignore())
            .ForMember(d => d.CreatedBy,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,          opt => opt.Ignore())
            .ForMember(d => d.UpdatedBy,          opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,          opt => opt.Ignore())
            .ForMember(d => d.DeletedBy,          opt => opt.Ignore())
            .ForMember(d => d.InActive,           opt => opt.Ignore());
    }
}
