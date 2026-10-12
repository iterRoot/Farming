using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ═══════════════════════════════════════════════════════════════
// STOCK BATCHES / SERIAL NUMBERS — read-only.
// The numbers are created and used only by the stock documents
// (through IInventoryPostingService), never edited by hand.
// GET /StockBatch?itemCode=&whsCode=&availableOnly=   → numbers on file (for picking)
// GET /StockBatch/History?itemCode=&batchNo=          → every movement of one number
// GET /StockBatch/ByDocument?baseDocType=&baseDocEntry= → numbers a document moved
// GET /StockBatch/Expiring?days=30                    → batches with stock that expire soon
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("StockBatch")]
public class StockBatchController : ControllerBase
{
    private readonly MyDbContext _db;
    public StockBatchController(MyDbContext db) => _db = db;

    private static DateTime? D(DateTime? d) => d?.ToUniversalTime().Date;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? itemCode, [FromQuery] string? whsCode, [FromQuery] bool availableOnly = false)
    {
        var q = _db.Set<StockBatch>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(itemCode)) q = q.Where(b => b.ItemCode == itemCode);
        if (!string.IsNullOrWhiteSpace(whsCode)) q = q.Where(b => b.WhsCode == whsCode);
        if (availableOnly) q = q.Where(b => b.Quantity > 0);
        var rows = await q.OrderBy(b => b.ItemCode).ThenBy(b => b.ExpiryDate == null).ThenBy(b => b.ExpiryDate).ThenBy(b => b.BatchNo).ToListAsync();
        return Ok(rows.Select(b => new
        {
            b.Id, b.ItemCode, b.WhsCode, batchNo = b.BatchNo, b.Kind, b.Quantity,
            expiryDate = D(b.ExpiryDate), manufacturingDate = D(b.ManufacturingDate), receivedDate = D(b.ReceivedDate),
            expired = b.ExpiryDate != null && b.ExpiryDate.Value.ToUniversalTime().Date < DateTime.UtcNow.Date,
        }));
    }

    [HttpGet("History")]
    public async Task<IActionResult> History([FromQuery] string itemCode, [FromQuery] string batchNo)
    {
        if (string.IsNullOrWhiteSpace(itemCode) || string.IsNullOrWhiteSpace(batchNo)) return BadRequest("itemCode and batchNo are required.");
        var up = batchNo.Trim().ToUpperInvariant();
        var rows = await _db.Set<StockBatchMove>().Where(m => m.ItemCode == itemCode && m.BatchNo.ToUpper() == up)
            .OrderBy(m => m.PostingDate).ThenBy(m => m.Id).ToListAsync();
        return Ok(rows);
    }

    [HttpGet("ByDocument")]
    public async Task<IActionResult> ByDocument([FromQuery] string baseDocType, [FromQuery] int baseDocEntry)
    {
        var rows = await _db.Set<StockBatchMove>().Where(m => m.BaseDocType == baseDocType && m.BaseDocEntry == baseDocEntry)
            .OrderBy(m => m.Id).ToListAsync();
        return Ok(rows);
    }

    [HttpGet("Expiring")]
    public async Task<IActionResult> Expiring([FromQuery] int days = 30)
    {
        if (days < 0 || days > 3650) return BadRequest("days must be between 0 and 3650.");
        var limit = DateTime.UtcNow.Date.AddDays(days);
        var rows = await _db.Set<StockBatch>().Where(b => b.Quantity > 0 && b.ExpiryDate != null && b.ExpiryDate <= limit)
            .OrderBy(b => b.ExpiryDate).ToListAsync();
        return Ok(rows.Select(b => new
        {
            b.ItemCode, b.WhsCode, batchNo = b.BatchNo, b.Quantity, expiryDate = D(b.ExpiryDate),
            expired = b.ExpiryDate!.Value.ToUniversalTime().Date < DateTime.UtcNow.Date,
        }));
    }
}
