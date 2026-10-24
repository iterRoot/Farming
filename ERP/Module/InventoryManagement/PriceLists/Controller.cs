using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.InventoryManagement.PriceLists;

// ═══════════════════════════════════════════════════════════════════
// PRICE LISTS  (route: /PriceList)
// GET    /PriceList           → all price lists
// GET    /PriceList/Options   → rounding methods/rules, authorisation groups
// GET    /PriceList/{id}
// POST   /PriceList
// PUT    /PriceList/{id}
// PUT    /PriceList/Batch     → save the whole grid in one transaction
// DELETE /PriceList/{id}      → refused while another list uses it as base,
//                               or a business partner is assigned to it
// GET    /PriceList/{id}/Items → every item with its price on this list
// PUT    /PriceList/{id}/Items → set or clear typed-in prices (null clears)
// GET    /PriceList/Price?listId=|listName=&itemCode= → one item's price
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class PriceListController : ControllerBase
{
    private readonly IPriceListRepository _repository;
    private readonly IMapper              _mapper;
    private readonly MyDbContext          _db;

    public PriceListController(IPriceListRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.BasePriceList).OrderBy(x => x.Id).ToList();
        return Ok(_mapper.Map<List<PriceListResponse>>(list));
    }

    [HttpGet("Options")]
    public IActionResult Options() => Ok(new PriceListOptionsResponse());

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.BasePriceList).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PriceListResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] PriceListRequest dto)
        => SaveRows(new List<PriceListBatchRow> { ToRow(null, dto) }, single: true);

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PriceListRequest dto)
        => SaveRows(new List<PriceListBatchRow> { ToRow(id, dto) }, single: true);

    [HttpPut("Batch")]
    public IActionResult Batch([FromBody] List<PriceListBatchRow> rows)
    {
        if (rows == null || rows.Count == 0) return BadRequest("Nothing to save.");
        return SaveRows(rows, single: false);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var dependents = _repository.GetAll()
            .Where(x => x.BasePriceListId == id && x.Id != id)
            .Select(x => x.Name)
            .ToList();
        if (dependents.Count > 0)
            return BadRequest($"'{entity.Name}' is the base price list of {string.Join(", ", dependents)} — " +
                              "change their base first.");

        var assigned = _db.Set<BPEntity>().Count(b => b.PriceList == entity.Name);
        if (assigned > 0)
            return BadRequest($"{assigned} business partner(s) use '{entity.Name}' — give them another price list first.");

        _repository.Remove(entity);   // its typed-in item prices go with it (cascade)
        _repository.Commit();
        return NoContent();
    }

    // ── Item prices ─────────────────────────────────────────────
    [HttpGet("{id:int}/Items")]
    public IActionResult GetItems(int id, [FromQuery] string? search = null)
    {
        var list = _repository.GetSingle(x => x.Id == id);
        if (list == null) return NotFound();

        var items = _db.Set<ItemEntity>().OrderBy(i => i.ItemCode).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            items = items.Where(i => i.ItemCode.Contains(search, StringComparison.OrdinalIgnoreCase)
                                  || i.ItemName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        var explicitPrices = _db.Set<ItemPrice>().Where(p => p.PriceListId == id).ToDictionary(p => p.ItemCode, p => p.UnitPrice);
        var resolved = PriceResolver.ResolveAll(_db, id, items);

        return Ok(items.Select(i =>
        {
            resolved.TryGetValue(i.ItemCode, out var r);
            return new ItemPriceRow
            {
                ItemCode = i.ItemCode, ItemName = i.ItemName, Uom = string.IsNullOrWhiteSpace(i.UomCode) ? i.UomName : i.UomCode,
                SalePrice = i.SalePrice,
                ExplicitPrice = explicitPrices.TryGetValue(i.ItemCode, out var e) ? e : null,
                EffectivePrice = r?.Price, Source = r?.Source, Via = r?.Via,
            };
        }));
    }

    [HttpPut("{id:int}/Items")]
    public IActionResult SetItems(int id, [FromBody] List<ItemPriceUpdate> rows)
    {
        var list = _repository.GetSingle(x => x.Id == id);
        if (list == null) return NotFound();
        if (rows == null || rows.Count == 0) return BadRequest("Nothing to save.");
        if (rows.Any(r => string.IsNullOrWhiteSpace(r.ItemCode))) return BadRequest("Every row needs an item code.");
        var dup = rows.GroupBy(r => r.ItemCode.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return BadRequest($"Item {dup.Key} appears more than once.");
        var negative = rows.FirstOrDefault(r => r.UnitPrice < 0);
        if (negative != null) return BadRequest($"The price for {negative.ItemCode} can't be negative.");

        var codes = rows.Select(r => r.ItemCode.Trim()).ToList();
        var known = _db.Set<ItemEntity>().Where(i => codes.Contains(i.ItemCode)).Select(i => i.ItemCode).ToHashSet();
        var unknown = codes.FirstOrDefault(c => !known.Contains(c));
        if (unknown != null) return BadRequest($"Item {unknown} not found.");

        var existing = _db.Set<ItemPrice>().Where(p => p.PriceListId == id && codes.Contains(p.ItemCode)).ToDictionary(p => p.ItemCode);
        int set = 0, cleared = 0;
        foreach (var r in rows)
        {
            var code = r.ItemCode.Trim();
            existing.TryGetValue(code, out var row);
            if (r.UnitPrice == null)
            {
                if (row != null) { _db.Set<ItemPrice>().Remove(row); cleared++; }
            }
            else if (row != null)
            {
                row.UnitPrice = r.UnitPrice.Value; row.UpdatedAt = DateTime.UtcNow; set++;
            }
            else
            {
                _db.Set<ItemPrice>().Add(new ItemPrice { PriceListId = id, ItemCode = code, UnitPrice = r.UnitPrice.Value, CreatedAt = DateTime.UtcNow, InActive = false });
                set++;
            }
        }
        _db.SaveChanges();
        return Ok(new { set, cleared });
    }

    // One item's price on one list, by list id or name (business partners store the name).
    [HttpGet("Price")]
    public IActionResult GetPrice([FromQuery] int? listId, [FromQuery] string? listName, [FromQuery] string itemCode)
    {
        var list = listId != null
            ? _repository.GetSingle(x => x.Id == listId)
            : _repository.GetSingle(x => x.Name == listName);
        if (list == null) return NotFound("Price list not found.");
        if (!list.IsActive) return BadRequest($"Price list '{list.Name}' is inactive.");
        var r = PriceResolver.Resolve(_db, list.Id, itemCode);
        if (r == null) return NotFound($"No price for {itemCode} on '{list.Name}'.");
        return Ok(new { listId = list.Id, listName = list.Name, currency = list.PrimaryCurrency, itemCode, price = r.Price, source = r.Source, via = r.Via });
    }

    private static PriceListBatchRow ToRow(int? id, PriceListRequest dto) => new()
    {
        Id                 = id,
        Name               = dto.Name,
        MultipleBase       = dto.MultipleBase,
        BasePriceListId    = dto.BasePriceListId,
        Factor             = dto.Factor,
        RoundingMethod     = dto.RoundingMethod,
        RoundingRule       = dto.RoundingRule,
        EndingInterval     = dto.EndingInterval,
        AuthorizationGroup = dto.AuthorizationGroup,
        GrossPrice         = dto.GrossPrice,
        PrimaryCurrency    = dto.PrimaryCurrency,
        IsActive           = dto.IsActive,
    };

    // Applies every row in memory, validates the resulting whole set (names
    // unique, bases exist, no base cycles), then commits once — so a grid
    // save either lands completely or not at all.
    private IActionResult SaveRows(List<PriceListBatchRow> rows, bool single)
    {
        var all      = _repository.GetAll().ToList();
        var existing = all.ToDictionary(x => x.Id);
        var touched  = new List<PriceList>();
        var renames  = new List<(string From, string To)>();

        for (var i = 0; i < rows.Count; i++)
        {
            var row   = rows[i];
            var label = single ? "" : $"Row {i + 1}: ";

            var error = ValidateFields(row, existing);
            if (error != null) return BadRequest(label + error);

            PriceList entity;
            if (row.Id is int id)
            {
                if (!existing.TryGetValue(id, out var found)) return single ? NotFound() : BadRequest($"{label}price list {id} not found.");
                entity = found;
                var oldName = found.Name;
                _mapper.Map<PriceListRequest, PriceList>(row, entity);
                if (!string.Equals(oldName, row.Name.Trim(), StringComparison.Ordinal)) renames.Add((oldName, row.Name.Trim()));
                entity.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                entity = _mapper.Map<PriceList>(row);
                entity.CreatedAt = DateTime.UtcNow;
                entity.InActive  = false;
                _repository.Add(entity);
                all.Add(entity);
            }

            entity.Name            = row.Name.Trim();
            entity.PrimaryCurrency = row.PrimaryCurrency.Trim().ToUpperInvariant();
            if (entity.BasePriceListId == entity.Id && entity.Id != 0) entity.BasePriceListId = null;
            if (!PriceListOptions.MethodsUsingEndingInterval.Contains(entity.RoundingMethod)) entity.EndingInterval = null;
            touched.Add(entity);
        }

        var duplicate = all.GroupBy(x => x.Name.Trim(), StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (duplicate != null) return BadRequest($"Price list name '{duplicate.Key}' is used more than once.");

        var cycle = FindBaseCycle(all);
        if (cycle != null) return BadRequest($"'{cycle}' would end up based on itself through a chain of base price lists.");

        // Business partners store their list by name, so a rename has to follow them.
        foreach (var (from, to) in renames)
            foreach (var bp in _db.Set<BPEntity>().Where(b => b.PriceList == from).ToList())
                bp.PriceList = to;

        _repository.Commit();

        if (single) return Ok(_mapper.Map<PriceListResponse>(ReloadWithBase(touched[0].Id)));
        return GetAll();
    }

    private static string? ValidateFields(PriceListBatchRow row, Dictionary<int, PriceList> existing)
    {
        if (string.IsNullOrWhiteSpace(row.Name))                 return "Price list name is required.";
        if (row.Name.Trim().Length > 100)                         return "Price list name is too long (max 100).";
        if (row.Factor <= 0)                                      return "Factor must be greater than zero.";
        if (!PriceListOptions.RoundingMethods.Contains(row.RoundingMethod))
            return $"Rounding method must be one of: {string.Join(", ", PriceListOptions.RoundingMethods)}.";
        if (!PriceListOptions.RoundingRules.Contains(row.RoundingRule))
            return $"Rounding rule must be one of: {string.Join(", ", PriceListOptions.RoundingRules)}.";
        if (PriceListOptions.MethodsUsingEndingInterval.Contains(row.RoundingMethod) && row.EndingInterval is not > 0)
            return $"'{row.RoundingMethod}' needs an Ending/Interval greater than zero.";
        if (row.AuthorizationGroup != null && !PriceListOptions.AuthorizationGroups.Contains(row.AuthorizationGroup))
            return $"Authorisation group must be one of: {string.Join(", ", PriceListOptions.AuthorizationGroups)}.";
        if (string.IsNullOrWhiteSpace(row.PrimaryCurrency) || row.PrimaryCurrency.Trim().Length > 10)
            return "Primary currency is required (max 10 characters).";
        if (row.BasePriceListId is int baseId && baseId != row.Id && !existing.ContainsKey(baseId))
            return $"Base price list {baseId} does not exist.";
        return null;
    }

    private static string? FindBaseCycle(List<PriceList> all)
    {
        var baseOf = all.Where(x => x.Id != 0).ToDictionary(x => x.Id, x => x.BasePriceListId);
        foreach (var start in all.Where(x => x.Id != 0))
        {
            var seen = new HashSet<int> { start.Id };
            var next = start.BasePriceListId;
            while (next is int n && baseOf.ContainsKey(n))
            {
                if (!seen.Add(n)) return start.Name;
                next = baseOf[n];
            }
        }
        return null;
    }

    private PriceList ReloadWithBase(int id) =>
        _repository.GetAll().Include(x => x.BasePriceList).First(x => x.Id == id);
}
