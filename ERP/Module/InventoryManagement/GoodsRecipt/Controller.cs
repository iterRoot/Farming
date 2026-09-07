using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Services;
using FarmingApi;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
// ✅ Alias to avoid conflict between namespace and class both named ItemsMaster
using ItemsMasterEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity          = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

public class GoodsReceiptController : MyController
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IMapper                 _mapper;
    private readonly IDocumentNumberService  _docNumber;
    private readonly MyDbContext             _db;
    private readonly IInventoryPostingService _inventoryPosting;

    public GoodsReceiptController(
        IGoodsReceiptRepository repository,
        IMapper                 mapper,
        IDocumentNumberService  docNumber,
        MyDbContext             db,
        IInventoryPostingService inventoryPosting)
    {
        _repository = repository;
        _mapper     = mapper;
        _docNumber  = docNumber;
        _db         = db;
        _inventoryPosting = inventoryPosting;
    }

    // ── GET /GoodsReceipt ──────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var query   = _repository.GetAll().Include(x => x.Lines);
        var results = _mapper.ProjectTo<GoodsReceiptListResponse>(query).ToList();
        return Ok(results);
    }

    // ── GET /GoodsReceipt/{id} ─────────────────────────────────
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        return Ok(_mapper.Map<GoodsReceiptListResponse>(entity));
    }

    // ── POST /GoodsReceipt ─────────────────────────────────────
    // ✅ Fix 1: [AllowAnonymous] — was missing (caused 405)
    // ✅ Fix 2: Auto DocNo from DocumentNumberRange
    // ✅ Fix 3: Vendor lookup → CustomerCode / CustomerName
    // ✅ Fix 4: Item lookup → ItemCode / ItemName per line
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsReceiptInsertListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // ✅ Auto-number
        string docNo;
        try { docNo = _docNumber.Next("GoodsReceipt"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // ✅ Vendor lookup → fill CustomerCode / CustomerName
        if (request.VendorId.HasValue && request.VendorId > 0)
        {
            var vendor = await _db.Set<BPEntity>()
                .FirstOrDefaultAsync(x => x.Id == request.VendorId.Value);
            if (vendor != null)
            {
                request.CustomerCode = vendor.Code;
                request.CustomerName = vendor.CardName;
            }
        }

        var entity = _mapper.Map<GoodsReceipt>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        // ✅ Auto-fill ItemCode / ItemName per line
        int ln = 1;
        foreach (var line in entity.Lines)
        {
            var reqLine = request.Lines.ElementAtOrDefault(ln - 1);
            if (reqLine?.ItemId > 0)
            {
                var item = await _db.Set<ItemsMasterEntity>()
                    .FirstOrDefaultAsync(x => x.Id == reqLine.ItemId);
                if (item != null)
                {
                    line.ItemCode = item.ItemCode ?? line.ItemCode;
                    line.ItemName = item.ItemName ?? line.ItemName;
                }
            }
            line.LineNum = ln++;
        }

        _repository.Add(entity);
        _repository.Commit();

        // Track each received line in the Inventory Journal (default warehouse).
        var whs = await _inventoryPosting.ResolveWarehouseCodeAsync(null);
        var postDate = entity.PostingDate == default ? DateTime.UtcNow : entity.PostingDate;
        foreach (var line in entity.Lines)
        {
            await _inventoryPosting.PostInAsync(
                line.ItemCode, whs, line.Qty, line.Price,
                "Goods Receipt", "GoodsReceipt", entity.Id, docNo, postDate,
                entity.CustomerCode, entity.CustomerName);
        }
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Receipt saved successfully", id = entity.Id, docNo });
    }

    // ── PUT /GoodsReceipt/{id} ─────────────────────────────────
    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsReceiptInsertListRequest request)
    {
        var entity = _repository.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Closed")
            return BadRequest("Cannot edit a Closed Goods Receipt");

        var existingDocNo = entity.DocNo;
        _db.RemoveRange(entity.Lines);

        // Vendor lookup → CustomerCode / CustomerName
        if (request.VendorId.HasValue && request.VendorId > 0)
        {
            var vendor = await _db.Set<BPEntity>()
                .FirstOrDefaultAsync(x => x.Id == request.VendorId.Value);
            if (vendor != null)
            {
                request.CustomerCode = vendor.Code;
                request.CustomerName = vendor.CardName;
            }
        }

        _mapper.Map(request, entity);
        entity.DocNo     = existingDocNo;   // ✅ preserve original
        entity.UpdatedAt = DateTime.UtcNow;

        int ln = 1;
        foreach (var line in entity.Lines)
        {
            var reqLine = request.Lines.ElementAtOrDefault(ln - 1);
            if (reqLine?.ItemId > 0)
            {
                var item = await _db.Set<ItemsMasterEntity>()
                    .FirstOrDefaultAsync(x => x.Id == reqLine.ItemId);
                if (item != null)
                {
                    line.ItemCode = item.ItemCode ?? line.ItemCode;
                    line.ItemName = item.ItemName ?? line.ItemName;
                }
            }
            line.LineNum = ln++;
        }

        _repository.Update(entity);
        _repository.Commit();
        return NoContent();
    }

    // ── DELETE /GoodsReceipt/{id} ──────────────────────────────
    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}