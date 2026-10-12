namespace FarmingApi.Modules.PurchaseAP.LandedCostType;

public class LandedCostTypeDto
{
    public int     Id      { get; set; }
    public string  Name    { get; set; } = null!;
    public string? Remarks { get; set; }
    public bool    Active  { get; set; } = true;
}
