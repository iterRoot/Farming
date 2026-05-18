namespace FarmingApi.Modules.BusinessPartners;

// ═══════════════════════════════════════════════════════════════════
// SUB DTOs — Address / Contact / Bank
// ═══════════════════════════════════════════════════════════════════

public class BPAddressDto
{
    public int     Id          { get; set; }
    public int     AdresType   { get; set; }    // 0=Ship-To, 1=Bill-To
    public string? AdressName  { get; set; }
    public string? Street      { get; set; }
    public string? Block       { get; set; }
    public string? City        { get; set; }
    public string? ZipCode     { get; set; }
    public string? County      { get; set; }
    public string? Country     { get; set; }
    public string? State       { get; set; }
    public bool    IsDefault   { get; set; }
}

public class BPContactDto
{
    public int     Id          { get; set; }
    public string? Name        { get; set; }
    public string? Position    { get; set; }
    public string? Tel1        { get; set; }
    public string? MobilePhone { get; set; }
    public string? Email       { get; set; }
    public string? Note        { get; set; }
    public bool    IsDefault   { get; set; }
}

public class BPBankAccountDto
{
    public int     Id          { get; set; }
    public string? BankCode    { get; set; }
    public string? BankName    { get; set; }
    public string? AccountNo   { get; set; }
    public string? Branch      { get; set; }
    public string? Currency    { get; set; }
    public string? Iban        { get; set; }
    public string? SwiftNum    { get; set; }
    public bool    IsDefault   { get; set; }
}

// ═══════════════════════════════════════════════════════════════════
// CREATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BPCreateRequest
{
    // ── Core ─────────────────────────────────────────────────────
    public string Code { get; set; } = null!;
    public string CardName { get; set; } = null!;       // Full name / company name
    public string? FrgnName { get; set; }

    // ── Type & Status ────────────────────────────────────────────
    public int TypeStatus   { get; set; }               // 0=Customer, 1=Vendor, 2=Lead
    public int ActiveStatus { get; set; }               // 0=Active, 1=Inactive

    // ── Contact Info ─────────────────────────────────────────────
    public string? Tel1        { get; set; }
    public string? Tel2        { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax         { get; set; }
    public string? Email       { get; set; }
    public string? Website     { get; set; }
    public string? Note        { get; set; }

    // ── Financial ────────────────────────────────────────────────
    public string?  Currency    { get; set; }
    public decimal  CreditLimit { get; set; }
    public string?  PayTerms    { get; set; }
    public string?  PriceList   { get; set; }
    public string?  GroupCode   { get; set; }

    // ── Tax ──────────────────────────────────────────────────────
    public string? TaxId    { get; set; }
    public string? VatGroup { get; set; }

    // ── Sub-tables ───────────────────────────────────────────────
    public List<BPAddressDto>     Addresses { get; set; } = new();
    public List<BPContactDto>     Contacts  { get; set; } = new();
    public List<BPBankAccountDto> BankAccts { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// UPDATE REQUEST
// ═══════════════════════════════════════════════════════════════════
public class BPUpdateRequest
{
    // Code is immutable — not included
    public string CardName  { get; set; } = null!;
    public string? FrgnName { get; set; }

    public int TypeStatus   { get; set; }
    public int ActiveStatus { get; set; }

    public string? Tel1        { get; set; }
    public string? Tel2        { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax         { get; set; }
    public string? Email       { get; set; }
    public string? Website     { get; set; }
    public string? Note        { get; set; }

    public string?  Currency    { get; set; }
    public decimal  CreditLimit { get; set; }
    public string?  PayTerms    { get; set; }
    public string?  PriceList   { get; set; }
    public string?  GroupCode   { get; set; }

    public string? TaxId    { get; set; }
    public string? VatGroup { get; set; }

    // ── Sub-tables (full replace on update) ──────────────────────
    public List<BPAddressDto>     Addresses { get; set; } = new();
    public List<BPContactDto>     Contacts  { get; set; } = new();
    public List<BPBankAccountDto> BankAccts { get; set; } = new();
}

// ═══════════════════════════════════════════════════════════════════
// RESPONSE DTO
// ═══════════════════════════════════════════════════════════════════
public class BPResponse
{
    public int    Id           { get; set; }
    public string Code         { get; set; } = null!;
    public string CardName     { get; set; } = null!;
    public string? FrgnName    { get; set; }

    // ── Type & Status (returns int + label) ──────────────────────
    public int    TypeStatus        { get; set; }
    public string TypeStatusLabel   { get; set; } = null!;   // "Customer" / "Vendor" / "Lead"
    public int    ActiveStatus      { get; set; }
    public string ActiveStatusLabel { get; set; } = null!;   // "Active" / "Inactive"

    // ── Contact ───────────────────────────────────────────────────
    public string? Tel1        { get; set; }
    public string? Tel2        { get; set; }
    public string? MobilePhone { get; set; }
    public string? Fax         { get; set; }
    public string? Email       { get; set; }
    public string? Website     { get; set; }
    public string? Note        { get; set; }

    // ── Financial ─────────────────────────────────────────────────
    public string?  Currency    { get; set; }
    public decimal  Balance     { get; set; }
    public decimal  CreditLimit { get; set; }
    public string?  PayTerms    { get; set; }
    public string?  PriceList   { get; set; }
    public string?  GroupCode   { get; set; }

    // ── Tax ───────────────────────────────────────────────────────
    public string? TaxId    { get; set; }
    public string? VatGroup { get; set; }

    // ── Sub-tables ────────────────────────────────────────────────
    public List<BPAddressDto>     Addresses { get; set; } = new();
    public List<BPContactDto>     Contacts  { get; set; } = new();
    public List<BPBankAccountDto> BankAccts { get; set; } = new();

    // ── Audit ─────────────────────────────────────────────────────
    public int       VersionNum { get; set; }
    public DateTime  CreatedAt  { get; set; }
    public DateTime? UpdatedAt  { get; set; }
}

// ── List response (lightweight, no sub-tables) ────────────────────
public class BPListResponse
{
    public int    Id                { get; set; }
    public string Code              { get; set; } = null!;
    public string CardName          { get; set; } = null!;
    public int    TypeStatus        { get; set; }
    public string TypeStatusLabel   { get; set; } = null!;
    public int    ActiveStatus      { get; set; }
    public string ActiveStatusLabel { get; set; } = null!;
    public string? Tel1             { get; set; }
    public string? MobilePhone      { get; set; }
    public string? Email            { get; set; }
    public decimal Balance          { get; set; }
    public DateTime CreatedAt       { get; set; }
}