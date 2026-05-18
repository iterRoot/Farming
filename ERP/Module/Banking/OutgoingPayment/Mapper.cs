using AutoMapper;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

public class OutgoingPaymentMapper : Profile
{
    public OutgoingPaymentMapper()
    {
        // ═══════════════════════════════════════════════════════════════
        // ENTITY -> RESPONSE
        // ═══════════════════════════════════════════════════════════════
        CreateMap<OutgoingPayment, OutgoingPaymentListResponse>()
            .ForMember(d => d.Invoices, opt => opt.MapFrom(s => s.Invoices));

        CreateMap<OutgoingPaymentInvoice, OutgoingPaymentInvoiceResponse>();

        // ═══════════════════════════════════════════════════════════════
        // CREATE REQUEST -> ENTITY
        // ═══════════════════════════════════════════════════════════════
        CreateMap<OutgoingPaymentListRequest, OutgoingPayment>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.DocNum, opt => opt.Ignore())
            .ForMember(d => d.CardName, opt => opt.Ignore())
            .ForMember(d => d.AppliedAmount, opt => opt.Ignore())
            .ForMember(d => d.UnappliedAmount, opt => opt.Ignore())
            .ForMember(d => d.BankName, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.JournalEntryId, opt => opt.Ignore())
            .ForMember(d => d.TransType, opt => opt.Ignore())
            .ForMember(d => d.UserSign, opt => opt.Ignore())
            .ForMember(d => d.UserSign2, opt => opt.Ignore())
            .ForMember(d => d.VersionNum, opt => opt.Ignore())
            .ForMember(d => d.Invoices, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.DeletedAt, opt => opt.Ignore())
            .ForMember(d => d.InActive, opt => opt.Ignore());
    }
}