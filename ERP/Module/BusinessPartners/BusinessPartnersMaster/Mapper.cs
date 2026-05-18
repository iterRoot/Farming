using AutoMapper;

namespace FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;

public class BusinessPartnersMasterMapper : Profile
{
    public BusinessPartnersMasterMapper()
    {
        // ═══════════════════════════════════════════════════════════════════
        // ENTITY → RESPONSE
        // ═══════════════════════════════════════════════════════════════════

        // Main BP — map int TypeStatus/ActiveStatus to human-readable labels
        CreateMap<BusinessPartnersMaster, BPResponse>()
            .ForMember(dest => dest.TypeStatusLabel, opt => opt.MapFrom(src =>
                src.TypeStatus == 0 ? "Customer"
                : src.TypeStatus == 1 ? "Vendor"
                : "Lead"))
            .ForMember(dest => dest.ActiveStatusLabel, opt => opt.MapFrom(src =>
                src.ActiveStatus == 0 ? "Active" : "Inactive"));

        // Main BP → lightweight list response
        CreateMap<BusinessPartnersMaster, BPListResponse>()
            .ForMember(dest => dest.TypeStatusLabel, opt => opt.MapFrom(src =>
                src.TypeStatus == 0 ? "Customer"
                : src.TypeStatus == 1 ? "Vendor"
                : "Lead"))
            .ForMember(dest => dest.ActiveStatusLabel, opt => opt.MapFrom(src =>
                src.ActiveStatus == 0 ? "Active" : "Inactive"));

        // Sub-tables — all field names match, auto-maps
        CreateMap<BPAddress,     BPAddressDto>();
        CreateMap<BPContact,     BPContactDto>();
        CreateMap<BPBankAccount, BPBankAccountDto>();

        // ═══════════════════════════════════════════════════════════════════
        // CREATE REQUEST → ENTITY
        // ═══════════════════════════════════════════════════════════════════
        CreateMap<BPCreateRequest, BusinessPartnersMaster>()
            // System-managed — never from request
            .ForMember(dest => dest.Id,         opt => opt.Ignore())
            .ForMember(dest => dest.Balance,    opt => opt.Ignore())
            .ForMember(dest => dest.VersionNum, opt => opt.Ignore())
            .ForMember(dest => dest.UserSign,   opt => opt.Ignore())
            // Audit from AuditableEntity
            .ForMember(dest => dest.CreatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.InActive,   opt => opt.Ignore())
            // Sub-tables mapped separately in controller
            .ForMember(dest => dest.Addresses,  opt => opt.Ignore())
            .ForMember(dest => dest.Contacts,   opt => opt.Ignore())
            .ForMember(dest => dest.BankAccts,  opt => opt.Ignore());

        // Sub-table DTOs → entities (for create)
        CreateMap<BPAddressDto, BPAddress>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.BPId,      opt => opt.Ignore())
            .ForMember(dest => dest.BP,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.InActive,  opt => opt.Ignore());

        CreateMap<BPContactDto, BPContact>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.BPId,      opt => opt.Ignore())
            .ForMember(dest => dest.BP,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.InActive,  opt => opt.Ignore());

        CreateMap<BPBankAccountDto, BPBankAccount>()
            .ForMember(dest => dest.Id,        opt => opt.Ignore())
            .ForMember(dest => dest.BPId,      opt => opt.Ignore())
            .ForMember(dest => dest.BP,        opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.InActive,  opt => opt.Ignore());

        // ═══════════════════════════════════════════════════════════════════
        // UPDATE REQUEST → ENTITY
        // ═══════════════════════════════════════════════════════════════════
        CreateMap<BPUpdateRequest, BusinessPartnersMaster>()
            // Immutable after creation
            .ForMember(dest => dest.Id,         opt => opt.Ignore())
            .ForMember(dest => dest.Code,       opt => opt.Ignore())
            // System-managed
            .ForMember(dest => dest.Balance,    opt => opt.Ignore())
            .ForMember(dest => dest.VersionNum, opt => opt.Ignore())
            .ForMember(dest => dest.UserSign,   opt => opt.Ignore())
            // Audit
            .ForMember(dest => dest.CreatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt,  opt => opt.Ignore())
            .ForMember(dest => dest.InActive,   opt => opt.Ignore())
            // Sub-tables handled manually in controller (clear + re-add)
            .ForMember(dest => dest.Addresses,  opt => opt.Ignore())
            .ForMember(dest => dest.Contacts,   opt => opt.Ignore())
            .ForMember(dest => dest.BankAccts,  opt => opt.Ignore());
    }
}