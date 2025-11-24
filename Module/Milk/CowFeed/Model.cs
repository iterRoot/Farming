
namespace FarmingApi.Modules.CowFeed;

public class CowFeedListResponse
{
    public Guid CowGuidId { get; set; }
    public int Id { get; set; }

    public string FoodType { get; set; } = null!;
    public string? FoodName { get; set; }
    public string? FoodSource { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "kg";

    public DateTime FeedingTime { get; set; }
    public string FeedingSession { get; set; } = "Morning";
    public string? Reason { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? TotalCost { get; set; }
}

public class CowFeedListRequest
{
    // public Guid CowGuidId { get; set; }
    // public int CowId { get; set; }

    public string FoodType { get; set; } = null!;
    public string? FoodName { get; set; }
    public string? FoodSource { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "kg";

    public DateTime FeedingTime { get; set; }
    public string FeedingSession { get; set; } = "Morning";
    public string? Reason { get; set; }

    public decimal? UnitPrice { get; set; }
    public decimal? TotalCost { get; set; }
}