namespace FarmingApi.Modules.Financials.JournalEntry;

// ═══════════════════════════════════════════════════════════════
// JOURNAL NUMBERING
//
// Every module that posts a journal needs the next JE number, and
// until now each one carried its own copy of the logic — fifteen of
// them, already drifted into four variants. This is the one
// implementation they all call.
//
// Two things the copies got wrong, fixed here:
//
//   • They took the year from DateTime.Now, so a document posted
//     with a back-dated posting date was numbered into the current
//     year. The year now comes from the posting date, which is what
//     the number is meant to describe.
//
//   • They picked the "last" journal with OrderByDescending(Id) and
//     incremented that. Id order and number order only agree while
//     nothing is ever inserted out of sequence; when they disagree
//     the result is a duplicate JrnlNo. The sequence is now taken
//     from the highest NUMBER in the year, not the newest row.
//     JrnlNo carries a unique index, so that collision does not
//     produce a quiet duplicate — it fails the save outright.
// ═══════════════════════════════════════════════════════════════
public interface IJournalEntryNumberService
{
    /// <summary>Next JE-{year}-{00001} for the year the entry posts into.</summary>
    string NextJournalNumber(DateTime postingDate);
}

public class JournalEntryNumberService : IJournalEntryNumberService
{
    private readonly MyDbContext _db;

    public JournalEntryNumberService(MyDbContext db) => _db = db;

    public string NextJournalNumber(DateTime postingDate) => Next(_db, postingDate);

    /// <summary>
    /// The static core. The fifteen posting services that need a number are
    /// constructed with nothing but a DbContext, so they call this directly
    /// rather than each growing a constructor parameter; the interface above
    /// exists for anything resolved through DI.
    /// </summary>
    public static string Next(MyDbContext db, DateTime postingDate) =>
        Next(db.Set<JournalEntry>(), postingDate);

    /// <summary>
    /// Overload for callers holding a repository rather than a context — the
    /// JournalEntry controller is constructed with only IJournalEntryRepository.
    /// </summary>
    public static string Next(IQueryable<JournalEntry> existing, DateTime postingDate)
    {
        var prefix = $"JE-{postingDate.Year}-";

        // Pulling the year's numbers back and taking the max in memory keeps
        // this provider-agnostic; a year of journal numbers is a small list.
        var used = existing
            .Where(j => j.JrnlNo.StartsWith(prefix))
            .Select(j => j.JrnlNo)
            .ToList();

        var highest = 0;
        foreach (var number in used)
        {
            var tail = number[prefix.Length..];
            if (int.TryParse(tail, out var n) && n > highest) highest = n;
        }

        return $"{prefix}{(highest + 1):D5}";
    }
}
