namespace FarmingApi.Modules.Administration.PostingPeriods;

public class PostingPeriodDto
{
    public int?      Id               { get; set; }
    public string    PeriodCode       { get; set; } = null!;
    public string?   PeriodName       { get; set; }
    public string?   PeriodStatus     { get; set; }
    public DateOnly? PostingDateFrom  { get; set; }
    public DateOnly? PostingDateTo    { get; set; }
    public DateOnly? DueDateFrom      { get; set; }
    public DateOnly? DueDateTo        { get; set; }
    public DateOnly? DocumentDateFrom { get; set; }
    public DateOnly? DocumentDateTo   { get; set; }
}

/// <summary>Whole-screen payload: the period grid plus the options below it.</summary>
public class PostingPeriodsSaveRequest
{
    public List<PostingPeriodDto> Periods { get; set; } = new();
    public bool CreateNextYearDueDates { get; set; }
    public bool AutoUpdateStatus       { get; set; }
    public int  DaysAfterNewPeriod     { get; set; }
}

public class PostingPeriodsResponse
{
    public List<PostingPeriodDto> Items { get; set; } = new();
    public bool CreateNextYearDueDates { get; set; }
    public bool AutoUpdateStatus       { get; set; }
    public int  DaysAfterNewPeriod     { get; set; }
}
