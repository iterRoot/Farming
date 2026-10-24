namespace FarmingApi.Modules.Sales.CommissionGroup;

// ── Commission Group ──────────────────────────────────────────
public class CommissionGroupResponse
{
    public int      Id             { get; set; }
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string   CommissionType { get; set; } = null!;
    public decimal  CommissionRate { get; set; }
    public decimal? MinSaleAmount  { get; set; }
    public decimal? MaxCommission  { get; set; }
    public string?  Currency       { get; set; }
    public string?  Description    { get; set; }
    public bool     IsActive       { get; set; }
    public string?  Remarks        { get; set; }
    public DateTime CreatedAt      { get; set; }
    public DateTime UpdatedAt      { get; set; }
    public List<CommissionTierResponse> Tiers { get; set; } = new();
}

public class CommissionTierResponse
{
    public int     Id         { get; set; }
    public int     TierOrder  { get; set; }
    public decimal FromAmount { get; set; }
    public decimal ToAmount   { get; set; }
    public decimal Rate       { get; set; }
}

// ── Requests ──────────────────────────────────────────────────
public class CommissionGroupRequest
{
    public string   Code           { get; set; } = null!;
    public string   Name           { get; set; } = null!;
    public string   CommissionType { get; set; } = "Percentage";
    public decimal  CommissionRate { get; set; }
    public decimal? MinSaleAmount  { get; set; }
    public decimal? MaxCommission  { get; set; }
    public string?  Currency       { get; set; } = "KHR";
    public string?  Description    { get; set; }
    public bool     IsActive       { get; set; } = true;
    public string?  Remarks        { get; set; }
    public List<CommissionTierRequest> Tiers { get; set; } = new();
}

public class CommissionTierRequest
{
    public decimal FromAmount { get; set; }
    public decimal ToAmount   { get; set; }
    public decimal Rate       { get; set; }
}