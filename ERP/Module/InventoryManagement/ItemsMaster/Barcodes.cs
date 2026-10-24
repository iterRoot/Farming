using FarmingApi;

namespace FarmingApi.Modules.Inventory.ItemsMaster;

// ══════════════════════════════════════════════════════════════════════════════
// Automatic item barcodes. An item saved without a barcode gets an EAN-13 in the
// GS1 in-store range (prefixes 200–299, reserved for a company's own use, never
// issued to manufacturers): "200" + the item Id in 9 digits + check digit.
// Built from the Id, so every item gets a different code without a counter.
// A barcode typed or scanned in by the user is kept as-is.
// ══════════════════════════════════════════════════════════════════════════════
public static class ItemBarcodes
{
    public const int InStorePrefix = 200;

    // EAN-13 check digit for the first 12 digits (weights 1,3,1,3,… from the left).
    public static char CheckDigit(string twelveDigits)
    {
        var sum = 0;
        for (var i = 0; i < 12; i++) sum += (twelveDigits[i] - '0') * (i % 2 == 0 ? 1 : 3);
        return (char)('0' + (10 - sum % 10) % 10);
    }

    // attempt 0 → prefix 200, attempt 1 → 201 …: only used if a hand-typed code already took the first choice.
    public static string ForItem(int itemId, int attempt = 0)
    {
        var body = (InStorePrefix + attempt).ToString() + itemId.ToString("D9");
        return body + CheckDigit(body);
    }

    // Gives every saved item in `items` (or, when null, every item in the table) that has
    // no barcode a generated one. Items must already have their Id. Returns how many changed;
    // the caller saves.
    public static int FillMissing(MyDbContext db, IEnumerable<ItemsMaster>? items = null)
    {
        var targets = (items ?? db.Set<ItemsMaster>().Where(x => x.Barcode == null || x.Barcode == "").ToList())
            .Where(x => x.Id > 0 && string.IsNullOrWhiteSpace(x.Barcode))
            .ToList();
        if (targets.Count == 0) return 0;

        var taken = db.Set<ItemsMaster>()
            .Where(x => x.Barcode != null && x.Barcode != "")
            .Select(x => x.Barcode)
            .ToHashSet();

        foreach (var item in targets)
        {
            string code;
            var attempt = 0;
            do code = ForItem(item.Id, attempt++); while (taken.Contains(code) && attempt < 100);
            item.Barcode = code;
            taken.Add(code);
        }
        return targets.Count;
    }

    // Error text when `barcode` already belongs to another item, else null.
    public static string? CheckUnique(MyDbContext db, string? barcode, int? exceptItemId)
    {
        var code = (barcode ?? "").Trim();
        if (code.Length == 0) return null;
        var other = db.Set<ItemsMaster>()
            .Where(x => x.Barcode == code && (exceptItemId == null || x.Id != exceptItemId))
            .Select(x => x.ItemCode)
            .FirstOrDefault();
        return other == null ? null : $"Barcode '{code}' is already used by item {other}.";
    }
}
