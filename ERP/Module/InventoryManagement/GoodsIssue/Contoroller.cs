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
    
    [HttpGet]
    public IActionResult Gets()
    {
        var query   = _repository.GetAll().Include(x => x.Lines);
        var results = _mapper.ProjectTo<GoodsIssueListResponse>(query).ToList();
        return Ok(results);
    }

    // ── GET /GoodsIssue/{id} ──────────────────────────────────
    
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
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] GoodsIssueInsertRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

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

        // Check batch / serial numbers for every line before anything is saved.
        var whs = await _inventoryPosting.ResolveWarehouseCodeAsync(null);
        var postDate = entity.PostingDate == default ? DateTime.UtcNow : entity.PostingDate;
        var tally = new FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchTally();
        foreach (var line in entity.Lines)
        {
            var batchError = await _inventoryPosting.CheckBatchesAsync(tally, line.ItemCode, whs, line.Qty, line.Batches,
                FarmingApi.Modules.InventoryManagement.InventoryJournal.BatchFlow.WriteOff, postDate);
            if (batchError != null) return BadRequest(batchError);
        }

        // Number the document only once it is known to be acceptable, so refusals don't burn numbers.
        string docNo;
        try { docNo = _docNumber.Next("GoodsIssue"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        entity.DocNo = docNo;

        // Document and stock ledger are saved together: if posting fails, neither is kept.
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            _repository.Add(entity);
            _repository.Commit();

            // Track each issued line in the Inventory Journal (default warehouse).
            foreach (var line in entity.Lines)
            {
                await _inventoryPosting.PostOutAsync(
                    line.ItemCode, whs, line.Qty,
                    "Goods Issue", "GoodsIssue", entity.Id, docNo, postDate,
                    entity.CustomerCode, entity.CustomerName, batches: line.Batches, allowExpired: true);
            }
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (InvalidOperationException ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.Message);
        }

        return Ok(new { message = "Goods Issue saved successfully", id = entity.Id, docNo = entity.DocNo });
    }

    // ── PUT /GoodsIssue/{id} ──────────────────────────────────
    
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