namespace FarmingApi.Modules.Administration.AddressFormats;

// ── Responses ─────────────────────────────────────────────────
public class AddressFormatResponse
{
    public int      Id          { get; set; }
    public string   Code        { get; set; } = null!;
    public string   Name        { get; set; } = null!;
    public string?  CountryCode { get; set; }
    public string?  Description { get; set; }
    public bool     IsDefault   { get; set; }
    public bool     IsActive    { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
    public List<AddressFormatLineResponse> Lines { get; set; } = new();
}

public class AddressFormatLineResponse
{
    public int     Id           { get; set; }
    public int     LineOrder    { get; set; }
    public string? Field1       { get; set; }
    public string? Field2       { get; set; }
    public string? Field3       { get; set; }
    public string? Separator12  { get; set; }
    public string? Separator23  { get; set; }
    public bool    PrintIfEmpty { get; set; }
}

// ── Requests ──────────────────────────────────────────────────
public class AddressFormatRequest
{
    public string  Code        { get; set; } = null!;
    public string  Name        { get; set; } = null!;
    public string? CountryCode { get; set; }
    public string? Description { get; set; }
    public bool    IsDefault   { get; set; } = false;
    public bool    IsActive    { get; set; } = true;
    public string? Remarks     { get; set; }
    public List<AddressFormatLineRequest> Lines { get; set; } = new();
}

public class AddressFormatLineRequest
{
    public int     LineOrder    { get; set; }
    public string? Field1       { get; set; }
    public string? Field2       { get; set; }
    public string? Field3       { get; set; }
    public string? Separator12  { get; set; } = ", ";
    public string? Separator23  { get; set; } = " ";
    public bool    PrintIfEmpty { get; set; } = false;
}

// ── Address preview helper ─────────────────────────────────────
// Used by the preview endpoint to render a sample address
public class AddressPreviewRequest
{
    public string? Attention   { get; set; }
    public string? CompanyName { get; set; }
    public string? Street      { get; set; }
    public string? HouseNumber { get; set; }
    public string? Building    { get; set; }
    public string? Floor       { get; set; }
    public string? Apartment   { get; set; }
    public string? Commune     { get; set; }
    public string? District    { get; set; }
    public string? Province    { get; set; }
    public string? City        { get; set; }
    public string? PostalCode  { get; set; }
    public string? StateRegion { get; set; }
    public string? Country     { get; set; }
    public string? POBox       { get; set; }
}

public class AddressPreviewResponse
{
    public List<string> Lines       { get; set; } = new();
    public string       Formatted   { get; set; } = null!;  // newline-joined
    public string       SingleLine  { get; set; } = null!;  // comma-joined
}