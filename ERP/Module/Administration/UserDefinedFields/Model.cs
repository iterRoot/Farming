namespace FarmingApi.Modules.Administration.UserDefinedFields;

public class UserDefinedFieldRequest
{
    public string  TableName    { get; set; } = null!;
    public string  FieldName    { get; set; } = null!;
    public string  Label        { get; set; } = null!;
    public string  FieldType    { get; set; } = "Text";
    public int?    Length       { get; set; }
    public string? DefaultValue { get; set; }
    public List<string> ValidValues { get; set; } = new();
    public bool    Mandatory    { get; set; }
    public int     SortOrder    { get; set; }
    public bool    Active       { get; set; } = true;
}

public class UserDefinedFieldUpdateRequest : UserDefinedFieldRequest { }

public class UserDefinedFieldResponse : UserDefinedFieldRequest
{
    public int Id { get; set; }
}
