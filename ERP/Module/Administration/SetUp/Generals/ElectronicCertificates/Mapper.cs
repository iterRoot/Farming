using AutoMapper;

namespace FarmingApi.Modules.Administration.ElectronicCertificates;

public class ElectronicCertificateMapper : Profile
{
    public ElectronicCertificateMapper()
    {
        CreateMap<ElectronicCertificate, ElectronicCertificateResponse>()
            .ForMember(d => d.IsExpired,
                opt => opt.MapFrom(s =>
                    s.ExpiryDate.HasValue && s.ExpiryDate.Value.Date < DateTime.UtcNow.Date))
            .ForMember(d => d.IsExpiringSoon,
                opt => opt.MapFrom(s =>
                    s.ExpiryDate.HasValue &&
                    s.ExpiryDate.Value.Date >= DateTime.UtcNow.Date &&
                    (s.ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days
                        <= (s.RenewalDaysAlert ?? 90)))
            .ForMember(d => d.DaysUntilExpiry,
                opt => opt.MapFrom(s =>
                    s.ExpiryDate.HasValue
                        ? (int?)(s.ExpiryDate.Value.Date - DateTime.UtcNow.Date).Days
                        : null))
            .ForMember(d => d.Attributes,
                opt => opt.MapFrom(s => s.Attributes.OrderBy(a => a.SortOrder)));

        CreateMap<CertificateAttribute, CertificateAttributeResponse>();

        CreateMap<ElectronicCertificateRequest, ElectronicCertificate>()
            .ForMember(d => d.Id,               opt => opt.Ignore())
            .ForMember(d => d.CertNo,           opt => opt.Ignore())
            .ForMember(d => d.Status,           opt => opt.Ignore())
            .ForMember(d => d.RevocationReason, opt => opt.Ignore())
            .ForMember(d => d.RevokedAt,        opt => opt.Ignore())
            .ForMember(d => d.RenewedFromId,    opt => opt.Ignore())
            .ForMember(d => d.RenewedToId,      opt => opt.Ignore())
            .ForMember(d => d.IsSigned,         opt => opt.Ignore())
            .ForMember(d => d.SignatureCert,    opt => opt.Ignore())
            .ForMember(d => d.SignatureHash,    opt => opt.Ignore())
            .ForMember(d => d.SignedAt,         opt => opt.Ignore())
            .ForMember(d => d.VerifiedAt,       opt => opt.Ignore())
            .ForMember(d => d.VerifiedBy,       opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,        opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,        opt => opt.Ignore())
            .ForMember(d => d.InActive,         opt => opt.Ignore())
            .ForMember(d => d.Attributes,       opt => opt.MapFrom(s => s.Attributes));

        CreateMap<CertificateAttributeRequest, CertificateAttribute>()
            .ForMember(d => d.Id,                      opt => opt.Ignore())
            .ForMember(d => d.ElectronicCertificateId, opt => opt.Ignore())
            .ForMember(d => d.ElectronicCertificate,   opt => opt.Ignore());
    }
}