using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;
using ItemEntity       = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBalance = FarmingApi.Modules.Sale.Inventory.Inventory;
using StockLedger      = FarmingApi.Modules.InventoryManagement.InventoryJournal.InventoryJournal;
using GLDetermination  = FarmingApi.Modules.Financials.GLAccountDetermination.GLAccountDetermination;
using CoaEntity        = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;

namespace FarmingApi.Modules.InventoryManagement.StockRevaluation;

// ═══════════════════════════════════════════════════════════════════
// STOCK REVALUATION  (route: /StockRevaluation, also /stockrevaluation)
// GET    /stockrevaluation                  → list
// GET    /stockrevaluation/item/{itemCode}  → live cost + on-hand for a line
// GET    /stockrevaluation/serials          → serial/batch numbers to revalue
// GET    /stockrevaluation/{id}             → full document
// POST   /stockrevaluation                  → create: sets the new item cost,
//                                              writes stock-ledger rows, posts
//                                              the variance journal entry
// PUT    /stockrevaluation/{id}             → revise remarks / journal remark
// POST   /stockrevaluation/{id}/cancel      → only for record-only documents
// DELETE /stockrevaluation/{id}             → only for record-only documents
//
// Cost is one figure per item (ItemsMaster.ItemCost) shared by every warehouse, so
// a price change revalues the item's stock in all warehouses. For a standard-cost
// item this is how the standard is changed; for a FIFO item every open layer is
// re-priced to the new cost.
// Current cost and on-hand quantity are always read here, never taken from
// the request.
// ═══════════════════════════════════════════════════════════════════
public class StockRevaluationController : MyController
{
    private const string PriceChange = "Price Change";

    private readonly IMapper                      _mapper;
    private readonly IStockRevaluationRepository  _repository;
    private readonly MyDbContext                  _db;
    private readonly FarmingApi.Modules.InventoryManagement.InventoryJournal.IInventoryPostingService _posting;

    public StockRevaluationController(IStockRevaluationRepository repository, IMapper mapper, MyDbContext db,
        FarmingApi.Modules.InventoryManagement.InventoryJournal.IInventoryPostingService posting)
    {
        _mapper     = mapper;
        _repository = repository;
        _db         = db;
        _posting    = posting;
    }

    private string NextNumber()
    {
        var year   = DateTime.UtcNow.Year;
        var prefix = $"SREV-{year}-";
        var used   = _repository.GetAll()
            .Where(s => s.Number.StartsWith(prefix))
            .Select(s => s.Number)
            .ToList();
        var highest = 0;
        foreach (var n in used)
            if (int.TryParse(n[prefix.Length..], out var v) && v > highest) highest = v;
        return $"{prefix}{highest + 1:D5}";
    }

    // ── GET /stockrevaluation ─────────────────────────────────
    [HttpGet]
    public IActionResult Gets()
    {
        var query = _repository.GetAll().Include(s => s.Lines).OrderByDescending(s => s.Id);
        var rows  = _mapper.ProjectTo<StockRevaluationListResponse>(query).ToList();
        return Ok(rows);
    }

    // ── GET /stockrevaluation/item/{itemCode} ─────────────────
    [HttpGet("item/{itemCode}")]
    public IActionResult ItemInfo(string itemCode)
    {
        var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == itemCode);
        if (item == null) return NotFound($"Item {itemCode} not found");

        return Ok(new StockRevaluationItemInfo
        {
            ItemCode    = item.ItemCode,
            ItemName    = item.ItemName,
            Uom         = string.IsNullOrWhiteSpace(item.UomCode) ? item.UomName : item.UomCode,
            CurrentCost = item.ItemCost,
            InStock     = OnHand(item.ItemCode).Sum(b => b.Balance),
        });
    }

    // ── GET /stockrevaluation/serials ─────────────────────────
    // Serial/batch numbers are not tracked anywhere yet, so there is nothing
    // to revalue — returns an empty set so the Find panel reports no matches.
    [HttpGet("serials")]
    public IActionResult Serials(
        [FromQuery] string? filteredBy, [FromQuery] string? filter,
        [FromQuery] DateTime? admissionFrom, [FromQuery] DateTime? admissionTo,
        [FromQuery] DateTime? expiryFrom, [FromQuery] DateTime? expiryTo,
        [FromQuery] bool? displayIssued)
        => Ok(new List<SerialBatchCandidate>());

    // ── GET /stockrevaluation/{id} ────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var entity = _repository.GetAll()
            .Include(s => s.Lines)
            .Include(s => s.SerialBatchLines)
            .FirstOrDefault(s => s.Id == id);
        if (entity == null) return NotFound($"Stock revaluation {id} not found");
        return Ok(_mapper.Map<StockRevaluationDetailResponse>(entity));
    }

    // ── POST /stockrevaluation ────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] StockRevaluationCreateRequest request)
    {
        if (request.PostingDate == null) return BadRequest("Posting date is required");
        var revaluationType = string.IsNullOrWhiteSpace(request.RevaluationType) ? PriceChange : request.RevaluationType;
        if (revaluationType != PriceChange)
            return BadRequest($"Only \"{PriceChange}\" is supported.");
        if (request.SerialBatchLines.Count > 0)
            return BadRequest("Serial/batch revaluation isn't supported: serial and batch numbers aren't tracked.");
        if (request.Lines.Count == 0) return BadRequest("Add at least one line");

        var postingDate = request.PostingDate.Value;

        // ── Resolve every line against live data before touching anything ──
        var planned = new List<(ItemEntity Item, decimal OldCost, decimal NewCost, List<InventoryBalance> Stock, decimal Qty, decimal Variance)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var lr in request.Lines)
        {
            if (string.IsNullOrWhiteSpace(lr.ItemNo)) return BadRequest("Every line needs an item.");
            var item = _db.Set<ItemEntity>().FirstOrDefault(i => i.ItemCode == lr.ItemNo);
            if (item == null) return BadRequest($"Item {lr.ItemNo} not found.");
            if (!seen.Add(item.ItemCode)) return BadRequest($"Item {item.ItemCode} appears on more than one line.");
            if (lr.NewCost < 0) return BadRequest($"New cost for {item.ItemCode} can't be negative.");

            var stock = OnHand(item.ItemCode);
            var qty   = stock.Sum(b => b.Balance);
            if (qty < 0)
                return BadRequest($"{item.ItemCode} has negative stock ({qty:0.####}). Correct the stock before revaluing it.");
            if (lr.NewCost == item.ItemCost)
                return BadRequest($"New cost for {item.ItemCode} equals its current cost ({item.ItemCost:0.####}).");

            // FIFO stock is worth what its open layers cost, not qty × the next-issue cost.
            var oldValue = FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.Of(item.ValuationMethod) == FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.Fifo
                ? _posting.OpenLayers(item.ItemCode).Sum(l => l.RemainingQty * l.UnitCost)
                : item.ItemCost * qty;
            var variance = Math.Round(lr.NewCost * qty - oldValue, 2);
            planned.Add((item, item.ItemCost, lr.NewCost, stock, qty, variance));
        }

        // ── Accounts: only needed for the side(s) that actually move ──
        var increase = planned.Where(p => p.Variance > 0).Sum(p => p.Variance);
        var decrease = -planned.Where(p => p.Variance < 0).Sum(p => p.Variance);
        var gl = _db.Set<GLDetermination>().FirstOrDefault();
        if (increase > 0 || decrease > 0)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(gl?.InventoryAccount)) missing.Add("Inventory Account");
            if (increase > 0 && string.IsNullOrWhiteSpace(gl?.InventoryRevaluationIncrease)) missing.Add("Inventory Revaluation Increase");
            if (decrease > 0 && string.IsNullOrWhiteSpace(gl?.InventoryRevaluationDecrease)) missing.Add("Inventory Revaluation Decrease");
            if (missing.Count > 0)
                return BadRequest($"Set these in G/L Account Determination first: {string.Join(", ", missing)}.");

            var codes = new List<string> { gl!.InventoryAccount! };
            if (increase > 0) codes.Add(gl.InventoryRevaluationIncrease!);
            if (decrease > 0) codes.Add(gl.InventoryRevaluationDecrease!);
            var accountError = CheckPostable(codes);
            if (accountError != null) return BadRequest(accountError);
        }

        using var tx = _db.Database.BeginTransaction();
        try
        {
            var entity = new StockRevaluation
            {
                Number               = NextNumber(),
                Series               = request.Series,
                PostingDate          = postingDate,
                DocumentDate         = request.DocumentDate,
                RevaluationType      = revaluationType,
                Ref2                 = request.Ref2,
                InflationBased       = request.InflationBased,
                ReferencedDocumentId = request.ReferencedDocumentId,
                Status               = "Posted",
                Remarks              = request.Remarks,
                JournalRemark        = request.JournalRemark,
                CostsApplied         = true,
                CreatedAt            = DateTime.UtcNow,
                InActive             = false,
            };

            var n = 1;
            foreach (var p in planned)
            {
                entity.Lines.Add(new StockRevaluationLine
                {
                    LineNum         = n++,
                    ItemNo          = p.Item.ItemCode,
                    ItemDescription = p.Item.ItemName,
                    WhsCode         = null,   // cost is item-wide: every warehouse is revalued
                    CurrentCost     = p.OldCost,
                    NewCost         = p.NewCost,
                    Uom             = string.IsNullOrWhiteSpace(p.Item.UomCode) ? p.Item.UomName : p.Item.UomCode,
                    InStock         = p.Qty,
                    CreatedAt       = DateTime.UtcNow,
                    InActive        = false,
                });
            }
            _repository.Add(entity);
            _db.SaveChanges();

            foreach (var p in planned)
            {
                p.Item.ItemCost  = p.NewCost;
                p.Item.UpdatedAt = DateTime.UtcNow;
                if (FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.Of(p.Item.ValuationMethod) == FarmingApi.Modules.InventoryManagement.InventoryJournal.Valuation.Fifo)
                    _posting.RevalueLayers(p.Item.ItemCode, p.NewCost);

                // One zero-quantity ledger row per warehouse holding stock, so the
                // stock ledger's value balance moves with the new cost.
                foreach (var b in p.Stock.Where(b => b.Balance != 0))
                {
                    _db.Set<StockLedger>().Add(new StockLedger
                    {
                        ItemCode     = p.Item.ItemCode,
                        WhsCode      = b.WhsCode,
                        PostingDate  = postingDate,
                        TransType    = "Stock Revaluation",
                        BaseDocType  = "StockRevaluation",
                        BaseDocEntry = entity.Id,
                        BaseDocNum   = entity.Number,
                        InQty        = 0,
                        OutQty       = 0,
                        UnitCost     = p.NewCost,
                        TransValue   = b.Balance * (p.NewCost - p.OldCost),
                        QtyBalance   = b.Balance,
                        ValueBalance = b.Balance * p.NewCost,
                        CreatedAt    = DateTime.UtcNow,
                        InActive     = false,
                    });
                }
            }

            JournalEntry? je = null;
            if (increase > 0 || decrease > 0)
            {
                je = BuildJournalEntry(entity, gl!, planned.Select(p => (p.Item.ItemCode, p.Variance)).ToList());
                _db.Set<JournalEntry>().Add(je);
                _db.SaveChanges();
                entity.JournalEntryId = je.Id;
            }

            _db.SaveChanges();
            tx.Commit();

            return Ok(new
            {
                message        = "Stock revaluation posted",
                id             = entity.Id,
                number         = entity.Number,
                totalVariance  = increase - decrease,
                journalEntryId = je?.Id,
                journalNo      = je?.JrnlNo,
            });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ── PUT /stockrevaluation/{id} ────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StockRevaluationUpdateRequest request)
    {
        var entity = _repository.GetSingle(s => s.Id == id);
        if (entity == null) return NotFound($"Stock revaluation {id} not found");
        if (entity.Status == "Cancelled") return BadRequest("A cancelled document cannot be edited");

        entity.Remarks       = request.Remarks;
        entity.JournalRemark = request.JournalRemark;
        entity.UpdatedAt     = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return NoContent();
    }

    // ── POST /stockrevaluation/{id}/cancel ────────────────────
    // A document that changed item costs can't be undone by cancelling it:
    // later receipts and issues already used the new cost. Post a new
    // revaluation back to the cost you want instead.
    [HttpPost("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var entity = _repository.GetSingle(s => s.Id == id);
        if (entity == null) return NotFound($"Stock revaluation {id} not found");
        if (entity.CostsApplied)
            return BadRequest("This revaluation changed item costs and can't be cancelled. Post a new revaluation to change the cost back.");
        if (entity.Status == "Cancelled") return BadRequest("Document is already cancelled");

        entity.Status    = "Cancelled";
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = "Stock revaluation cancelled", id });
    }

    // ── DELETE /stockrevaluation/{id} ─────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(s => s.Id == id);
        if (entity == null) return NotFound($"Stock revaluation {id} not found");
        if (entity.CostsApplied)
            return BadRequest("This revaluation changed item costs and posted to the ledger, so it can't be deleted.");

        entity.DeletedAt = DateTime.UtcNow;
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private List<InventoryBalance> OnHand(string itemCode) =>
        _db.Set<InventoryBalance>().Where(b => b.ItemsCode == itemCode).ToList();

    private string? CheckPostable(IEnumerable<string> codes)
    {
        foreach (var code in codes.Distinct())
        {
            var acct = _db.Set<CoaEntity>().FirstOrDefault(a => a.AcctCode == code);
            if (acct == null) return $"G/L account {code} doesn't exist in the Chart of Accounts.";
            if (acct.IsGroup || !acct.IsActive || !acct.AllowPosting || acct.Frozen)
                return $"G/L account {code} can't be posted to (it's a title, inactive, frozen, or blocks posting).";
        }
        return null;
    }

    private JournalEntry BuildJournalEntry(StockRevaluation doc, GLDetermination gl, List<(string ItemCode, decimal Variance)> lines)
    {
        var date = doc.PostingDate!.Value;
        var je = new JournalEntry
        {
            JrnlNo          = JournalEntryNumberService.Next(_db, date),
            TransDate       = date,
            DueDate         = date,
            DocDate         = doc.DocumentDate ?? date,
            RefNo           = doc.Number,
            Memo            = string.IsNullOrWhiteSpace(doc.JournalRemark) ? $"Stock Revaluation {doc.Number}" : doc.JournalRemark,
            TransType       = "KSRV",
            BaseDocType     = "KSRV",
            BaseDocEntry    = doc.Id,
            ExchangeRate    = 1,
            Status          = "O",
            IsAutoGenerated = true,
            VersionNum      = 1,
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };

        var lineNum = 1;
        void Add(string account, decimal debit, decimal credit, string memo) => je.Lines.Add(new JournalEntryLine
        {
            LineNum     = lineNum++,
            AccountCode = account,
            AccountName = _db.Set<CoaEntity>().Where(a => a.AcctCode == account).Select(a => a.AcctName).FirstOrDefault() ?? account,
            Debit       = debit,
            Credit      = credit,
            DebitLC     = debit,
            CreditLC    = credit,
            LineMemo    = memo,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        });

        foreach (var (itemCode, variance) in lines.Where(l => l.Variance != 0))
        {
            var memo = $"{doc.Number} — {itemCode}";
            if (variance > 0)
            {
                Add(gl.InventoryAccount!, variance, 0, memo);
                Add(gl.InventoryRevaluationIncrease!, 0, variance, memo);
            }
            else
            {
                Add(gl.InventoryRevaluationDecrease!, -variance, 0, memo);
                Add(gl.InventoryAccount!, 0, -variance, memo);
            }
        }

        je.TotalDebit  = je.Lines.Sum(l => l.Debit);
        je.TotalCredit = je.Lines.Sum(l => l.Credit);
        je.TotalLC     = je.TotalDebit;
        je.TotalFC     = je.TotalDebit;
        return je;
    }
}
