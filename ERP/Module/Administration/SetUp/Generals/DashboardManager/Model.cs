namespace FarmingApi.Modules.Administration.DashboardManager;

// ── Responses ─────────────────────────────────────────────────
public class DashboardConfigResponse
{
    public int      Id          { get; set; }
    public string   Name        { get; set; } = null!;
    public string?  Description { get; set; }
    public string   RoleTarget  { get; set; } = null!;
    public string   LayoutType  { get; set; } = null!;
    public int      Columns     { get; set; }
    public bool     IsDefault   { get; set; }
    public bool     IsActive    { get; set; }
    public string?  ThemeColor  { get; set; }
    public string?  Remarks     { get; set; }
    public DateTime CreatedAt   { get; set; }
    public DateTime UpdatedAt   { get; set; }
    public List<DashboardWidgetResponse> Widgets { get; set; } = new();
}

public class DashboardWidgetResponse
{
    public int     Id          { get; set; }
    public int     Position    { get; set; }
    public string  WidgetKey   { get; set; } = null!;
    public string  Title       { get; set; } = null!;
    public string  Module      { get; set; } = null!;
    public string  WidgetType  { get; set; } = null!;
    public string? Icon        { get; set; }
    public string? Route       { get; set; }
    public int     ColSpan     { get; set; }
    public bool    IsVisible   { get; set; }
    public string? Settings    { get; set; }
}

// ── Requests ──────────────────────────────────────────────────
public class DashboardConfigRequest
{
    public string   Name        { get; set; } = null!;
    public string?  Description { get; set; }
    public string   RoleTarget  { get; set; } = "All";
    public string   LayoutType  { get; set; } = "Grid";
    public int      Columns     { get; set; } = 3;
    public bool     IsDefault   { get; set; } = false;
    public bool     IsActive    { get; set; } = true;
    public string?  ThemeColor  { get; set; }
    public string?  Remarks     { get; set; }
    public List<DashboardWidgetRequest> Widgets { get; set; } = new();
}

public class DashboardWidgetRequest
{
    public int     Position   { get; set; }
    public string  WidgetKey  { get; set; } = null!;
    public string  Title      { get; set; } = null!;
    public string  Module     { get; set; } = null!;
    public string  WidgetType { get; set; } = "Card";
    public string? Icon       { get; set; }
    public string? Route      { get; set; }
    public int     ColSpan    { get; set; } = 1;
    public bool    IsVisible  { get; set; } = true;
    public string? Settings   { get; set; }
}

// Widget layout reorder (drag & drop)
public class WidgetReorderRequest
{
    public List<WidgetPositionItem> Items { get; set; } = new();
}

public class WidgetPositionItem
{
    public int WidgetId  { get; set; }
    public int Position  { get; set; }
    public bool IsVisible{ get; set; }
}