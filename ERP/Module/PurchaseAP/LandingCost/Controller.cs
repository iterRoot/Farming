using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.DocumentNumberRange;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using GoodsReceiptPOEntity = FarmingApi.Modules.PurchaseAP.GoodsReceiptPO.GoodsReceiptPO;

namespace FarmingApi.Modules.PurchaseAP.LandingCost;

[ApiController]
[Route("[controller]")]
public class LandingCostController : ControllerBase
{
    private readonly MyDbContext            _db;
    private readonly IMapper                _mapper;
    private readonly ILandedCostJournalService _journalService;
    private readonly IDocumentNumberRangeRepository _docNums;

    public LandingCostController(MyDbContext db, IMapper mapper, ILandedCostJournalService journalService, IDocumentNumberRangeRepository docNums)
    {
        _db             = db;
        _mapper         = mapper;
        _journalService = journalService;
        _docNums        = docNums;
    }

    // ── GET /LandingCost ─────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<LandedCost>()
            .Include(x => x.Vendor)
            .Include(x => x.BaseDocs).ThenInclude(b => b.GoodsReceiptPO)
            .Include(x => x.CostLines)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<LandedCostResponse>>(list));
    }

    // ── GET /LandingCost/{id} ────────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var lc = await Load(id);
        if (lc == null) return NotFound();
        return Ok(_mapper.Map<LandedCostResponse>(lc));
    }

    // ── POST /LandingCost ────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LandedCostRequest dto)
    {
        var validation = await ValidateAsync(dto);
        if (validation != null) return validation;

        var vendor = await _db.Set<BPEntity>().FirstAsync(x => x.Id == dto.VendorId);

        string docNum;
        try { docNum = _docNums.GenerateNextNumber("LandingCost").DocNo; }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        var lc = new LandedCost
        {
            PostingDate = dto.PostingDate,
            DocDate     = dto.DocDate,
            Status      = "D",
            VendorId    = dto.VendorId,
            Remarks     = dto.Remarks,
            Total       = dto.CostLines.Sum(c => c.Amount),
            DocNum      = docNum,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };

        lc.BaseDocs = dto.GoodsReceiptPOIds.Distinct()
            .Select(id => new LandedCostBaseDoc { GoodsReceiptPOId = id })
            .ToList();

        lc.CostLines = dto.CostLines
            .Select(c => new LandedCostLine { CostType = c.CostType, Amount = c.Amount, Remarks = c.Remarks })
            .ToList();

        _db.Set<LandedCost>().Add(lc);
        await _db.SaveChangesAsync();

        var created = await Load(lc.Id);
        return CreatedAtAction(nameof(GetById), new { id = lc.Id }, _mapper.Map<LandedCostResponse>(created));
    }

    // ── PUT /LandingCost/{id} ────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] LandedCostRequest dto)
    {
        var lc = await Load(id);
        if (lc == null) return NotFound();
        if (lc.Status != "D") return BadRequest("Only a Draft Landed Cost can be edited.");

        var validation = await ValidateAsync(dto);
        if (validation != null) return validation;

        lc.PostingDate = dto.PostingDate;
        lc.DocDate     = dto.DocDate;
        lc.VendorId    = dto.VendorId;
        lc.Remarks     = dto.Remarks;
        lc.Total       = dto.CostLines.Sum(c => c.Amount);
        lc.UpdatedAt   = DateTime.UtcNow;

        _db.RemoveRange(lc.BaseDocs);
        _db.RemoveRange(lc.CostLines);
        lc.BaseDocs = dto.GoodsReceiptPOIds.Distinct()
            .Select(gid => new LandedCostBaseDoc { GoodsReceiptPOId = gid })
            .ToList();
        lc.CostLines = dto.CostLines
            .Select(c => new LandedCostLine { CostType = c.CostType, Amount = c.Amount, Remarks = c.Remarks })
            .ToList();

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── DELETE /LandingCost/{id} ─────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var lc = await _db.Set<LandedCost>().FindAsync(id);
        if (lc == null) return NotFound();
        if (lc.Status != "D") return BadRequest("Only a Draft Landed Cost can be deleted — it has already adjusted item costs and posted a journal entry.");

        _db.Set<LandedCost>().Remove(lc);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /LandingCost/{id}/Post ──────────────────────────────
    // Allocates every cost line across all base receipts' lines by each
    // line's own value share (Quantity * Price), adds the resulting per-unit
    // amount onto ItemsMaster.ItemCost, and posts the Dr-Inventory/Cr-clearing
    // journal entry — same shape GoodsReceiptPO itself posts.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var lc = await Load(id);
        if (lc == null) return NotFound();
        if (lc.Status != "D") return BadRequest("Only a Draft Landed Cost can be posted.");
        if (!lc.BaseDocs.Any()) return BadRequest("Add at least one Goods Receipt PO to allocate costs against.");
        if (!lc.CostLines.Any()) return BadRequest("Add at least one cost line.");

        var totalCost = lc.CostLines.Sum(c => c.Amount);
        if (totalCost <= 0) return BadRequest("Total landed cost must be greater than zero.");

        var allLines = lc.BaseDocs.SelectMany(b => b.GoodsReceiptPO.Lines).ToList();
        if (!allLines.Any()) return BadRequest("The selected Goods Receipt PO(s) have no lines to allocate against.");

        var totalValue = allLines.Sum(l => l.Quantity * l.Price);
        if (totalValue <= 0) return BadRequest("The selected receipts have no value to allocate against.");

        var notAverage = allLines.Where(l => FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.Of(l.Item.ValuationMethod)
                                             != FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.MovingAverage)
                                 .Select(l => l.ItemCode ?? l.Item.ItemCode).Distinct().ToList();
        if (notAverage.Count > 0)
            return BadRequest($"Landed costs can only be applied to moving-average items; {string.Join(", ", notAverage)} {(notAverage.Count == 1 ? "uses" : "use")} standard or FIFO costing.");

        var allocations = new List<LandedCostAllocationResponse>();

        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            foreach (var line in allLines)
            {
                var lineValue  = line.Quantity * line.Price;
                var allocated  = Math.Round(totalCost * (lineValue / totalValue), 2);
                var unitAdd    = line.Quantity > 0 ? allocated / line.Quantity : 0;

                line.Item.ItemCost  += unitAdd;
                line.Item.UpdatedAt  = DateTime.UtcNow;

                allocations.Add(new LandedCostAllocationResponse
                {
                    ItemCode      = line.ItemCode ?? line.Item.ItemCode,
                    ItemName      = line.ItemName,
                    Quantity      = line.Quantity,
                    LineValue     = lineValue,
                    AllocatedCost = allocated,
                    UnitCostAdded = unitAdd,
                    NewItemCost   = line.Item.ItemCost,
                });
            }

            var vendor = await _db.Set<BPEntity>().FirstAsync(x => x.Id == lc.VendorId);
            var je = _journalService.CreateJournalEntry(lc, vendor);

            lc.Status     = "O";
            lc.Total      = totalCost;
            lc.UpdatedAt  = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(new { message = "Landed cost posted.", journalEntryId = je.Id, allocations });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private async Task<LandedCost?> Load(int id) =>
        await _db.Set<LandedCost>()
            .Include(x => x.Vendor)
            .Include(x => x.BaseDocs).ThenInclude(b => b.GoodsReceiptPO).ThenInclude(g => g.Lines).ThenInclude(l => l.Item)
            .Include(x => x.CostLines)
            .FirstOrDefaultAsync(x => x.Id == id);

    private async Task<IActionResult?> ValidateAsync(LandedCostRequest dto)
    {
        var vendorExists = await _db.Set<BPEntity>().AnyAsync(x => x.Id == dto.VendorId);
        if (!vendorExists) return BadRequest("Vendor not found.");

        if (dto.GoodsReceiptPOIds == null || !dto.GoodsReceiptPOIds.Any())
            return BadRequest("At least one Goods Receipt PO is required.");

        var distinctIds  = dto.GoodsReceiptPOIds.Distinct().ToList();
        var foundReceipts = await _db.Set<GoodsReceiptPOEntity>().CountAsync(x => distinctIds.Contains(x.Id));
        if (foundReceipts != distinctIds.Count)
            return BadRequest("One or more Goods Receipt POs were not found.");

        if (dto.CostLines == null || !dto.CostLines.Any())
            return BadRequest("At least one cost line is required.");

        foreach (var c in dto.CostLines)
        {
            if (!LandedCostTypes.All.Contains(c.CostType))
                return BadRequest($"Cost type must be one of: {string.Join(", ", LandedCostTypes.All)}");
            if (c.Amount <= 0)
                return BadRequest("Each cost line's amount must be greater than zero.");
        }

        return null;
    }
}
