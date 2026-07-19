using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.PurchaseAP.GoodsReceiptPO;

[ApiController]
[Route("[controller]")]   // ✅ route = /GoodsReceiptPO
public class GoodsReceiptPOController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IGoodsReceiptPOJournalService _journalService;

    public GoodsReceiptPOController(
        MyDbContext db,
        IMapper mapper,
        IGoodsReceiptPOJournalService journalService)
    {
        _db = db; _mapper = mapper; _journalService = journalService;
    }

    // ── GET /GoodsReceiptPO ─────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<GoodsReceiptPOResponse>>(list));
    }

    // ── GET /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Vendor)
            .Include(x => x.Lines).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();
        return Ok(_mapper.Map<GoodsReceiptPOResponse>(gr));
    }

    // ── POST /GoodsReceiptPO ────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReceiptPORequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        var gr = _mapper.Map<GoodsReceiptPO>(dto);

        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        // ══════════════════════════════════════════════════════════
        // BEGIN TRANSACTION — Goods Receipt PO + Journal Entry
        // If ANYTHING fails, BOTH are rolled back.
        // ══════════════════════════════════════════════════════════
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            // ── Step 1: Save the goods receipt ────────────────────
            _db.Set<GoodsReceiptPO>().Add(gr);
            await _db.SaveChangesAsync();
            // gr.Id is now populated

            // ── Step 2: Auto-create Journal Entry ─────────────────
            // Adds the JE + lines to the DbContext but does NOT save.
            var je = _journalService.CreateJournalEntry(gr, vendor);

            // Link back now that gr.Id exists
            je.BaseDocEntry = gr.Id;

            // ── Step 3: Save Journal Entry ────────────────────────
            await _db.SaveChangesAsync();

            // ── Step 4: Commit — both GRPO + JE are final ─────────
            await transaction.CommitAsync();

            var created = await _db.Set<GoodsReceiptPO>()
                .Include(x => x.Vendor)
                .Include(x => x.Lines).ThenInclude(l => l.Item)
                .FirstAsync(x => x.Id == gr.Id);

            return CreatedAtAction(nameof(GetById), new { id = gr.Id }, new
            {
                goodsReceiptPO = _mapper.Map<GoodsReceiptPOResponse>(created),
                journalEntryId = je.Id,
                journalNo      = je.JrnlNo,
            });
        }
        catch (InvalidOperationException ex)
        {
            // ── ROLLBACK — neither GRPO nor JE is saved ───────────
            await transaction.RollbackAsync();

            return BadRequest(new
            {
                error   = "Goods Receipt PO creation failed — transaction rolled back",
                details = ex.Message,
                hint    = ex.Message.Contains("KGLD")
                    ? "Go to Administration → GL Account Determination and configure InventoryAccount and InventoryOffset."
                    : null,
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                error   = "Unexpected error — transaction rolled back",
                details = ex.Message,
            });
        }
    }

    // ── PUT /GoodsReceiptPO/{id} ────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReceiptPOUpdateRequest dto)
    {
        var gr = await _db.Set<GoodsReceiptPO>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (gr == null) return NotFound();

        var vendor = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.VendorId);
        if (vendor == null) return BadRequest($"Vendor {dto.VendorId} not found");

        _db.RemoveRange(gr.Lines);
        _mapper.Map(dto, gr);

        foreach (var line in gr.Lines)
        {
            var item = await _db.Set<ItemsMaster>().FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null) return BadRequest($"Item {line.ItemId} not found");
            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /GoodsReceiptPO/{id} ─────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var gr = await _db.Set<GoodsReceiptPO>().FindAsync(id);
        if (gr == null) return NotFound();
        _db.Set<GoodsReceiptPO>().Remove(gr);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}