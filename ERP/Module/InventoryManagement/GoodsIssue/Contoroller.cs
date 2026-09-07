using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Services;
using FarmingApi;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;

// ✅ Aliases — avoids namespace/class name conflict
using ItemsMasterEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity          = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Inventory.GoodsIssue;

public class GoodsIssueController : MyController
{
    private readonly IGoodsIssueRepository  _repository;
    private readonly IMapper                _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly MyDbContext            _db;
    private readonly IInventoryPostingService _inventoryPosting;

    public GoodsIssueController(
        IGoodsIssueRepository  repository,
        IMapper                mapper,
        IDocumentNumberService docNumber,
        MyDbContext            db,
        IInventoryPostingService inventoryPosting)
    {
        _repository = repository;
        _mapper     = mapper;
        _docNumber  = docNumber;
        _db         = db;
        _inventoryPosting = inventoryPosting;
    }

    // ── GET /GoodsIssue ───────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var query   = _repository.GetAll().Include(x => x.Lines);
        var results = _mapper.ProjectTo<GoodsIssueListResponse>(query).ToList();
        return Ok(results);
    }

    // ── GET /GoodsIssue/{id} ──────────────────────────────────
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        return Ok(_mapper.Map<GoodsIssueListResponse>(entity));
    }

    // ── POST /GoodsIssue ──────────────────────────────────────
    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsIssueInsertRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // ✅ Auto-number
        string docNo;
        try { docNo = _docNumber.Next("GoodsIssue"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // ✅ Vendor lookup → CustomerCode / CustomerName
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

        var entity     = _mapper.Map<GoodsIssue>(request);
        entity.DocNo   = docNo;
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

        // Track each issued line in the Inventory Journal (default warehouse).
        var whs = await _inventoryPosting.ResolveWarehouseCodeAsync(null);
        var postDate = entity.PostingDate == default ? DateTime.UtcNow : entity.PostingDate;
        foreach (var line in entity.Lines)
        {
            await _inventoryPosting.PostOutAsync(
                line.ItemCode, whs, line.Qty,
                "Goods Issue", "GoodsIssue", entity.Id, docNo, postDate,
                entity.CustomerCode, entity.CustomerName);
        }
        await _db.SaveChangesAsync();

        return Ok(new { message = "Goods Issue saved successfully", id = entity.Id, docNo = entity.DocNo });
    }

    // ── PUT /GoodsIssue/{id} ──────────────────────────────────
    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] GoodsIssueUpdateRequest request)
    {
        var entity = _repository.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Closed")
            return BadRequest("Cannot edit a Closed Goods Issue");

        var existingDocNo = entity.DocNo;
        _db.RemoveRange(entity.Lines);

        // Vendor lookup
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

    // ── POST /GoodsIssue/{id}/Close ───────────────────────────
    [AllowAnonymous]
    [HttpPost("{id:int}/Close")]
    public IActionResult Close(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Closed") return BadRequest("Already closed");

        entity.Status    = "Closed";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Goods Issue closed", docNo = entity.DocNo });
    }

    // ── DELETE /GoodsIssue/{id} ───────────────────────────────
    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Closed")
            return BadRequest("Cannot delete a Closed Goods Issue");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }
}