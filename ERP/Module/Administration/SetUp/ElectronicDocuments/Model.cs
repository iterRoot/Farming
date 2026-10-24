namespace FarmingApi.Modules.Administration.ElectronicDocuments;

// ── Response ──────────────────────────────────────────────────
public class ElectronicDocumentResponse
{
    public int      Id              { get; set; }
    public string   DocumentNo      { get; set; } = null!;
    public string   DocumentType    { get; set; } = null!;
    public string   Status          { get; set; } = null!;
    public string   BaseDocType     { get; set; } = null!;
    public int?     BaseDocId       { get; set; }
    public string?  BaseDocNo       { get; set; }
    public string?  BaseDocDate     { get; set; }
    public string?  BPCode          { get; set; }
    public string?  BPName          { get; set; }
    public string?  BPTaxId         { get; set; }
    public string?  BPEmail         { get; set; }
    public string   Format          { get; set; } = null!;
    public string   Channel         { get; set; } = null!;
    public string?  Encoding        { get; set; }
    public string?  SchemaVersion   { get; set; }
    public string?  FileName        { get; set; }
    public string?  FileUrl         { get; set; }
    public long?    FileSizeBytes   { get; set; }
    public string?  FileHash        { get; set; }
    public string?  MimeType        { get; set; }
    public decimal? TotalAmount     { get; set; }
    public decimal? TaxAmount       { get; set; }
    public string?  Currency        { get; set; }
    public DateTime? GeneratedAt    { get; set; }
    public DateTime? SignedAt       { get; set; }
    public DateTime? SubmittedAt    { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string?  SubmittedBy     { get; set; }
    public string?  ExternalRef     { get; set; }
    public string?  SubmissionId    { get; set; }
    public string?  QrCodeData      { get; set; }
    public string?  ResponseCode    { get; set; }
    public string?  ResponseMessage { get; set; }
    public bool     IsValid         { get; set; }
    public bool     IsSigned        { get; set; }
    public string?  SignatureCert   { get; set; }
    public DateTime? CertExpiry     { get; set; }
    public int      RetryCount      { get; set; }
    public DateTime? NextRetryAt    { get; set; }
    public bool     IsActive        { get; set; }
    public string?  Remarks         { get; set; }
    public DateTime CreatedAt       { get; set; }
    public DateTime UpdatedAt       { get; set; }
    public List<ElectronicDocumentLogResponse> Logs { get; set; } = new();
}

public class ElectronicDocumentLogResponse
{
    public int      Id           { get; set; }
    public DateTime LoggedAt     { get; set; }
    public string   EventType    { get; set; } = null!;
    public string   FromStatus   { get; set; } = null!;
    public string   ToStatus     { get; set; } = null!;
    public string?  PerformedBy  { get; set; }
    public string?  Channel      { get; set; }
    public string?  ExternalRef  { get; set; }
    public string?  ResponseCode { get; set; }
    public string?  Message      { get; set; }
    public bool     IsSuccess    { get; set; }
}

// ── Request ───────────────────────────────────────────────────
public class ElectronicDocumentRequest
{
    public string   DocumentType    { get; set; } = null!;
    public string   BaseDocType     { get; set; } = null!;
    public int?     BaseDocId       { get; set; }
    public string?  BaseDocNo       { get; set; }
    public string?  BaseDocDate     { get; set; }
    public string?  BPCode          { get; set; }
    public string?  BPName          { get; set; }
    public string?  BPTaxId         { get; set; }
    public string?  BPEmail         { get; set; }
    public string   Format          { get; set; } = "PDF";
    public string   Channel         { get; set; } = "Email";
    public string?  SchemaVersion   { get; set; }
    public string?  FileName        { get; set; }
    public string?  FileUrl         { get; set; }
    public decimal? TotalAmount     { get; set; }
    public decimal? TaxAmount       { get; set; }
    public string?  Currency        { get; set; } = "KHR";
    public string?  Remarks         { get; set; }
}

// ── Status-transition actions ──────────────────────────────────
public class SubmitRequest
{
    public string?  SubmittedBy   { get; set; }
    public string?  ExternalRef   { get; set; }
    public string?  SubmissionId  { get; set; }
    public string?  Notes         { get; set; }
}

public class AcknowledgeRequest
{
    public bool    IsAccepted     { get; set; }
    public string? ExternalRef    { get; set; }
    public string? ResponseCode   { get; set; }
    public string? ResponseMessage{ get; set; }
}

// ── Summary / dashboard stats ─────────────────────────────────
public class EDocumentSummary
{
    public int Total        { get; set; }
    public int Draft        { get; set; }
    public int Submitted    { get; set; }
    public int Accepted     { get; set; }
    public int Rejected     { get; set; }
    public int Cancelled    { get; set; }
    public int PendingRetry { get; set; }
}