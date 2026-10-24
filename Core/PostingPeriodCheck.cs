using Microsoft.EntityFrameworkCore;
using PostingPeriodEntity = FarmingApi.Modules.Administration.PostingPeriods.PostingPeriod;

namespace FarmingApi.Core;

// For actions that post on a date without carrying it in a request body (so
// PostingPeriodGuardFilter can't see it): the same Locked-period rule, asked directly.
public static class PostingPeriodCheck
{
    public static string? LockError(MyDbContext db, DateTime date, string category = "Inventory")
    {
        var d = DateOnly.FromDateTime(date);
        var period = db.Set<PostingPeriodEntity>().AsNoTracking()
            .FirstOrDefault(p => p.PostingDateFrom != null && p.PostingDateTo != null
                              && p.PostingDateFrom <= d && p.PostingDateTo >= d);
        if (period == null) return null;
        var blocked = period.PeriodStatus switch
        {
            "Locked" => true,
            "Locked Except Sales" => category != "Sales",
            "Closing Period" => category != "Financial",
            _ => false,
        };
        return blocked ? $"The posting period for {date:yyyy-MM-dd} is {period.PeriodStatus}." : null;
    }
}
