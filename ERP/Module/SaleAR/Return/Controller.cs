using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Modules.Inventory.ItemsMaster;  // ✅ your namespace
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using FarmingApi.Services;

using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.SaleAR.Return;

[ApiController]
[Route("[controller]")]         // ✅ route = /Return
public class ReturnController : ControllerBase  // ✅ ReturnController
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IInventoryPostingService _inventoryPosting;
    private readonly IDocumentNumberService _docNumber;

    public ReturnController(
        MyDbContext db,
        IMapper mapper,
        IInventoryPostingService inventoryPosting,
        IDocumentNumberService docNumber)
    {
        _db     = db;
        _mapper = mapper;
        _inventoryPosting = inventoryPosting;
        _docNumber = docNumber;
    }

    // ── GET /Return ─────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<ReturnListResponse>>(list));
    }

    // ── GET /Return/{id} ────────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();
        return Ok(_mapper.Map<ReturnListResponse>(Return));
    }

    // ── POST /Return ────────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReturnListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var Return = _mapper.Map<Return>(dto);

        // Auto-assign the document number (server-side, race-safe) — same as
        // every other Sale A/R document.
        try { Return.DocNum = _docNumber.Next("Return"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        foreach (var line in Return.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            // Blank warehouse falls back to the default; an unknown one is rejected.
            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        // The return and its stock movement must commit together, or neither.
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            _db.Set<Return>().Add(Return);
            await _db.SaveChangesAsync();       // Return.Id populated

            // Goods physically come back into stock, at current average cost.
            var stockDate = Return.PostingDate ?? DateTime.UtcNow;
            foreach (var line in Return.Items)
            {
                await _inventoryPosting.PostReturnInAsync(
                    line.ItemCode!, line.WhsCode!, line.Quantity,
                    "Return", "Return", Return.Id, Return.DocNum, stockDate,
                    customer.Code, customer.CardName);
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }

        var created = await _db.Set<Return>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == Return.Id);

        return CreatedAtAction(nameof(GetById), new { id = Return.Id },
            _mapper.Map<ReturnListResponse>(created));
    }

    // ── PUT /Return/{id} ────────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReturnUpdateRequest dto)
    {
        var Return = await _db.Set<Return>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (Return == null) return NotFound();

        var customer = await _db.Set<BPEntity>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(Return.Items);
        _mapper.Map(dto, Return);

        foreach (var line in Return.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;

            try { line.WhsCode = await _inventoryPosting.ResolveWarehouseCodeAsync(line.WhsCode); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /Return/{id} ─────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var Return = await _db.Set<Return>().FindAsync(id);
        if (Return == null) return NotFound();

        _db.Set<Return>().Remove(Return);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}