using AutoMapper;

namespace FarmingApi.Modules.Financials.JournalVoucher;

public class JournalVoucherMapper : Profile
{
    public JournalVoucherMapper()
    {
        CreateMap<JournalVoucher, JournalVoucherListResponse>()
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Lines));

        CreateMap<JournalVoucherLine, JournalVoucherLineResponse>();

        CreateMap<JournalVoucherListRequest, JournalVoucher>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.VoucherNo, opt => opt.Ignore())
            .ForMember(d => d.TotalDebit, opt => opt.Ignore())
            .ForMember(d => d.TotalCredit, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.PostedToJournalEntryId, opt => opt.Ignore())
            .ForMember(d => d.PostedDate, opt => opt.Ignore())
            .ForMember(d => d.PostedBy, opt => opt.Ignore())
            .ForMember(d => d.ApprovalStatus, opt => opt.Ignore())
            .ForMember(d => d.ApprovedBy, opt => opt.Ignore())
            .ForMember(d => d.ApprovedDate, opt => opt.Ignore())
            .ForMember(d => d.ApprovalNote, opt => opt.Ignore())
            .ForMember(d => d.UserSign, opt => opt.Ignore())
            .ForMember(d => d.UserSign2, opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.Lines, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());

        CreateMap<JournalVoucherLineRequest, JournalVoucherLine>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.JournalVoucherId, opt => opt.Ignore())
            .ForMember(d => d.AccountName, opt => opt.Ignore())
            .ForMember(d => d.CardName, opt => opt.Ignore())
            .ForMember(d => d.JournalVoucher, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());
    }
}