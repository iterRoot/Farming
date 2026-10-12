namespace FarmingApi.Modules.Administration.SetUp.Generals.SavedSearch;

public class SearchFieldOption
{
    public string   Name      { get; set; } = null!;
    public string   Label     { get; set; } = null!;
    public string   Type      { get; set; } = null!;   // "Text" | "Number" | "Boolean"
    public string[] Operators { get; set; } = Array.Empty<string>();
}

public class SearchTargetOption
{
    public string Key    { get; set; } = null!;
    public string Label   { get; set; } = null!;
    public List<SearchFieldOption> Fields { get; set; } = new();
}

public class SavedSearchFilterResponse
{
    public int    Id       { get; set; }
    public int    LineNum  { get; set; }
    public string Field    { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value    { get; set; } = null!;
}

public class SavedSearchResponse
{
    public int      Id           { get; set; }
    public string   Name         { get; set; } = null!;
    public string   TargetEntity { get; set; } = null!;
    public string?  Description  { get; set; }
    public bool     IsActive     { get; set; }
    public DateTime CreatedAt    { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public List<SavedSearchFilterResponse> Filters { get; set; } = new();
}

public class SavedSearchFilterRequest
{
    public string Field    { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value    { get; set; } = null!;
}

public class SavedSearchRequest
{
    public string   Name         { get; set; } = null!;
    public string   TargetEntity { get; set; } = null!;
    public string?  Description  { get; set; }
    public bool     IsActive     { get; set; } = true;
    public List<SavedSearchFilterRequest> Filters { get; set; } = new();
}

// Run an unsaved filter set directly — what the Create page's live preview uses.
public class SearchPreviewRequest
{
    public string TargetEntity { get; set; } = null!;
    public List<SavedSearchFilterRequest> Filters { get; set; } = new();
}

public class SearchResultRow
{
    public int    Id      { get; set; }
    public string Display { get; set; } = null!;
}
