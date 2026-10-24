using AutoMapper;

namespace FarmingApi.Modules.Administration.ElectronicDocuments;

public class ElectronicDocumentMapper : Profile
{
    public ElectronicDocumentMapper()
    {
        CreateMap<ElectronicDocument, ElectronicDocumentResponse>()
            .ForMember(d => d.Logs,
                opt => opt.MapFrom(s => s.Logs.OrderByDescending(l => l.LoggedAt)));

        CreateMap<ElectronicDocumentLog, ElectronicDocumentLogResponse>();

        CreateMap<ElectronicDocumentRequest, ElectronicDocument>()
            .ForMember(d => d.Id,            opt => opt.Ignore())
            .ForMember(d => d.DocumentNo,    opt => opt.Ignore()) // generated in controller
            .ForMember(d => d.Status,        opt => opt.Ignore())
            .ForMember(d => d.GeneratedAt,   opt => opt.Ignore())
            .ForMember(d => d.SignedAt,      opt => opt.Ignore())
            .ForMember(d => d.SubmittedAt,   opt => opt.Ignore())
            .ForMember(d => d.AcknowledgedAt,opt => opt.Ignore())
            .ForMember(d => d.SubmittedBy,   opt => opt.Ignore())
            .ForMember(d => d.ExternalRef,   opt => opt.Ignore())
            .ForMember(d => d.SubmissionId,  opt => opt.Ignore())
            .ForMember(d => d.QrCodeData,    opt => opt.Ignore())
            .ForMember(d => d.ResponseCode,  opt => opt.Ignore())
            .ForMember(d => d.ResponseMessage,opt => opt.Ignore())
            .ForMember(d => d.IsValid,       opt => opt.Ignore())
            .ForMember(d => d.IsSigned,      opt => opt.Ignore())
            .ForMember(d => d.SignatureCert, opt => opt.Ignore())
            .ForMember(d => d.CertExpiry,    opt => opt.Ignore())
            .ForMember(d => d.RetryCount,    opt => opt.Ignore())
            .ForMember(d => d.NextRetryAt,   opt => opt.Ignore())
            .ForMember(d => d.FileHash,      opt => opt.Ignore())
            .ForMember(d => d.FileSizeBytes, opt => opt.Ignore())
            .ForMember(d => d.MimeType,      opt => opt.Ignore())
            .ForMember(d => d.Encoding,      opt => opt.Ignore())
            .ForMember(d => d.Logs,          opt => opt.Ignore())
            .ForMember(d => d.CreatedAt,     opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt,     opt => opt.Ignore())
            .ForMember(d => d.InActive,      opt => opt.Ignore());
    }
}