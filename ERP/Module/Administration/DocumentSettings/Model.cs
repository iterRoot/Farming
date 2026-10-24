using System.Text.Json;

namespace FarmingApi.Modules.Administration.DocumentSettings;

public class DocumentTypeSettingDto
{
    public string DocumentType { get; set; } = null!;
    public bool   Configured   { get; set; }
    public Dictionary<string, JsonElement> Settings { get; set; } = new();
}

public class DocumentSettingsResponse
{
    public Dictionary<string, JsonElement> General   { get; set; } = new();
    public List<DocumentTypeSettingDto>    Documents { get; set; } = new();
}

public class DocumentTypeSettingSaveRequest
{
    public Dictionary<string, JsonElement> Settings { get; set; } = new();
}
