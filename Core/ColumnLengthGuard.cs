using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace KamnttraApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// TEXT THAT DOESN'T FIT ITS COLUMN
//
// Request models carry no length limits, so text longer than its column used
// to reach Postgres, fail with 22001 "value too long for type character
// varying(n)", and come back as a 500 with no useful message.
//
// Wired in centrally, from MyDbContext.SaveChanges (next to the other guards):
// before anything is written, every new or changed text value is checked
// against its column's length (varchar(n) / char(n) from the EF model). One
// that doesn't fit is refused with a DocumentRuleException, which controllers
// and DocumentRuleFilter already turn into a 400 with its message.
// ═══════════════════════════════════════════════════════════════════════════
public static class ColumnLengthGuard
{
    private static readonly ConcurrentDictionary<IProperty, int?> Limits = new();
    private static readonly Regex Sized = new(@"^\s*(?:character varying|varchar|character|char|nvarchar|nchar)\s*\(\s*(\d+)\s*\)",
                                              RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static void Apply(DbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
            foreach (var p in entry.Properties)
            {
                if (p.CurrentValue is not string text) continue;
                if (entry.State == EntityState.Modified && !p.IsModified) continue;
                var limit = LimitOf(p.Metadata);
                if (limit is int max && text.Length > max)
                    throw new DocumentRuleException(
                        $"{Words(entry.Metadata.ClrType.Name)}: {Words(p.Metadata.Name)} is too long " +
                        $"({text.Length} characters; the limit is {max}).");
            }
        }
    }

    private static int? LimitOf(IProperty property) => Limits.GetOrAdd(property, p =>
    {
        if (p.GetMaxLength() is int max) return max;
        try
        {
            var m = Sized.Match(p.GetColumnType());
            return m.Success ? int.Parse(m.Groups[1].Value) : null;
        }
        catch (InvalidOperationException)
        {
            return null;   // not mapped to a column
        }
    });

    // "GoodsReceiptPOLine" → "Goods Receipt PO Line", "ItemName" → "Item Name".
    private static string Words(string name) =>
        Regex.Replace(name, @"(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])", " ");
}
