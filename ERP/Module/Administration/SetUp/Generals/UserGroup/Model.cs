using System;

namespace FarmingApi.Modules.Administration.SetUp.UserGroup;

public class UserGroupListResponse
{
    public int Id { get; set; }
    public string ?GroupName { get; set; }
    public string ?GroupDec { get; set; }
    public string? Allowences { get; set; }
    public int? TPLId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string ?GroupType { get; set; }
    public int? CockpitId { get; set; }
    public int? LogInstanc { get; set; }
    public int? UserSign { get; set; }
    public int? UserSign2 { get; set; }
    public int VersionNum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool InActive { get; set; }
}

public class UserGroupListRequest
{
    public string? GroupName { get; set; }
    public string ?GroupDec { get; set; }
    public string? Allowences { get; set; }
    public int? TPLId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string ?GroupType { get; set; }
    public int? CockpitId { get; set; }
}

public class UserGroupUpdateRequest
{
    public string? GroupName { get; set; }
    public string ?GroupDec { get; set; }
    public string ?Allowences { get; set; }
    public int? TPLId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string ?GroupType { get; set; }
    public int? CockpitId { get; set; }
}