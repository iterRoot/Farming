namespace FarmingApi.Modules.Administration.ElectronicCertificates;

// ── Response ──────────────────────────────────────────────────
public class ElectronicCertificateResponse
{
    public int      Id                { get; set; }
    public string   CertNo            { get; set; } = null!;
    public string   CertName          { get; set; } = null!;
    public string   CertType          { get; set; } = null!;
    public string?  CertStandard      { get; set; }
    public string?  SerialNumber      { get; set; }
    public string?  CertBody          { get; set; }
    public string   HolderType        { get; set; } = null!;
    public string?  HolderCode        { get; set; }
    public string   HolderName        { get; set; } = null!;
    public string?  HolderDept        { get; set; }
    public string?  IssuingAuthority  { get; set; }
    public string?  IssuingCountry    { get; set; }
    public string?  AuthorityContact  { get; set; }
    public DateTime IssuedDate        { get; set; }
    public DateTime? ExpiryDate       { get; set; }
    public DateTime? RenewalDueDate   { get; set; }
    public int?     RenewalDaysAlert  { get; set; }
    public DateTime? VerifiedAt       { get; set; }
    public string?  VerifiedBy        { get; set; }
    public string   Status            { get; set; } = null!;
    public string?  RevocationReason  { get; set; }
    public DateTime? RevokedAt        { get; set; }
    public int?     RenewedFromId     { get; set; }
    public int?     RenewedToId       { get; set; }
    public string?  FileUrl           { get; set; }
    public string?  FileName          { get; set; }
    public string?  ThumbnailUrl      { get; set; }
    public string?  QrCodeData        { get; set; }
    public string?  VerificationUrl   { get; set; }
    public bool     IsSigned          { get; set; }
    public string?  SignatureCert     { get; set; }
    public string?  SignatureHash     { get; set; }
    public DateTime? SignedAt         { get; set; }
    public string?  LinkedModule      { get; set; }
    public string?  LinkedDocType     { get; set; }
    public int?     LinkedDocId       { get; set; }
    public string?  LinkedDocNo       { get; set; }
    public string?  Scope             { get; set; }
    public string?  CoveredSites      { get; set; }
    public string?  Restrictions      { get; set; }
    public string?  Tags              { get; set; }
    public bool     IsPublic          { get; set; }
    public bool     IsActive          { get; set; }
    public string?  Remarks           { get; set; }
    public DateTime CreatedAt         { get; set; }
    public DateTime UpdatedAt         { get; set; }
    // Computed
    public bool     IsExpired         { get; set; }
    public bool     IsExpiringSoon    { get; set; }
    public int?     DaysUntilExpiry   { get; set; }
    public List<CertificateAttributeResponse> Attributes { get; set; } = new();
}

public class CertificateAttributeResponse
{
    public int     Id         { get; set; }
    public string  AttrKey    { get; set; } = null!;
    public string  AttrLabel  { get; set; } = null!;
    public string? AttrValue  { get; set; }
    public string  DataType   { get; set; } = null!;
    public int     SortOrder  { get; set; }
}

// ── Request ───────────────────────────────────────────────────
public class ElectronicCertificateRequest
{
    public string   CertName          { get; set; } = null!;
    public string   CertType          { get; set; } = null!;
    public string?  CertStandard      { get; set; }
    public string?  SerialNumber      { get; set; }
    public string?  CertBody          { get; set; }
    public string   HolderType        { get; set; } = "Company";
    public string?  HolderCode        { get; set; }
    public string   HolderName        { get; set; } = null!;
    public string?  HolderDept        { get; set; }
    public string?  IssuingAuthority  { get; set; }
    public string?  IssuingCountry    { get; set; }
    public string?  AuthorityContact  { get; set; }
    public DateTime IssuedDate        { get; set; }
    public DateTime? ExpiryDate       { get; set; }
    public DateTime? RenewalDueDate   { get; set; }
    public int?     RenewalDaysAlert  { get; set; } = 90;
    public string?  FileUrl           { get; set; }
    public string?  FileName          { get; set; }
    public string?  ThumbnailUrl      { get; set; }
    public string?  QrCodeData        { get; set; }
    public string?  VerificationUrl   { get; set; }
    public string?  LinkedModule      { get; set; }
    public string?  LinkedDocType     { get; set; }
    public int?     LinkedDocId       { get; set; }
    public string?  LinkedDocNo       { get; set; }
    public string?  Scope             { get; set; }
    public string?  CoveredSites      { get; set; }
    public string?  Restrictions      { get; set; }
    public string?  Tags              { get; set; }
    public bool     IsPublic          { get; set; } = false;
    public bool     IsActive          { get; set; } = true;
    public string?  Remarks           { get; set; }
    public List<CertificateAttributeRequest> Attributes { get; set; } = new();
}

public class CertificateAttributeRequest
{
    public string  AttrKey   { get; set; } = null!;
    public string  AttrLabel { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string  DataType  { get; set; } = "Text";
    public int     SortOrder { get; set; } = 0;
}

// Renewal / revocation
public class RenewRequest
{
    public DateTime  NewIssuedDate  { get; set; }
    public DateTime? NewExpiryDate  { get; set; }
    public string?   NewSerialNumber{ get; set; }
    public string?   NewFileUrl     { get; set; }
    public string?   Notes          { get; set; }
}

public class RevokeRequest
{
    public string  Reason    { get; set; } = null!;
    public string? RevokedBy { get; set; }
}

// Summary / dashboard
public class CertSummary
{
    public int Total          { get; set; }
    public int Active         { get; set; }
    public int Expired        { get; set; }
    public int ExpiringSoon   { get; set; }  // within RenewalDaysAlert
    public int Pending        { get; set; }
    public int Revoked        { get; set; }
    public Dictionary<string, int> ByType   { get; set; } = new();
    public Dictionary<string, int> ByHolder { get; set; } = new();
}