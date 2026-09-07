using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ═══════════════════════════════════════════════════════════════
// INVENTORY JOURNAL — read-only stock ledger.
// Rows are only ever written by IInventoryPostingService from the
// documents that move stock (Goods Receipt PO, Delivery, ...).
// GET /InventoryJournal                          → all rows, newest first
// GET /InventoryJournal?itemCode=..&whsCode=..    → filtered
// GET /InventoryJournal/ByDocument?baseDocType=..&baseDocEntry=..
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("InventoryJournal")]
public class InventoryJournalController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public InventoryJournalController(MyDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? itemCode,
        [FromQuery] string? whsCode,
        [FromQuery] string? baseDocType,
        [FromQuery] string? cardCode)
    {
        var query = _db.Set<InventoryJournal>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(itemCode))    query = query.Where(x => x.ItemCode == itemCode);
        if (!string.IsNullOrWhiteSpace(whsCode))     query = query.Where(x => x.WhsCode == whsCode);
        if (!string.IsNullOrWhiteSpace(baseDocType)) query = query.Where(x => x.BaseDocType == baseDocType);
        if (!string.IsNullOrWhiteSpace(cardCode))    query = query.Where(x => x.CardCode == cardCode);

        var list = await query
            .OrderByDescending(x => x.PostingDate)
            .ThenByDescending(x => x.Id)
            .ToListAsync();

        return Ok(_mapper.Map<List<InventoryJournalResponse>>(list));
    }

    [AllowAnonymous]
    [HttpGet("ByDocument")]
    public async Task<IActionResult> ByDocument([FromQuery] string baseDocType, [FromQuery] int baseDocEntry)
    {
        var list = await _db.Set<InventoryJournal>()
            .Where(x => x.BaseDocType == baseDocType && x.BaseDocEntry == baseDocEntry)
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(_mapper.Map<List<InventoryJournalResponse>>(list));
    }
}
