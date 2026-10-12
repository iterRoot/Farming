using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Inventory.ItemsMaster;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// WriteOff is an issue that may take expired batches (scrapping stock, returning it to the vendor).
public enum BatchFlow { In, ReturnIn, Out, WriteOff }

// Running total of what a document has already claimed, so a document with two lines
// for the same batch is checked against the stock that's left after the first line.
public class BatchTally
{
    internal Dictionary<string, decimal> Delta = new(StringComparer.OrdinalIgnoreCase);
}

// The rules for batch- and serial-managed stock. Used by the posting service both to check
// a whole document before anything is saved and to apply each line's numbers.
public class BatchTracker
{
    private readonly MyDbContext _db;
    public BatchTracker(MyDbContext db) => _db = db;

    public static string? KindOf(ItemsMaster item)
    {
        var m = (item.ManageBy ?? "").Trim();
        if (m.StartsWith("Batch", StringComparison.OrdinalIgnoreCase)) return "Batch";
        if (m.StartsWith("Serial", StringComparison.OrdinalIgnoreCase)) return "Serial";
        return null;
    }

    private static bool Leaves(BatchFlow f) => f == BatchFlow.Out || f == BatchFlow.WriteOff;

    private static DateTime Day(DateTime d) => DateTime.SpecifyKind(d.Date, DateTimeKind.Utc);
    private static DateTime? Day(DateTime? d) => d == null ? null : Day(d.Value);
    private static DateTime Read(DateTime d) => d.ToUniversalTime().Date;   // stored values come back in local kind

    // ── lookups: unsaved rows first, then the database ──
    private async Task<StockBatch?> FindBatch(string item, string whs, string no)
    {
        var local = _db.Set<StockBatch>().Local.FirstOrDefault(b => b.Kind == "Batch" && b.ItemCode == item
            && string.Equals(b.WhsCode, whs, StringComparison.OrdinalIgnoreCase) && string.Equals(b.BatchNo, no, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;
        var up = no.ToUpperInvariant(); var w = whs.ToUpperInvariant();
        return await _db.Set<StockBatch>().FirstOrDefaultAsync(b => b.Kind == "Batch" && b.ItemCode == item && b.WhsCode.ToUpper() == w && b.BatchNo.ToUpper() == up);
    }

    private async Task<StockBatch?> FindSerial(string item, string no)
    {
        var local = _db.Set<StockBatch>().Local.FirstOrDefault(b => b.Kind == "Serial" && b.ItemCode == item
            && string.Equals(b.BatchNo, no, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;
        var up = no.ToUpperInvariant();
        return await _db.Set<StockBatch>().FirstOrDefaultAsync(b => b.Kind == "Serial" && b.ItemCode == item && b.BatchNo.ToUpper() == up);
    }

    private async Task<StockBatch?> AnyBatchRow(string item, string no)
    {
        var local = _db.Set<StockBatch>().Local.FirstOrDefault(b => b.Kind == "Batch" && b.ItemCode == item
            && string.Equals(b.BatchNo, no, StringComparison.OrdinalIgnoreCase));
        if (local != null) return local;
        var up = no.ToUpperInvariant();
        return await _db.Set<StockBatch>().FirstOrDefaultAsync(b => b.Kind == "Batch" && b.ItemCode == item && b.BatchNo.ToUpper() == up);
    }

    private static decimal Get(BatchTally? t, string key) => t != null && t.Delta.TryGetValue(key, out var d) ? d : 0m;
    private static void Add(BatchTally? t, string key, decimal v) { if (t != null) t.Delta[key] = Get(t, key) + v; }

    // Returns an error message, or null when the numbers are acceptable. With a tally the
    // claim is recorded so later lines of the same document see it.
    public async Task<string?> ValidateAsync(BatchTally? tally, ItemsMaster item, string whs, decimal qty,
        IReadOnlyList<BatchLine>? lines, BatchFlow flow, DateTime date)
    {
        var kind = KindOf(item);
        var have = lines != null && lines.Count > 0;
        if (kind == null)
            return have ? $"{item.ItemCode} isn't managed by batch or serial numbers, so numbers can't be entered for it." : null;
        if (!have) return $"{item.ItemCode} is managed by {kind.ToLower()} numbers — enter them on its line.";
        if (qty <= 0) return null;

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        decimal sum = 0m;
        var norm = new List<(string No, decimal Qty, BatchLine Src)>();
        foreach (var l in lines!)
        {
            var no = (l.BatchNo ?? "").Trim();
            if (no.Length == 0) return $"A {kind.ToLower()} number is missing on {item.ItemCode}.";
            if (no.Length > 100) return $"{kind} number '{no[..20]}…' is longer than 100 characters.";
            if (no.Any(char.IsControl)) return $"{kind} number '{no}' has characters that aren't allowed.";
            if (!seen.Add(no)) return $"{kind} number '{no}' appears twice on the {item.ItemCode} line.";
            var q = kind == "Serial" ? (l.Quantity == 0 ? 1m : l.Quantity) : l.Quantity;
            if (kind == "Serial" && q != 1m) return $"Serial numbers are one unit each ('{no}' has {q:0.####}).";
            if (q <= 0) return $"Quantity for {kind.ToLower()} '{no}' must be greater than zero.";
            if (l.ExpiryDate != null && l.ManufacturingDate != null && l.ExpiryDate.Value.Date < l.ManufacturingDate.Value.Date)
                return $"{kind} '{no}' expires before it was made.";
            sum += q;
            norm.Add((no, q, l));
        }
        if (Math.Abs(sum - qty) > 0.0001m)
            return $"{item.ItemCode} needs {qty:0.####} but the {kind.ToLower()} numbers add up to {sum:0.####}.";
        if (kind == "Serial" && qty != Math.Floor(qty)) return $"{item.ItemCode} is serial-managed, so its quantity must be a whole number.";

        foreach (var (no, q, src) in norm)
        {
            if (kind == "Batch")
            {
                var key = $"B|{item.ItemCode}|{whs}|{no}";
                if (flow == BatchFlow.In)
                {
                    var row = await FindBatch(item.ItemCode, whs, no);
                    if (row?.ExpiryDate != null && src.ExpiryDate != null && Read(row.ExpiryDate.Value) != src.ExpiryDate.Value.Date)
                        return $"Batch '{no}' already exists with expiry {Read(row.ExpiryDate.Value):yyyy-MM-dd}; a receipt can't give it a different one.";
                }
                else if (flow == BatchFlow.ReturnIn)
                {
                    if (await AnyBatchRow(item.ItemCode, no) == null && Get(tally, key) == 0)
                        return $"Batch '{no}' was never received for {item.ItemCode}, so it can't be returned.";
                }
                else
                {
                    var row = await FindBatch(item.ItemCode, whs, no);
                    var available = (row?.Quantity ?? 0m) + Get(tally, key);
                    if (row == null || available < q)
                        return $"Batch '{no}' has {Math.Max(0m, available):0.####} of {item.ItemCode} in {whs}; {q:0.####} needed.";
                    if (flow == BatchFlow.Out && row.ExpiryDate != null && Read(row.ExpiryDate.Value) < date.Date)
                        return $"Batch '{no}' expired on {Read(row.ExpiryDate.Value):yyyy-MM-dd} and can't be issued.";
                }
                Add(tally, key, Leaves(flow) ? -q : q);
            }
            else
            {
                var key = $"S|{item.ItemCode}|{no}";
                var row = await FindSerial(item.ItemCode, no);
                var inStock = (row?.Quantity ?? 0m) + Get(tally, key);
                if (flow == BatchFlow.In)
                {
                    if (inStock > 0) return $"Serial number '{no}' is already in stock for {item.ItemCode}.";
                }
                else if (flow == BatchFlow.ReturnIn)
                {
                    if (row == null) return $"Serial number '{no}' was never received for {item.ItemCode}, so it can't be returned.";
                    if (inStock > 0) return $"Serial number '{no}' is already in stock for {item.ItemCode}.";
                }
                else
                {
                    if (row == null || inStock < 1 || !string.Equals(row.WhsCode, whs, StringComparison.OrdinalIgnoreCase))
                        return $"Serial number '{no}' isn't in stock in {whs} for {item.ItemCode}.";
                }
                Add(tally, key, Leaves(flow) ? -1m : 1m);
            }
        }
        return null;
    }

    // Applies already-validated numbers: updates KBAT and writes the KBTX movement rows.
    public async Task ApplyAsync(ItemsMaster item, string whs, IReadOnlyList<BatchLine>? lines, BatchFlow flow, DateTime date,
        string transType, string baseDocType, int baseDocEntry, string? baseDocNum, string? cardCode)
    {
        var kind = KindOf(item);
        if (kind == null || lines == null) return;
        var day = Day(date);

        foreach (var l in lines)
        {
            var no = l.BatchNo!.Trim();
            var q = kind == "Serial" ? 1m : l.Quantity;
            StockBatch? row;

            if (kind == "Batch")
            {
                row = await FindBatch(item.ItemCode, whs, no);
                if (Leaves(flow))
                {
                    row!.Quantity -= q; row.UpdatedAt = DateTime.UtcNow;
                }
                else if (row != null)
                {
                    row.Quantity += q; row.UpdatedAt = DateTime.UtcNow;
                    row.ExpiryDate ??= Day(l.ExpiryDate); row.ManufacturingDate ??= Day(l.ManufacturingDate);
                }
                else
                {
                    var sibling = flow == BatchFlow.ReturnIn ? await AnyBatchRow(item.ItemCode, no) : null;
                    _db.Set<StockBatch>().Add(new StockBatch
                    {
                        ItemCode = item.ItemCode, WhsCode = whs, BatchNo = no, Kind = "Batch", Quantity = q, ReceivedDate = day,
                        ExpiryDate = sibling?.ExpiryDate ?? Day(l.ExpiryDate), ManufacturingDate = sibling?.ManufacturingDate ?? Day(l.ManufacturingDate),
                    });
                }
            }
            else
            {
                row = await FindSerial(item.ItemCode, no);
                if (Leaves(flow)) { row!.Quantity = 0; row.UpdatedAt = DateTime.UtcNow; }
                else if (row != null) { row.Quantity = 1; row.WhsCode = whs; row.UpdatedAt = DateTime.UtcNow; }
                else
                {
                    _db.Set<StockBatch>().Add(new StockBatch
                    {
                        ItemCode = item.ItemCode, WhsCode = whs, BatchNo = no, Kind = "Serial", Quantity = 1, ReceivedDate = day,
                        ExpiryDate = Day(l.ExpiryDate), ManufacturingDate = Day(l.ManufacturingDate),
                    });
                }
            }

            _db.Set<StockBatchMove>().Add(new StockBatchMove
            {
                ItemCode = item.ItemCode, WhsCode = whs, BatchNo = no, Quantity = Leaves(flow) ? -q : q,
                PostingDate = date, TransType = transType, BaseDocType = baseDocType, BaseDocEntry = baseDocEntry,
                BaseDocNum = baseDocNum, CardCode = cardCode,
            });
        }
    }
}
