using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Core;

// ═════════════════════════════════════════════════════════════════════
// DOCUMENTS A STOCK REVALUATION CAN NAME AS ITS REASON
// A revaluation changes what an item costs, so the document it points at is one that
// sets or changes cost: a receipt, a purchase invoice or credit, a return, a stock
// receipt or issue, or a production receipt or issue. A reference is a (type, id)
// pair — ids repeat across tables, so an id alone can't identify a document — and
// its label is always built here from the stored document, never taken from the client.
// Tables and columns come from the document-link registry (DocLinkEngine), so nothing
// here hard-codes a table name.
// ═════════════════════════════════════════════════════════════════════
public static class ReferenceDocuments
{
    // Entity name (as the registry knows it) → what people call it.
    public static readonly (string Type, string Label)[] Types =
    {
        ("GoodsReceiptPO",        "Goods Receipt PO"),
        ("APInvoice",             "A/P Invoice"),
        ("APCreditNote",          "A/P Credit Memo"),
        ("GoodsReturn",           "Goods Return"),
        ("GoodsReceipt",          "Goods Receipt"),
        ("GoodsIssue",            "Goods Issue"),
        ("ReceiptFromProduction", "Receipt from Production"),
        ("IssueForProduction",    "Issue for Production"),
    };

    // A draft or cancelled document hasn't set any cost, so it can't be the reason for one.
    private const string LiveOnly = "NOT IN ('D','X')";

    public class Row { public int Id { get; set; } public string? Num { get; set; } }

    // Some documents are saved without a number; show "#id" for those rather than a blank.
    private static string NumberOf(DocHeader h) =>
        $"COALESCE(NULLIF(\"{h.DocNumCol}\"::text, ''), '#' || \"{h.IdCol}\"::text)";

    public static string? CanonicalType(string? type) =>
        Types.Select(t => t.Type).FirstOrDefault(t => t.Equals(type?.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string LabelOf(string type) => Types.First(t => t.Type == type).Label;

    // The reference's type and label for an existing, live document — or null when there isn't one.
    public static (string Type, string Label)? Describe(MyDbContext db, string? type, int id)
    {
        var canonical = CanonicalType(type);
        if (canonical == null) return null;
        var h = new DocLinkEngine(db).Header(canonical);
        if (h?.DocNumCol == null) return null;

        var row = db.Database.SqlQueryRaw<Row>(
            $"SELECT \"{h.IdCol}\" AS \"Id\", {NumberOf(h)} AS \"Num\" FROM \"{h.Table}\" " +
            $"WHERE \"{h.IdCol}\" = {{0}} AND \"{h.StatusCol}\"::text {LiveOnly}", id).AsEnumerable().FirstOrDefault();
        return row == null ? null : (canonical, $"{LabelOf(canonical)} {row.Num}");
    }

    // Newest first within each type, in the order of Types. An empty search lists the latest of each.
    public static List<object> Search(MyDbContext db, string? q, int perType)
    {
        var engine = new DocLinkEngine(db);
        var term = (q ?? "").Trim();
        var like = "%" + term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
        var hits = new List<object>();

        foreach (var (type, label) in Types)
        {
            var h = engine.Header(type);
            if (h?.DocNumCol == null) continue;
            var rows = db.Database.SqlQueryRaw<Row>(
                $"SELECT \"{h.IdCol}\" AS \"Id\", {NumberOf(h)} AS \"Num\" FROM \"{h.Table}\" " +
                $"WHERE \"{h.StatusCol}\"::text {LiveOnly} AND \"{h.DocNumCol}\"::text ILIKE {{0}} ESCAPE '\\' " +
                $"ORDER BY \"{h.IdCol}\" DESC LIMIT {perType}", like).AsEnumerable();
            foreach (var r in rows)
                hits.Add(new { type, typeLabel = label, id = r.Id, docNum = r.Num, label = $"{label} {r.Num}" });
        }
        return hits;
    }
}

[ApiController]
[Route("Documents")]
public class DocumentsController : ControllerBase
{
    private readonly MyDbContext _db;
    public DocumentsController(MyDbContext db) => _db = db;

    // GET /Documents/Search?q=GRPO — documents a stock revaluation can reference (see above).
    [HttpGet("Search")]
    public IActionResult Search([FromQuery] string? q) => Ok(ReferenceDocuments.Search(_db, q, perType: 10));
}
