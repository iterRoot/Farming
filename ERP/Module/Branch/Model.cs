namespace FarmingApi.Modules.Branch;

public class BranchListResponse
{
    public int     Id                { get; set; }
    public string  BranchCode        { get; set; } = null!;
    public string  BranchName        { get; set; } = null!;
    public string? BranchNameForeign { get; set; }
    public string? BranchRegNo       { get; set; }
    public string? Address           { get; set; }
    public string? AddressForeign    { get; set; }

    public bool    MainBranch  { get; set; }
    public string? TaxOfficeNo { get; set; }
    public bool    Disabled    { get; set; }

    public string? DefaultCustomerId          { get; set; }
    public string? DefaultSupplierId          { get; set; }
    public string? DefaultWarehouseId         { get; set; }
    public string? DefaultResourceWarehouseId { get; set; }

    public string? AliasName            { get; set; }
    public string? AddressType          { get; set; }
    public string? Street               { get; set; }
    public string? StreetNo             { get; set; }
    public string? BuildingFloorRoom    { get; set; }
    public string? Postcode             { get; set; }
    public string? Block                { get; set; }
    public string? City                 { get; set; }
    public string? State                { get; set; }
    public string? County               { get; set; }
    public string? CountryRegion        { get; set; }
    public string? GlobalLocationNumber { get; set; }

    public DateTime  CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BranchListRequest
{
    /// <summary>Optional — auto-generated from the branch name when omitted.</summary>
    public string? BranchCode        { get; set; }
    public string  BranchName        { get; set; } = null!;
    public string? BranchNameForeign { get; set; }
    public string? BranchRegNo       { get; set; }
    public string? Address           { get; set; }
    public string? AddressForeign    { get; set; }

    public bool    MainBranch  { get; set; }
    public string? TaxOfficeNo { get; set; }
    public bool    Disabled    { get; set; }

    public string? DefaultCustomerId          { get; set; }
    public string? DefaultSupplierId          { get; set; }
    public string? DefaultWarehouseId         { get; set; }
    public string? DefaultResourceWarehouseId { get; set; }

    public string? AliasName            { get; set; }
    public string? AddressType          { get; set; }
    public string? Street               { get; set; }
    public string? StreetNo             { get; set; }
    public string? BuildingFloorRoom    { get; set; }
    public string? Postcode             { get; set; }
    public string? Block                { get; set; }
    public string? City                 { get; set; }
    public string? State                { get; set; }
    public string? County               { get; set; }
    public string? CountryRegion        { get; set; }
    public string? GlobalLocationNumber { get; set; }
}

/// <summary>Update carries the same shape; BranchCode stays immutable.</summary>
public class BranchUpdateRequest : BranchListRequest { }
