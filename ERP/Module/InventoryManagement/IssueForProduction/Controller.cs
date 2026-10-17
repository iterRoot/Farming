using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.InventoryManagement.InventoryJournal;
using ItemEntity      = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBal    = FarmingApi.Modules.Sale.Inventory.Inventory;
using FarmingApi.Modules.Financials.GLAccountDetermination;

namespace FarmingApi.Modules.InventoryManagement.IssueForProduction;

// ═══════════════════════════════════════════════════════════════════
// ISSUE FOR PRODUCTION  (route: /IssueForProduction)
// POST posts immediately: components leave stock at valuation cost and a
// balanced journal entry records the move (Dr WIP / Cr Inventory — one
// inventory account in this app, so the entry nets to a balanced wash,
// the same shape Production Order "Close" uses).
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class IssueForProductionController : ControllerBase
{
    private readonly IIssueForProductionRepository _repository;
    private readonly IMapper                  _mapper;
    private readonly MyDbContext              _db;
    private readonly IInventoryPostingService _posting;

    public IssueForProductionController(IIssueForProductionRepository repository, IMapper mapper,
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
        return Ok(_mapper.Map<List<IssueForProductionResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<IssueForProductionResponse>(e));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] IssueForProductionRequest dto)
    {
        if (dto.Lines == null || dto.Lines.Count == 0) return BadRequest("Add at least one component line.");
        if (dto.Lines.Any(l => l.Quantity <= 0))        return BadRequest("Every quantity must be greater than zero.");

        var warehouse = await _posting.ResolveWarehouseCodeAsync(dto.Warehouse);

        var items = _db.Set<ItemEntity>().ToDictionary(i => i.ItemCode, i => i);
        foreach (var l in dto.Lines)
            if (string.IsNullOrWhiteSpace(l.ItemCode) || !items.ContainsKey(l.ItemCode))
                return BadRequest($"Item '{l.ItemCode}' does not exist.");

        // Check stock before touching anything.
        var shortages = new List<object>();
        foreach (var l in dto.Lines)
        {
            var bal = await _db.Set<InventoryBal>()
                .Where(b => b.ItemsCode == l.ItemCode && b.WhsCode == warehouse)
                .Select(b => (decimal?)b.Balance).FirstOrDefaultAsync() ?? 0m;
            if (bal < l.Quantity)
                shortages.Add(new { itemCode = l.ItemCode, needed = l.Quantity, onHand = bal, @short = l.Quantity - bal });
        }
        if (shortages.Count > 0)
            return BadRequest(new { message = "Not enough stock to issue.", shortages });

        var gl = await _db.Set<GLAccountDetermination>().FirstOrDefaultAsync();
        var inventoryAccount = gl?.InventoryAccount;
        if (string.IsNullOrWhiteSpace(inventoryAccount))
            return BadRequest("Set the Inventory account in GL Account Determination (KGLD) first.");

        var postingDate = dto.PostingDate == default ? DateTime.UtcNow : dto.PostingDate;

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var entity = new IssueForProduction
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
                var cost = await _posting.PostOutAsync(l.ItemCode, warehouse, l.Quantity,
                    "Issue for Production", "IssueForProduction", 0, entity.DocNum, postingDate);
                total += cost;
                entity.Lines.Add(new IssueForProductionLine
                {
                    LineNum  = lineNum++,
                    ItemCode = l.ItemCode,
                    ItemName = items[l.ItemCode].ItemName,
                    Quantity = l.Quantity,
                    Cost     = cost,
                });
            }
            entity.TotalCost = total;

            var je = BuildWashEntry(entity.DocNum, inventoryAccount!, total, postingDate,
                "Dr WIP / Cr Inventory — components issued", "KIGE");
            _db.Set<JournalEntry>().Add(je);
            await _db.SaveChangesAsync();

            entity.JournalEntryId = je.Id;
            _repository.Add(entity);
            _repository.Commit();

            await tx.CommitAsync();
            return Ok(new { issue = _mapper.Map<IssueForProductionResponse>(ReloadWithLines(entity.Id)),
                            journalEntryId = je.Id, journalNo = je.JrnlNo, totalCost = total });
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    private JournalEntry BuildWashEntry(string docNum, string account, decimal amount, DateTime date, string memo, string transType)
    {
        var je = new JournalEntry
        {
            JrnlNo = JournalEntryNumberService.Next(_db, date), TransDate = date, DueDate = date, DocDate = date,
            RefNo = docNum, Memo = $"{transType} {docNum} — {memo}", TransType = transType, BaseDocType = transType,
            Currency = "USD", ExchangeRate = 1, Status = "O", IsAutoGenerated = true, VersionNum = 1,
            CreatedAt = DateTime.UtcNow, InActive = false,
        };
        je.Lines.Add(new JournalEntryLine
        {
            LineNum = 1, AccountCode = account, AccountName = "Work in Process",
            Debit = amount, Credit = 0, DebitLC = amount, CreditLC = 0,
            LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
        });
        je.Lines.Add(new JournalEntryLine
        {
            LineNum = 2, AccountCode = account, AccountName = "Inventory",
            Debit = 0, Credit = amount, DebitLC = 0, CreditLC = amount,
            LineMemo = memo, CreatedAt = DateTime.UtcNow, InActive = false,
        });
        je.TotalDebit = amount; je.TotalCredit = amount; je.TotalLC = amount; je.TotalFC = amount;
        return je;
    }

    private string NextNumber()
    {
        var year = DateTime.UtcNow.Year; var prefix = $"IP-{year}-";
        var highest = _repository.GetAll().Where(o => o.DocNum.StartsWith(prefix)).Select(o => o.DocNum).ToList()
            .Select(n => int.TryParse(n[prefix.Length..], out var v) ? v : 0).DefaultIfEmpty(0).Max();
        return $"{prefix}{highest + 1:D5}";
    }

    private IssueForProduction ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}
