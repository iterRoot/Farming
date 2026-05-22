using AutoMapper;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccountsMapper : Profile
{
    public ChartOfAccountsMapper()
    {
        // ── Entity → Response (all fields map by name) ────────────
        CreateMap<ChartOfAccounts, ChartOfAccountsResponse>();
        CreateMap<ChartOfAccounts, ChartOfAccountsTreeResponse>()
            .ForMember(d => d.Children, opt => opt.Ignore());

        // ── Create Request → Entity ───────────────────────────────
        CreateMap<ChartOfAccountsCreateRequest, ChartOfAccounts>()
            // System — set by controller
            .ForMember(d => d.Id,            opt => opt.Ignore())
            .ForMember(d => d.Balance,       opt => opt.Ignore())
            .ForMember(d => d.DebitBalance,  opt => opt.Ignore())
            .ForMember(d => d.CreditBalance, opt => opt.Ignore())
            .ForMember(d => d.VersionNum,    opt => opt.Ignore())
            .ForMember(d => d.UserSign,      opt => opt.Ignore())
            .ForMember(d => d.DataSource,    opt => opt.Ignore())
            // AllowPosting = inverse of BlockManualPosting
            .ForMember(d => d.AllowPosting,  opt => opt.MapFrom(s => !s.BlockManualPosting))
            // Freeze — not set on create
            .ForMember(d => d.Frozen,        opt => opt.Ignore())
            .ForMember(d => d.FrozenFrom,    opt => opt.Ignore())
            .ForMember(d => d.FrozenTo,      opt => opt.Ignore())
            // Navigation
            .ForMember(d => d.ParentAccount, opt => opt.Ignore())
            .ForMember(d => d.ChildAccounts, opt => opt.Ignore())
            // AuditableEntity fields
            .ForMember(d => d.CreatedAt,     opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,     opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,     opt => opt.Ignore())
            .ForMember(d => d.InActive,      opt => opt.Ignore());

        // ── Update Request → Entity ───────────────────────────────
        CreateMap<ChartOfAccountsUpdateRequest, ChartOfAccounts>()
            // Immutable after creation
            .ForMember(d => d.Id,            opt => opt.Ignore())
            .ForMember(d => d.AcctCode,      opt => opt.Ignore())
            .ForMember(d => d.Level,         opt => opt.Ignore())
            .ForMember(d => d.FatherNum,     opt => opt.Ignore())
            // System — managed separately
            .ForMember(d => d.Balance,       opt => opt.Ignore())
            .ForMember(d => d.DebitBalance,  opt => opt.Ignore())
            .ForMember(d => d.CreditBalance, opt => opt.Ignore())
            .ForMember(d => d.VersionNum,    opt => opt.Ignore())
            .ForMember(d => d.UserSign,      opt => opt.Ignore())
            .ForMember(d => d.DataSource,    opt => opt.Ignore())
            // AllowPosting = inverse of BlockManualPosting
            .ForMember(d => d.AllowPosting,  opt => opt.MapFrom(s => !s.BlockManualPosting))
            // Navigation
            .ForMember(d => d.ParentAccount, opt => opt.Ignore())
            .ForMember(d => d.ChildAccounts, opt => opt.Ignore())
            // AuditableEntity fields
            .ForMember(d => d.CreatedAt,     opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,     opt => opt.Ignore())
            .ForMember(d => d.DeletedAt,     opt => opt.Ignore())
            .ForMember(d => d.InActive,      opt => opt.Ignore());
    }
}