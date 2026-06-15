// using AutoMapper;

// namespace FarmingApi.Modules.Financials.BankReconciliation;

// public class BankReconciliationMapper : Profile
// {
//     public BankReconciliationMapper()
//     {
//         // Entity → Response
//         CreateMap<BankReconciliation, BankReconciliationResponse>()
//             .ForMember(d => d.Lines,          opt => opt.MapFrom(s => s.Lines))
//             .ForMember(d => d.TotalLines,     opt => opt.MapFrom(s => s.Lines.Count))
//             .ForMember(d => d.ReconciledLines,opt => opt.MapFrom(s => s.Lines.Count(l => l.IsReconciled)))
//             .ForMember(d => d.PendingLines,   opt => opt.MapFrom(s => s.Lines.Count(l => !l.IsReconciled)));

//         CreateMap<BankReconciliationLine, BankReconciliationLineResponse>();

//         // Create Request → Entity
//         CreateMap<BankReconciliationRequest, BankReconciliation>()
//             .ForMember(d => d.Id,             opt => opt.Ignore())
//             .ForMember(d => d.ReconcNo,       opt => opt.Ignore())   // auto-generated
//             .ForMember(d => d.AccountName,    opt => opt.MapFrom(s => s.AccountName ?? s.AccountCode))
//             .ForMember(d => d.GlOpeningBal,   opt => opt.Ignore())   // loaded from KCOA
//             .ForMember(d => d.GlClosingBal,   opt => opt.Ignore())   // loaded from KCOA
//             .ForMember(d => d.ReconciledAmt,  opt => opt.Ignore())
//             .ForMember(d => d.UnreconciledAmt,opt => opt.Ignore())
//             .ForMember(d => d.Difference,     opt => opt.Ignore())
//             .ForMember(d => d.Status,         opt => opt.Ignore())
//             .ForMember(d => d.VersionNum,     opt => opt.Ignore())
//             .ForMember(d => d.UserSign,       opt => opt.Ignore())
//             .ForMember(d => d.Lines,          opt => opt.Ignore())
//             .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
//             .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
//             .ForMember(d => d.DeletedAt,      opt => opt.Ignore())
//             .ForMember(d => d.InActive,       opt => opt.Ignore());

//         // Update Request → Entity
//         CreateMap<BankReconciliationUpdateRequest, BankReconciliation>()
//             .ForMember(d => d.Id,             opt => opt.Ignore())
//             .ForMember(d => d.ReconcNo,       opt => opt.Ignore())
//             .ForMember(d => d.AccountCode,    opt => opt.Ignore())
//             .ForMember(d => d.AccountName,    opt => opt.Ignore())
//             .ForMember(d => d.Currency,       opt => opt.Ignore())
//             .ForMember(d => d.GlOpeningBal,   opt => opt.Ignore())
//             .ForMember(d => d.GlClosingBal,   opt => opt.Ignore())
//             .ForMember(d => d.ReconciledAmt,  opt => opt.Ignore())
//             .ForMember(d => d.UnreconciledAmt,opt => opt.Ignore())
//             .ForMember(d => d.Difference,     opt => opt.Ignore())
//             .ForMember(d => d.Status,         opt => opt.Ignore())
//             .ForMember(d => d.VersionNum,     opt => opt.Ignore())
//             .ForMember(d => d.UserSign,       opt => opt.Ignore())
//             .ForMember(d => d.Lines,          opt => opt.Ignore())
//             .ForMember(d => d.CreatedAt,      opt => opt.Ignore())
//             .ForMember(d => d.UpdatedAt,      opt => opt.Ignore())
//             .ForMember(d => d.DeletedAt,      opt => opt.Ignore())
//             .ForMember(d => d.InActive,       opt => opt.Ignore());

//         // Line Request → Line Entity
//         CreateMap<BankReconciliationLineRequest, BankReconciliationLine>()
//             .ForMember(d => d.Id,                   opt => opt.Ignore())
//             .ForMember(d => d.BankReconciliationId,  opt => opt.Ignore())
//             .ForMember(d => d.Balance,               opt => opt.Ignore())   // calculated
//             .ForMember(d => d.ReconciledDate,        opt => opt.Ignore())
//             .ForMember(d => d.ReconciledRef,         opt => opt.Ignore())
//             .ForMember(d => d.BankReconciliation,    opt => opt.Ignore())
//             .ForMember(d => d.CreatedAt,             opt => opt.Ignore())
//             .ForMember(d => d.UpdatedAt,             opt => opt.Ignore())
//             .ForMember(d => d.DeletedAt,             opt => opt.Ignore())
//             .ForMember(d => d.InActive,              opt => opt.Ignore());
//     }
// }