namespace FarmingApi.Modules.Administration.MenuAlias;

public class MenuAliasDto
{
    public int     Id       { get; set; }
    public string  Alias    { get; set; } = null!;
    public string  MenuName { get; set; } = null!;
    public string? MenuPath { get; set; }
    public string? Module   { get; set; }
    public string? Language { get; set; }
    public bool    InActive { get; set; }
}

public class MenuAliasSaveRequest
{
    public string  Alias    { get; set; } = null!;
    public string  MenuName { get; set; } = null!;
    public string? MenuPath { get; set; }
    public string? Module   { get; set; }
    public string? Language { get; set; }
    public bool    InActive { get; set; }
}
