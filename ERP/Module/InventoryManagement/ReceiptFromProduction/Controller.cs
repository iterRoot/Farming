using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using FarmingApi.Modules.Financials.GLAccountDetermination;

namespace FarmingApi.Modules.InventoryManagement.ReceiptFromProduction;

// ═══════════════════════════════════════════════════════════════════
// RECEIPT FROM PRODUCTION  (route: /ReceiptFromProduction)
// POST posts immediately: finished goods enter stock at the given unit
// cost and a balanced journal entry records the move (Dr Inventory / Cr
// WIP — one inventory account here, so a balanced wash).
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class ReceiptFromProductionController : ControllerBase
{
    private readonly IReceiptFromProductionRepository _repository;
    private readonly IMapper                  _mapper;
    private readonly MyDbContext              _db;
    private readonly IInventoryPostingService _posting;

    public ReceiptFromProductionController(IReceiptFromProductionRepository repository, IMapper mapper,
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
        var list = _repository.GetAll().Include(x => x.Lines).OrderByDescending(x => x.Id).ToList();
        return Ok(_mapper.Map<List<ReceiptFromProductionResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ReceiptFromProductionResponse>(e));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ReceiptFromProductionRequest dto)
    {
        if (dto.Lines == null || dto.Lines.Count == 0) return BadRequest("Add at least one finished-good line.");
        if (dto.Lines.Any(l => l.Quantity <= 0))        return BadRequest("Every quantity must be greater than zero.");
        if (dto.Lines.Any(l => l.UnitCost < 0))         return BadRequest("Unit cost can't be negative.");

        var warehouse = await _posting.ResolveWarehouseCodeAsync(dto.Warehouse);

        var items = _db.Set<ItemEntity>().ToDictionary(i => i.ItemCode, i => i);
        foreach (var l in dto.Lines)
            if (string.IsNullOrWhiteSpace(l.ItemCode) || !items.ContainsKey(l.ItemCode))
                return BadRequest($"Item '{l.ItemCode}' does not exist.");

        var gl = await _db.Set<GLAccountDetermination>().FirstOrDefaultAsync();
        var inventoryAccount = gl?.InventoryAccount;
        if (string.IsNullOrWhiteSpace(inventoryAccount))
            return BadRequest("Set the Inventory account in GL Account Determination (KGLD) first.");

        var postingDate = dto.PostingDate == default ? DateTime.UtcNow : dto.PostingDate;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var entity = new ReceiptFromProduction
            {
                DocNum            = NextNumber(),
                PostingDate       = postingDate,
                Warehouse         = warehouse,
                ProductionOrderNo = dto.ProductionOrderNo,
                Status            = "C",
                Remarks           = dto.Remarks,
                CreatedAt         = DateTime.UtcNow,
                InActive          = false,
            };

            decimal total = 0m;
            var lineNum = 1;
            foreach (var l in dto.Lines)
            {
                await _posting.PostInAsync(l.ItemCode, warehouse, l.Quantity, l.UnitCost,
                    "Receipt from Production", "ReceiptFromProduction", 0, entity.DocNum, postingDate);
                var lineTotal = Math.Round(l.Quantity * l.UnitCost, 2);
                total += lineTotal;
                entity.Lines.Add(new ReceiptFromProductionLine
                {
                    LineNum  = lineNum++,
                    ItemCode = l.ItemCode,
                    ItemName = items[l.ItemCode].ItemName,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    Total    = lineTotal,
                });
            }
            entity.TotalCost = total;

            var je = BuildWashEntry(entity.DocNum, inventoryAccount!, total, postingDate,
                "Dr Inventory / Cr WIP — finished goods received");
            _db.Set<JournalEntry>().Add(je);
            await _db.SaveChangesAsync();

            entity.JournalEntryId = je.Id;
            _repository.Add(entity);
            _repository.Commit();

            await tx.CommitAsync();
            return Ok(new { receipt = _mapper.Map<ReceiptFromProductionResponse>(ReloadWithLines(entity.Id)),
                            journalEntryId = je.Id, journalNo = je.JrnlNo, totalCost = total });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private JournalEntry BuildWashEntry(string docNum, string account, decimal amount, DateTime date, string memo)
    {
        var je = new JournalEntry
        {
            JrnlNo = JournalEntryNumberService.Next(_db, date), TransDate = date, DueDate = date, DocDate = date,
            RefNo = docNum, Memo = $"KIGN {docNum} — {memo}", TransType = "KIGN", BaseDocType = "KIGN",
            Currency = "USD", ExchangeRate = 1, Status = "O", IsAutoGenerated = true, VersionNum = 1,
            CreatedAt = DateTime.UtcNow, InActive = false,
        };
        je.Lines.Add(new JournalEntryLine
        {
            LineNum = 1, AccountCode = account, AccountName = "Inventory (finished goods)",
            Debit = amount, Credit = 0, DebitLC = amount, CreditLC = 0,
            LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
        });
        je.Lines.Add(new JournalEntryLine
        {
            LineNum = 2, AccountCode = account, AccountName = "Work in Process",
            Debit = 0, Credit = amount, DebitLC = 0, CreditLC = amount,
            LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
        });
        je.TotalDebit = amount; je.TotalCredit = amount; je.TotalLC = amount; je.TotalFC = amount;
        return je;
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year; var prefix = $"RP-{year}-";
        var highest = _repository.GetAll().Where(o => o.DocNum.StartsWith(prefix)).Select(o => o.DocNum).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0).DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private ReceiptFromProduction ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}
