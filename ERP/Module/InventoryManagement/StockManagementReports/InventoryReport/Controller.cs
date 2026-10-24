using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JournalEntity = FarmingApi.Modules.InventoryManagement.InventoryJournal.InventoryJournal;
using ItemsMasterEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;

namespace FarmingApi.Modules.InventoryManagement.StockManagementReports.InventoryReport;

// ═══════════════════════════════════════════════════════════════
// INVENTORY REPORT — beginning/in/out/ending qty & value per item,
// derived from the KINM stock ledger (InventoryJournal). Read-only.
// GET /Reports/Inventory?fromDate=..&toDate=..&groupBy=Item|ItemGroup|Warehouse
// ═══════════════════════════════════════════════════════════════
public class InventoryReportRow
{
    public string  ItemCode  { get; set; } = "";
    public string  ItemName  { get; set; } = "";
    public string  ItemGroup { get; set; } = "";
    public string  WhsCode   { get; set; } = "";
    public string  Uom       { get; set; } = "";
    public decimal BeginQty  { get; set; }
    public decimal InQty     { get; set; }
    public decimal OutQty    { get; set; }
    public decimal EndQty    { get; set; }
    public decimal AvgCost   { get; set; }
    public decimal EndValue  { get; set; }
}

// Internal running-balance snapshot per (ItemCode, WhsCode) pair, before
// any item/warehouse/item-group roll-up is applied.
internal class Ledger
{
    public string  ItemCode  = "";
    public string  WhsCode   = "";
    public decimal BeginQty;
    public decimal BeginValue;
    public decimal InQty;
    public decimal OutQty;
    public decimal EndQty;
    public decimal EndValue;
}

[ApiController]
[Route("Reports/Inventory")]
public class InventoryReportController : ControllerBase
{
    private readonly MyDbContext _db;

    public InventoryReportController(MyDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate,
        [FromQuery] string? itemFrom,
        [FromQuery] string? itemTo,
        [FromQuery] string? itemGroup,
        [FromQuery] string? warehouse,
        [FromQuery] string groupBy = "Item",
        [FromQuery] bool includeZero = false)
    {
        if (fromDate == default || toDate == default)
            return BadRequest(new { message = "fromDate and toDate are required." });
        if (fromDate > toDate)
            return BadRequest(new { message = "fromDate must be on or before toDate." });

        var from = fromDate.Date;
        var to = toDate.Date;

        var journalQuery = _db.Set<JournalEntity>().AsNoTracking().Where(x => x.PostingDate < to.AddDays(1));
        if (!string.IsNullOrWhiteSpace(warehouse))
            journalQuery = journalQuery.Where(x => x.WhsCode == warehouse);

        var rows = await journalQuery
            .OrderBy(x => x.PostingDate).ThenBy(x => x.Id)
            .Select(x => new { x.ItemCode, x.WhsCode, x.PostingDate, x.InQty, x.OutQty, x.UnitCost, x.QtyBalance, x.ValueBalance })
            .ToListAsync();

        // Per (ItemCode, WhsCode) running-balance ledger.
        var ledgers = rows
            .GroupBy(x => new { x.ItemCode, x.WhsCode })
            .Select(g =>
            {
                var beginRow = g.Where(x => x.PostingDate < from).LastOrDefault();
                var endRow = g.Where(x => x.PostingDate <= to).LastOrDefault();
                var inRange = g.Where(x => x.PostingDate >= from && x.PostingDate <= to).ToList();

                return new Ledger
                {
                    ItemCode = g.Key.ItemCode,
                    WhsCode = g.Key.WhsCode,
                    BeginQty = beginRow?.QtyBalance ?? 0,
                    BeginValue = beginRow?.ValueBalance ?? 0,
                    InQty = inRange.Sum(x => x.InQty),
                    OutQty = inRange.Sum(x => x.OutQty),
                    EndQty = endRow?.QtyBalance ?? beginRow?.QtyBalance ?? 0,
                    EndValue = endRow?.ValueBalance ?? beginRow?.ValueBalance ?? 0,
                };
            })
            .Where(l => includeZero || l.BeginQty != 0 || l.InQty != 0 || l.OutQty != 0 || l.EndQty != 0)
            .ToList();

        if (!string.IsNullOrWhiteSpace(itemFrom))
            ledgers = ledgers.Where(l => string.Compare(l.ItemCode, itemFrom, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        if (!string.IsNullOrWhiteSpace(itemTo))
            ledgers = ledgers.Where(l => string.Compare(l.ItemCode, itemTo, StringComparison.OrdinalIgnoreCase) <= 0).ToList();

        var itemCodes = ledgers.Select(l => l.ItemCode).Distinct().ToList();
        var items = await _db.Set<ItemsMasterEntity>().AsNoTracking()
            .Where(i => itemCodes.Contains(i.ItemCode))
            .Select(i => new { i.ItemCode, i.ItemName, i.ItemGroup, i.UomCode, i.UomName })
            .ToListAsync();
        var itemMeta = items.ToDictionary(i => i.ItemCode, i => i);

        string GroupOf(string code) => itemMeta.TryGetValue(code, out var m) ? m.ItemGroup ?? "" : "";
        string NameOf(string code) => itemMeta.TryGetValue(code, out var m) ? m.ItemName ?? "" : "";
        string UomOf(string code) => itemMeta.TryGetValue(code, out var m) ? (m.UomCode ?? m.UomName ?? "") : "";

        if (!string.IsNullOrWhiteSpace(itemGroup))
            ledgers = ledgers.Where(l => GroupOf(l.ItemCode) == itemGroup).ToList();

        // Roll the (Item, Warehouse) ledgers up to the requested grouping level.
        IEnumerable<InventoryReportRow> result = (groupBy ?? "Item").Replace(" ", "") switch
        {
            "Warehouse" => ledgers
                .GroupBy(l => l.WhsCode)
                .Select(g => ToRow("", "", "", g.Key, "", g)),

            "ItemGroup" => ledgers
                .GroupBy(l => GroupOf(l.ItemCode))
                .Select(g => ToRow("", "", g.Key, "", "", g)),

            _ => ledgers
                .GroupBy(l => l.ItemCode)
                .Select(g => ToRow(g.Key, NameOf(g.Key), GroupOf(g.Key), "", UomOf(g.Key), g)),
        };

        return Ok(result.OrderBy(r => r.ItemCode).ThenBy(r => r.WhsCode).ToList());
    }

    private static InventoryReportRow ToRow(string itemCode, string itemName, string itemGroup, string whsCode, string uom, IEnumerable<Ledger> group)
    {
        var beginQty = group.Sum(x => x.BeginQty);
        var inQty = group.Sum(x => x.InQty);
        var outQty = group.Sum(x => x.OutQty);
        var endQty = group.Sum(x => x.EndQty);
        var endValue = group.Sum(x => x.EndValue);

        return new InventoryReportRow
        {
            ItemCode = itemCode,
            ItemName = itemName,
            ItemGroup = itemGroup,
            WhsCode = whsCode,
            Uom = uom,
            BeginQty = beginQty,
            InQty = inQty,
            OutQty = outQty,
            EndQty = endQty,
            AvgCost = endQty != 0 ? endValue / endQty : 0,
            EndValue = endValue,
        };
    }
}
