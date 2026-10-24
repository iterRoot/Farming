using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using ItemEntity      = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using WarehouseEntity = FarmingApi.Modules.Sale.Warehouse.Warehouse;
using InventoryBal    = FarmingApi.Modules.Sale.Inventory.Inventory;
using Bom             = FarmingApi.Modules.InventoryManagement.BillOfMaterial.BillOfMaterial;
using FarmingApi.Modules.Financials.GLAccountDetermination;

namespace FarmingApi.Modules.InventoryManagement.ProductionOrder;

// ═══════════════════════════════════════════════════════════════════
// PRODUCTION ORDER  (route: /ProductionOrder)
// GET    /ProductionOrder
// GET    /ProductionOrder/{id}
// POST   /ProductionOrder            → Planned. Snapshots the BOM, scaled.
// POST   /ProductionOrder/{id}/Release
// POST   /ProductionOrder/{id}/Close → consumes components, produces the
//                                      finished good, posts one JE.
// POST   /ProductionOrder/{id}/Cancel
// DELETE /ProductionOrder/{id}       → Planned only
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ProductionOrderController : ControllerBase
{
    private readonly IProductionOrderRepository _repository;
    private readonly IMapper                    _mapper;
    private readonly MyDbContext                _db;
    private readonly IInventoryPostingService   _posting;

    public ProductionOrderController(IProductionOrderRepository repository, IMapper mapper,
                                     MyDbContext db, IInventoryPostingService posting)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
        _posting    = posting;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<ProductionOrderResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ProductionOrderResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] ProductionOrderRequest dto)
    {
        if (dto.PlannedQty <= 0) return BadRequest("Planned quantity must be greater than zero.");

        var bom = _db.Set<Bom>().Include(b => b.Lines).FirstOrDefault(b => b.Id == dto.BillOfMaterialId);
        if (bom == null) return BadRequest($"Bill of Material {dto.BillOfMaterialId} not found.");
        if (!bom.IsActive) return BadRequest($"BOM '{bom.Code}' is inactive.");
        if (!string.IsNullOrWhiteSpace(dto.ItemCode) && dto.ItemCode != bom.ParentItemCode)
            return BadRequest($"Item '{dto.ItemCode}' does not match this BOM's parent item '{bom.ParentItemCode}'.");

        var warehouse = string.IsNullOrWhiteSpace(dto.Warehouse) ? bom.Warehouse : dto.Warehouse;
        if (!_db.Set<WarehouseEntity>().Any(w => w.Code == warehouse))
            return BadRequest($"Warehouse '{warehouse}' does not exist.");

        var entity = new ProductionOrder
        {
            DocNum           = NextNumber(),
            ItemCode         = bom.ParentItemCode,
            ItemName         = bom.ParentItemName,
            BillOfMaterialId = bom.Id,
            BomCode          = bom.Code,
            PlannedQty       = dto.PlannedQty,
            Warehouse        = warehouse,
            DueDate          = dto.DueDate == default ? DateTime.UtcNow.Date : dto.DueDate,
            Status           = "Planned",
            Remarks          = dto.Remarks,
            CreatedAt        = DateTime.UtcNow,
            InActive         = false,
        };

        // Scale every BOM line by (order qty / BOM yield qty).
        var factor = dto.PlannedQty / bom.Quantity;
        var lineNum = 1;
        foreach (var l in bom.Lines)
        {
            entity.Lines.Add(new ProductionOrderLine
            {
                LineNum    = lineNum++,
                ItemCode   = l.ItemCode,
                ItemName   = l.ItemName,
                PlannedQty = Math.Round(l.Quantity * factor, 4),
                Warehouse  = warehouse,
            });
        }

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<ProductionOrderResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpPost("{id:int}/Release")]
    public IActionResult Release(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Planned") return BadRequest($"Only a Planned order can be released (this one is {entity.Status}).");
        entity.Status    = "Released";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"{entity.DocNum} released." });
    }

    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status is "Closed" or "Cancelled")
            return BadRequest($"{entity.Status} orders cannot be cancelled.");
        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"{entity.DocNum} cancelled." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Planned")
            return BadRequest("Only a Planned order can be deleted — Release then Cancel instead.");
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // CLOSE — the only action that touches inventory or GL.
    //   For each component: consume PlannedQty at its current moving-
    //   average cost (PostOutAsync) from the component's own line
    //   warehouse.
    //   Produce the finished good: PlannedQty units in at unit cost =
    //   total component cost / PlannedQty (PostInAsync blends this
    //   into the finished good's own moving average, same mechanism
    //   GRPO already uses).
    //   JE: this app has one InventoryAccount for all stock (see
    //   GLAccountDetermination) — no separate Raw-Material/Finished-
    //   Goods split — so the entry is Dr InventoryAccount (FG value)
    //   / Cr InventoryAccount (component value) on the SAME account.
    //   Both sides are equal by construction, so this is a balanced
    //   "wash" entry that still records the transformation for audit.
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id, [FromBody(EmptyBodyBehavior = Microsoft.AspNetCore.Mvc.ModelBinding.EmptyBodyBehavior.Allow)] ProductionCloseRequest? body = null)
    {
        var entity = await _db.Set<ProductionOrder>().Include(x => x.Lines).FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Released")
            return BadRequest($"Only a Released order can be closed (this one is {entity.Status}).");

        // Check every component has enough stock before touching anything.
        var shortages = new List<ShortageLine>();
        foreach (var line in entity.Lines)
        {
            var balance = await _db.Set<InventoryBal>()
                .Where(b => b.ItemsCode == line.ItemCode && b.WhsCode == line.Warehouse)
                .Select(b => (decimal?)b.Balance).FirstOrDefaultAsync() ?? 0m;
            if (balance < line.PlannedQty)
                shortages.Add(new ShortageLine
                {
                    ItemCode = line.ItemCode, ItemName = line.ItemName,
                    Needed = line.PlannedQty, OnHand = balance, Short = line.PlannedQty - balance,
                });
        }
        if (shortages.Count > 0)
            return BadRequest(new { message = "Not enough stock to complete this order.", shortages });

        var gl = await _db.Set<GLAccountDetermination>().FirstOrDefaultAsync();
        var inventoryAccount = gl?.InventoryAccount;
        if (string.IsNullOrWhiteSpace(inventoryAccount))
            return BadRequest("Set the Inventory account in GL Account Determination (KGLD) first.");

        using var tx = _db.Database.BeginTransaction();
        try
        {
            decimal totalComponentCost = 0m;
            foreach (var line in entity.Lines)
            {
                var item = await _db.Set<ItemEntity>().FirstAsync(i => i.ItemCode == line.ItemCode);
                // Components leave stock at their valuation-method cost (average, standard or FIFO layers).
                var pick = body?.Components?.FirstOrDefault(c => string.Equals(c.ItemCode, line.ItemCode, StringComparison.OrdinalIgnoreCase))?.Batches;
                totalComponentCost += await _posting.PostOutAsync(line.ItemCode, line.Warehouse, line.PlannedQty,
                    "Production Order", "ProductionOrder", entity.Id, entity.DocNum, DateTime.UtcNow, batches: pick);
            }

            var unitCost = entity.PlannedQty > 0 ? totalComponentCost / entity.PlannedQty : 0m;
            await _posting.PostInAsync(entity.ItemCode, entity.Warehouse, entity.PlannedQty, unitCost,
                "Production Order", "ProductionOrder", entity.Id, entity.DocNum, DateTime.UtcNow, batches: body?.Finished);

            var je = new JournalEntry
            {
                JrnlNo          = JournalEntryNumberService.Next(_db, DateTime.UtcNow),
                TransDate       = DateTime.UtcNow,
                DueDate         = DateTime.UtcNow,
                DocDate         = DateTime.UtcNow,
                RefNo           = entity.DocNum,
                Memo            = $"Production Order {entity.DocNum} — {entity.PlannedQty:N4} x {entity.ItemName}",
                TransType       = "KWOR",
                BaseDocType     = "KWOR",
                BaseDocEntry    = entity.Id,
                Currency        = "USD",
                ExchangeRate    = 1,
                Status          = "O",
                IsAutoGenerated = true,
                VersionNum      = 1,
                CreatedAt       = DateTime.UtcNow,
                InActive        = false,
            };
            je.Lines.Add(new JournalEntryLine
            {
                LineNum = 1, AccountCode = inventoryAccount, AccountName = "Inventory (finished good produced)",
                Debit = totalComponentCost, Credit = 0, DebitLC = totalComponentCost, CreditLC = 0,
                LineMemo = $"Produced {entity.PlannedQty:N4} x {entity.ItemCode} — {entity.DocNum}",
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
            je.Lines.Add(new JournalEntryLine
            {
                LineNum = 2, AccountCode = inventoryAccount, AccountName = "Inventory (components consumed)",
                Debit = 0, Credit = totalComponentCost, DebitLC = 0, CreditLC = totalComponentCost,
                LineMemo = $"Consumed for {entity.DocNum}",
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
            je.TotalDebit = totalComponentCost; je.TotalCredit = totalComponentCost;
            je.TotalLC = totalComponentCost; je.TotalFC = totalComponentCost;

            _db.Set<JournalEntry>().Add(je);
            await _db.SaveChangesAsync();

            entity.Status         = "Closed";
            entity.ClosedDate     = DateTime.UtcNow;
            entity.JournalEntryId = je.Id;
            entity.UpdatedAt      = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            return Ok(new
            {
                message = $"{entity.DocNum} closed — {entity.PlannedQty:N4} x {entity.ItemCode} produced.",
                journalEntryId = je.Id, journalNo = je.JrnlNo, totalCost = totalComponentCost, unitCost,
            });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private string NextNumber()
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"WO-{year}-";
        var highest = _repository.GetAll().Where(o => o.DocNum.StartsWith(prefix))
            .Select(o => o.DocNum).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private ProductionOrder ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}
