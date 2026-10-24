namespace FarmingApi.Modules.SaleAR.DunningTerm;

public class DunningTermLevelRequest
{
    public int    Level       { get; set; }
    public int    DaysOverdue { get; set; }
    public string LetterText  { get; set; } = null!;
}

public class DunningTermLevelResponse
{
    public int    Id          { get; set; }
    public int    Level       { get; set; }
    public int    DaysOverdue { get; set; }
    public string LetterText  { get; set; } = null!;
}

public class DunningTermRequest
{
    public string Code     { get; set; } = null!;
    public string Name     { get; set; } = null!;
    public bool   IsActive { get; set; } = true;
    public string? Remarks { get; set; }
    public List<DunningTermLevelRequest> Levels { get; set; } = new();
}

public class DunningTermResponse
{
    public int      Id        { get; set; }
    public string   Code      { get; set; } = null!;
    public string   Name      { get; set; } = null!;
    public bool     IsActive  { get; set; }
    public string?  Remarks   { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<DunningTermLevelResponse> Levels { get; set; } = new();
}
