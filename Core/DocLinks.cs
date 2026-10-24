using System.Collections;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════
// COPY FROM / COPY TO — draw-down of source documents
//
// A document line can name the document it was copied from (BaseType + BaseEntry). This works
// out — from what the target documents already carry — how much of each source item has been
// drawn, and:
//   • refuses a line that draws more than the source has left, from a source that isn't open,
//     from a different business partner, or for an item the source doesn't have;
//   • closes the source once every item has been fully drawn;
//   • re-opens it when a document that drew from it is deleted or cancelled.
// Everything is discovered from the EF model, so any header with a Status and lines with
// ItemCode + Quantity (+ BaseEntry / BaseType on the copying side) takes part.
// ═══════════════════════════════════════════════════════════════════════
public class DocHeader
{
    public IEntityType Type { get; init; } = null!;
    public string Name => Type.ClrType.Name;
    public string Table { get; init; } = null!;
    public string StatusCol { get; init; } = null!;
    public string? BpCol { get; init; }
    public string? DocNumCol { get; init; }
    public string IdCol { get; init; } = null!;
}

public class DocLine
{
    public DocHeader Header { get; init; } = null!;
    public string Table { get; init; } = null!;
    public string FkCol { get; init; } = null!;
    public string ItemCol { get; init; } = null!;
    public string QtyCol { get; init; } = null!;
    public string? BaseEntryCol { get; init; }
    public string? BaseTypeCol { get; init; }
}

public class DocRow { public string ItemCode { get; set; } = ""; public decimal Qty { get; set; } }
public class DocText { public string? Value { get; set; } }

public class DocLinkEngine
{
    private static readonly Dictionary<string, string> Alias = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SaleQuotation"] = "SaleQuotion", ["SalesQuotation"] = "SaleQuotion", ["SaleQuotion"] = "SaleQuotion",
        ["GoodsReceiptPO"] = "GoodsReceiptPO", ["GoodsReciptPO"] = "GoodsReceiptPO", ["GRPO"] = "GoodsReceiptPO",
        ["ARInvoice"] = "ARInvoice", ["ArInvoice"] = "ARInvoice", ["APInvoice"] = "APInvoice", ["ApInvoice"] = "APInvoice",
        ["ARCreditNote"] = "ARCreditNote", ["ArCreditNote"] = "ARCreditNote", ["APCreditNote"] = "APCreditNote", ["ApCreditNote"] = "APCreditNote",
    };

    private readonly MyDbContext _db;
    private static List<DocHeader>? _headers; private static List<DocLine>? _lines; private static object? _model;

    public DocLinkEngine(MyDbContext db)
    {
        _db = db;
        Build();
    }

    private void Build()
    {
        if (_model == _db.Model && _headers != null) return;
        var lines = new List<DocLine>(); var headers = new Dictionary<IEntityType, DocHeader>();
        foreach (var et in _db.Model.GetEntityTypes())
        {
            var item = et.FindProperty("ItemCode"); var qty = et.FindProperty("Quantity") ?? et.FindProperty("Qty");
            if (item == null || qty == null || et.GetTableName() == null) continue;
            var fk = et.GetForeignKeys().FirstOrDefault(f => f.PrincipalEntityType.FindProperty("Status") != null && (f.PrincipalEntityType.FindProperty("DocNum") != null || f.PrincipalEntityType.FindProperty("DocNo") != null) && f.Properties.Count == 1 && f.Properties[0].Name != "BaseEntry" && !f.Properties[0].IsNullable);
            if (fk == null) continue;
            var pt = fk.PrincipalEntityType;
            if (!headers.TryGetValue(pt, out var h))
            {
                var bp = pt.FindProperty("CustomerId") ?? pt.FindProperty("VendorId");
                var doc = pt.FindProperty("DocNum") ?? pt.FindProperty("DocNo");
                h = new DocHeader
                {
                    Type = pt, Table = pt.GetTableName()!, StatusCol = pt.FindProperty("Status")!.GetColumnName(),
                    BpCol = bp?.GetColumnName(), DocNumCol = doc?.GetColumnName(), IdCol = pt.FindPrimaryKey()!.Properties[0].GetColumnName(),
                };
                headers[pt] = h;
            }
            lines.Add(new DocLine
            {
                Header = h, Table = et.GetTableName()!, FkCol = fk.Properties[0].GetColumnName(),
                ItemCol = item.GetColumnName(), QtyCol = qty.GetColumnName(),
                BaseEntryCol = et.FindProperty("BaseEntry")?.GetColumnName(), BaseTypeCol = et.FindProperty("BaseType")?.GetColumnName(),
            });
        }
        _headers = headers.Values.ToList(); _lines = lines; _model = _db.Model;
    }

    public IReadOnlyList<DocHeader> Headers => _headers!;
    public IReadOnlyList<DocLine> Lines => _lines!;

    public DocHeader? Header(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var n = name.Trim(); if (Alias.TryGetValue(n, out var a)) n = a;
        return Headers.FirstOrDefault(h => string.Equals(h.Name, n, StringComparison.OrdinalIgnoreCase));
    }

    // Names a BaseType may have been stored under for a header (canonical + aliases).
    private IEnumerable<string> NamesFor(DocHeader h) =>
        new[] { h.Name }.Concat(Alias.Where(kv => string.Equals(kv.Value, h.Name, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key)).Select(x => x.ToUpperInvariant()).Distinct();

    public string? StatusOf(DocHeader h, int id) =>
        _db.Database.SqlQueryRaw<DocText>($"SELECT \"{h.StatusCol}\"::text AS \"Value\" FROM \"{h.Table}\" WHERE \"{h.IdCol}\" = {{0}}", id).AsEnumerable().FirstOrDefault()?.Value;

    public int? BpOf(DocHeader h, int id) => h.BpCol == null ? null :
        _db.Database.SqlQueryRaw<DocText>($"SELECT \"{h.BpCol}\"::text AS \"Value\" FROM \"{h.Table}\" WHERE \"{h.IdCol}\" = {{0}}", id).AsEnumerable().Select(x => int.TryParse(x.Value, out var v) ? (int?)v : null).FirstOrDefault();

    public string? NumberOf(DocHeader h, int id) => h.DocNumCol == null ? null :
        _db.Database.SqlQueryRaw<DocText>($"SELECT \"{h.DocNumCol}\"::text AS \"Value\" FROM \"{h.Table}\" WHERE \"{h.IdCol}\" = {{0}}", id).AsEnumerable().FirstOrDefault()?.Value;

    /// <summary>What the source document holds per item (summing repeated lines).</summary>
    public Dictionary<string, decimal> Ordered(DocHeader src, int id)
    {
        var d = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var l in Lines.Where(x => x.Header == src))
            foreach (var r in _db.Database.SqlQueryRaw<DocRow>($"SELECT l.\"{l.ItemCol}\" AS \"ItemCode\", SUM(l.\"{l.QtyCol}\")::numeric AS \"Qty\" FROM \"{l.Table}\" l WHERE l.\"{l.FkCol}\" = {{0}} AND l.\"{l.ItemCol}\" IS NOT NULL GROUP BY l.\"{l.ItemCol}\"", id).AsEnumerable())
                d[r.ItemCode] = d.GetValueOrDefault(r.ItemCode) + r.Qty;
        return d;
    }

    /// <summary>What other live documents have already drawn from the source, per item.
    /// A document whose Status is D (rejected) or X (cancelled) draws nothing.</summary>
    public Dictionary<string, decimal> Drawn(DocHeader src, int id, DocHeader? excludeType = null, int? excludeId = null)
    {
        var d = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        var names = NamesFor(src).ToList();
        foreach (var t in Lines.Where(x => x.BaseEntryCol != null && x.BaseTypeCol != null))
        {
            var excl = excludeType == t.Header && excludeId != null ? $" AND h.\"{t.Header.IdCol}\" <> {excludeId}" : "";
            var nameList = string.Join(",", names.Select(n => $"'{n.Replace("'", "''")}'"));
            foreach (var r in _db.Database.SqlQueryRaw<DocRow>(
                $"SELECT l.\"{t.ItemCol}\" AS \"ItemCode\", SUM(l.\"{t.QtyCol}\")::numeric AS \"Qty\" FROM \"{t.Table}\" l JOIN \"{t.Header.Table}\" h ON h.\"{t.Header.IdCol}\" = l.\"{t.FkCol}\" " +
                $"WHERE l.\"{t.BaseEntryCol}\" = {{0}} AND UPPER(l.\"{t.BaseTypeCol}\") IN ({nameList}) AND h.\"{t.Header.StatusCol}\"::text NOT IN ('D','X'){excl} AND l.\"{t.ItemCol}\" IS NOT NULL GROUP BY l.\"{t.ItemCol}\"", id).AsEnumerable())
                d[r.ItemCode] = d.GetValueOrDefault(r.ItemCode) + r.Qty;
        }
        return d;
    }

    public Dictionary<string, (decimal Ordered, decimal Drawn, decimal Remaining)> Remaining(DocHeader src, int id, DocHeader? excludeType = null, int? excludeId = null)
    {
        var ord = Ordered(src, id); var dr = Drawn(src, id, excludeType, excludeId);
        return ord.ToDictionary(k => k.Key, k => (k.Value, dr.GetValueOrDefault(k.Key), k.Value - dr.GetValueOrDefault(k.Key)), StringComparer.OrdinalIgnoreCase);
    }

    public void SetStatus(DocHeader h, int id, string status, string onlyIf) =>
        _db.Database.ExecuteSqlRaw($"UPDATE \"{h.Table}\" SET \"{h.StatusCol}\" = {{0}} WHERE \"{h.IdCol}\" = {{1}} AND \"{h.StatusCol}\"::text = {{2}}", status, id, onlyIf);

    /// <summary>Closes a fully drawn source; with <paramref name="reopen"/> also re-opens a closed one that has quantity left.</summary>
    // Only "still to be delivered / received / invoiced" documents are closed and re-opened by draw-down;
    // invoices and credit notes keep their own open/paid status.
    private static readonly HashSet<string> AutoStatus = new(StringComparer.OrdinalIgnoreCase)
    {
        "SaleQuotion", "SaleOrder", "Delivery", "Return", "ReturnRequest", "SaleBlanketAgreement",
        "PurchaseQuotation", "PurchaseOrder", "GoodsReceiptPO", "GoodsReturnRequest", "PurchaseBlanketAgreement", "PurchaseRequest",
    };

    public void Reconcile(DocHeader src, int id, bool reopen)
    {
        if (!AutoStatus.Contains(src.Name)) return;
        var rem = Remaining(src, id);
        if (rem.Count == 0) return;
        if (rem.Values.All(r => r.Remaining <= 0.0001m)) SetStatus(src, id, "C", "O");
        else if (reopen) SetStatus(src, id, "O", "C");
    }

    /// <summary>Distinct sources a target document's lines were copied from.</summary>
    public List<(DocHeader Src, int Id)> SourcesOf(DocHeader target, int targetId)
    {
        var res = new List<(DocHeader, int)>();
        foreach (var l in Lines.Where(x => x.Header == target && x.BaseEntryCol != null && x.BaseTypeCol != null))
            foreach (var r in _db.Database.SqlQueryRaw<DocText>($"SELECT DISTINCT l.\"{l.BaseTypeCol}\" || '|' || l.\"{l.BaseEntryCol}\"::text AS \"Value\" FROM \"{l.Table}\" l WHERE l.\"{l.FkCol}\" = {{0}} AND l.\"{l.BaseEntryCol}\" IS NOT NULL", targetId).AsEnumerable())
            {
                var p = (r.Value ?? "").Split('|');
                if (p.Length == 2 && int.TryParse(p[1], out var e) && Header(p[0]) is { } h) res.Add((h, e));
            }
        return res;
    }
}

// Applies the rules above to document POST / PUT / DELETE / Cancel requests.
public class DocLinkFilter : IAsyncActionFilter
{
    private readonly MyDbContext _db;
    public DocLinkFilter(MyDbContext db) => _db = db;

    private static IEnumerable<object> LinesOf(object body)
    {
        foreach (var name in new[] { "Items", "Lines" })
        {
            var p = body.GetType().GetProperty(name);
            if (p?.GetValue(body) is IEnumerable e and not string)
            {
                foreach (var o in e) if (o != null) yield return o;
                yield break;
            }
        }
    }
    private static T? Get<T>(object o, string name) { var v = o.GetType().GetProperty(name)?.GetValue(o); return v is T t ? t : default; }

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var method = ctx.HttpContext.Request.Method;
        var path = ctx.HttpContext.Request.Path.Value ?? "";
        if (ctx.ActionDescriptor is not ControllerActionDescriptor cad || method == "GET") { await next(); return; }

        var engine = new DocLinkEngine(_db);
        var target = engine.Header(cad.ControllerName);
        if (target == null) { await next(); return; }
        ctx.RouteData.Values.TryGetValue("id", out var idObj);
        int? targetId = int.TryParse(idObj?.ToString(), out var tid) ? tid : null;

        var isCancel = method == "POST" && targetId != null && path.EndsWith("/Cancel", StringComparison.OrdinalIgnoreCase);
        var isDelete = method == "DELETE" && targetId != null;
        List<(DocHeader Src, int Id)> before = new();

        if (method is "POST" or "PUT" && !isCancel)
        {
            var body = ctx.ActionArguments.Values.FirstOrDefault(v => v != null && LinesOf(v).Any());
            if (body != null)
            {
                var err = Validate(engine, target, targetId, method == "PUT", body);
                if (err != null) { ctx.Result = new BadRequestObjectResult(err); return; }
            }
        }
        if (targetId != null && (isDelete || isCancel || method == "PUT")) before = engine.SourcesOf(target, targetId.Value);

        var executed = await next();
        var ok = executed.Result is ObjectResult { StatusCode: >= 200 and < 300 } || executed.Result is StatusCodeResult { StatusCode: >= 200 and < 300 }
                 || executed.Result is OkResult or NoContentResult or CreatedResult or CreatedAtActionResult;
        if (executed.Exception != null || !ok) return;

        // Sources touched by this request: those it now draws from, and (delete/cancel/edit) those it drew from before.
        var touched = new HashSet<(string, int)>(before.Select(b => (b.Src.Name, b.Id)));
        var reopen = isDelete || isCancel || method == "PUT";
        if (method is "POST" or "PUT" && !isCancel)
        {
            var body = ctx.ActionArguments.Values.FirstOrDefault(v => v != null && LinesOf(v).Any());
            if (body != null)
                foreach (var l in LinesOf(body))
                    if (Get<int?>(l, "BaseEntry") is int e && engine.Header(Get<string>(l, "BaseType")) is { } sh) touched.Add((sh.Name, e));
        }
        foreach (var (name, id) in touched)
            if (engine.Header(name) is { } h) engine.Reconcile(h, id, reopen);
    }

    private string? Validate(DocLinkEngine engine, DocHeader target, int? targetId, bool isUpdate, object body)
    {
        var lines = LinesOf(body).ToList();
        var bpId = Get<int>(body, "CustomerId"); if (bpId == 0) bpId = Get<int>(body, "VendorId");
        var wanted = new Dictionary<(string Src, int Id), Dictionary<string, decimal>>();
        var n = 0;
        foreach (var l in lines)
        {
            n++;
            var baseEntry = Get<int?>(l, "BaseEntry"); var baseType = Get<string>(l, "BaseType");
            if (baseEntry == null) continue;   // not a copied line (a type on its own is just a UI default)
            if (string.IsNullOrWhiteSpace(baseType)) return $"Line {n}: a copied line needs both its base document type and number.";
            var src = engine.Header(baseType);
            if (src == null) return $"Line {n}: '{baseType}' isn't a document type lines can be copied from.";
            // canonical name, so later drawn-quantity checks find it
            l.GetType().GetProperty("BaseType")?.SetValue(l, src.Name);

            var itemId = Get<int>(l, "ItemId"); var qty = Get<decimal>(l, "Quantity");
            var code = _db.Set<FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster>().Where(i => i.Id == itemId).Select(i => i.ItemCode).FirstOrDefault();
            if (code == null) continue;
            var key = (src.Name, baseEntry.Value);
            if (!wanted.TryGetValue(key, out var d)) wanted[key] = d = new(StringComparer.OrdinalIgnoreCase);
            d[code] = d.GetValueOrDefault(code) + qty;
        }

        foreach (var ((srcName, srcId), items) in wanted)
        {
            var src = engine.Header(srcName)!;
            var status = engine.StatusOf(src, srcId);
            var num = engine.NumberOf(src, srcId) ?? $"#{srcId}";
            if (status == null) return $"{src.Name} {srcId} doesn't exist.";
            if (status is "D" or "X") return $"{src.Name} {num} is {(status == "X" ? "cancelled" : "rejected")} and can't be copied from.";
            if (bpId != 0 && src.BpCol != null && engine.BpOf(src, srcId) is int sb && sb != bpId)
                return $"{src.Name} {num} belongs to a different business partner.";

            var rem = engine.Remaining(src, srcId, isUpdate ? target : null, isUpdate ? targetId : null);
            if (rem.Count == 0) continue;   // a source with no item lines (e.g. a down payment) has nothing to draw down
            foreach (var (item, req) in items)
            {
                if (!rem.TryGetValue(item, out var r)) return $"{item} isn't on {src.Name} {num}.";
                if (req > r.Remaining + 0.0001m)
                    return $"{req:0.####} of {item} requested from {src.Name} {num}, but only {Math.Max(0, r.Remaining):0.####} of the {r.Ordered:0.####} ordered remain to be copied.";
            }
        }
        return null;
    }
}

[ApiController]
[Route("DocLink")]
public class DocLinkController : ControllerBase
{
    private readonly MyDbContext _db;
    public DocLinkController(MyDbContext db) => _db = db;

    // GET /DocLink/Remaining?type=SaleOrder&id=5 — per item: ordered, already copied, still copyable.
    [HttpGet("Remaining")]
    public IActionResult Remaining([FromQuery] string type, [FromQuery] int id)
    {
        var e = new DocLinkEngine(_db);
        var h = e.Header(type);
        if (h == null) return BadRequest($"'{type}' isn't a document type lines can be copied from.");
        var status = e.StatusOf(h, id);
        if (status == null) return NotFound($"{h.Name} {id} not found.");
        var rem = e.Remaining(h, id);
        return Ok(new
        {
            type = h.Name, id, docNum = e.NumberOf(h, id), status,
            items = rem.Select(kv => new { itemCode = kv.Key, ordered = kv.Value.Ordered, drawn = kv.Value.Drawn, remaining = Math.Max(0, kv.Value.Remaining) }),
        });
    }

    // GET /DocLink/Registry — the document types and line tables the engine found (diagnostic).
    [HttpGet("Registry")]
    public IActionResult Registry()
    {
        var e = new DocLinkEngine(_db);
        return Ok(e.Lines.Select(l => new { header = l.Header.Name, headerTable = l.Header.Table, lineTable = l.Table, canCopy = l.BaseEntryCol != null && l.BaseTypeCol != null, hasBp = l.Header.BpCol != null }));
    }
}
