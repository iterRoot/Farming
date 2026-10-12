using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FarmingApi.Core;
using JournalEntryDoc  = FarmingApi.Modules.Financials.JournalEntry.JournalEntry;
using JournalEntryLine = FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine;
using CoaEntity        = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;
using ItemEntity       = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using BPEntity         = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

// ═══════════════════════════════════════════════════════════════
// G/L ACCOUNT OVERRIDES (KGLO)
// The determination screen sets one account per role for the whole company; an override
// replaces it for one item group (revenue, cost of goods sold, inventory, purchase expense)
// or one business-partner group (receivables, payables). Postings pick the most specific
// account: partner's own control account → its group's override → the company default.
// ═══════════════════════════════════════════════════════════════
public class GLAccountOverride : AuditableEntity
{
    public string Scope       { get; set; } = null!;   // ItemGroup | BPGroup
    public string GroupKey    { get; set; } = null!;   // the item group / partner group text
    public string Field       { get; set; } = null!;   // a KGLD role, e.g. SalesRevenue
    public string AccountCode { get; set; } = null!;
}

public class GLAccountOverrideConfig : IEntityTypeConfiguration<GLAccountOverride>
{
    public void Configure(EntityTypeBuilder<GLAccountOverride> b)
    {
        b.ToTable("KGLO");
        b.HasKey(x => x.Id);
        b.Property(x => x.Scope).HasMaxLength(20).IsRequired();
        b.Property(x => x.GroupKey).HasMaxLength(100).IsRequired();
        b.Property(x => x.Field).HasMaxLength(50).IsRequired();
        b.Property(x => x.AccountCode).HasMaxLength(50).IsRequired();
        b.HasIndex(x => new { x.Scope, x.GroupKey, x.Field }).IsUnique();
    }
}

public static class GLOverrideFields
{
    public static readonly string[] ItemGroupFields = { "SalesRevenue", "CostOfGoodsSold", "InventoryAccount", "PurchaseExpense" };
    public static readonly string[] BPGroupFields   = { "AccountsReceivable", "AccountsPayable" };
}

// Picks the account for a posting and splits one journal line across item groups.
public class GLAccountResolver
{
    private readonly MyDbContext _db;
    private Dictionary<string, string>? _map;
    public GLAccountResolver(MyDbContext db) => _db = db;

    private static string Key(string scope, string group, string field) => $"{scope}|{group.Trim().ToUpperInvariant()}|{field}";

    private Dictionary<string, string> Map => _map ??= _db.Set<GLAccountOverride>().AsNoTracking().ToList()
        .ToDictionary(o => Key(o.Scope, o.GroupKey, o.Field), o => o.AccountCode);

    public string? ForItemGroup(string field, string? itemGroup, string? fallback) =>
        !string.IsNullOrWhiteSpace(itemGroup) && Map.TryGetValue(Key("ItemGroup", itemGroup, field), out var a) ? a : fallback;

    public string? ForBpGroup(string field, string? bpGroup, string? fallback) =>
        !string.IsNullOrWhiteSpace(bpGroup) && Map.TryGetValue(Key("BPGroup", bpGroup, field), out var a) ? a : fallback;

    /// <summary>
    /// Splits <paramref name="total"/> across the accounts its lines' item groups resolve to, in
    /// proportion to each line's weight; the rounding remainder goes to the largest share. With no
    /// overrides in play this returns one entry: (fallback, total).
    /// </summary>
    public List<(string Account, decimal Amount)> SplitByItemGroup(string field, string? fallback, decimal total,
        IEnumerable<(string? ItemCode, decimal Weight)> lines)
    {
        var single = new List<(string, decimal)> { (fallback ?? "", total) };
        if (Map.Count == 0 || string.IsNullOrEmpty(fallback)) return single;

        var list = lines.Where(l => l.Weight > 0).ToList();
        var codes = list.Select(l => l.ItemCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
        var groups = _db.Set<ItemEntity>().Where(i => codes.Contains(i.ItemCode)).ToDictionary(i => i.ItemCode, i => i.ItemGroup);

        var weight = new Dictionary<string, decimal>();
        foreach (var l in list)
        {
            var acct = ForItemGroup(field, l.ItemCode != null && groups.TryGetValue(l.ItemCode, out var g) ? g : null, fallback) ?? fallback!;
            weight[acct] = weight.GetValueOrDefault(acct) + l.Weight;
        }
        var sum = weight.Values.Sum();
        if (sum <= 0 || total <= 0) return single;

        var result = weight.Select(w => (Account: w.Key, Amount: Math.Round(total * w.Value / sum, 2))).ToList();
        var diff = total - result.Sum(r => r.Amount);
        if (diff != 0)
        {
            var i = result.IndexOf(result.OrderByDescending(r => r.Amount).First());
            result[i] = (result[i].Account, result[i].Amount + diff);
        }
        return result.Where(r => r.Amount != 0).ToList();
    }

    /// <summary>Replaces the line picked by <paramref name="pick"/> with one line per split (same side, same memo).</summary>
    public static void SplitLine(JournalEntryDoc je, Func<JournalEntryLine, bool> pick, List<(string Account, decimal Amount)> splits)
    {
        var line = je.Lines.FirstOrDefault(pick);
        if (line == null || splits.Count == 0) return;
        var debit = line.Debit > 0;
        var idx = je.Lines.ToList().IndexOf(line);
        line.AccountCode = splits[0].Account;
        SetAmount(line, debit, splits[0].Amount);
        for (var i = 1; i < splits.Count; i++)
        {
            var extra = new JournalEntryLine
            {
                AccountCode = splits[i].Account, AccountName = line.AccountName, CardCode = line.CardCode, CardName = line.CardName,
                LineMemo = line.LineMemo, TaxAmount = line.TaxAmount, CreatedAt = DateTime.UtcNow, InActive = false,
            };
            SetAmount(extra, debit, splits[i].Amount);
            je.Lines.Add(extra);
        }
        // keep line numbers unique and in order
        var n = 1;
        foreach (var l in je.Lines) l.LineNum = n++;
    }

    private static void SetAmount(JournalEntryLine l, bool debit, decimal amt)
    {
        l.Debit = debit ? amt : 0; l.DebitLC = debit ? amt : 0;
        l.Credit = debit ? 0 : amt; l.CreditLC = debit ? 0 : amt;
        l.DebitFC = 0; l.CreditFC = 0;
    }
}

// ═══════════════════════════════════════════════════════════════
// GET    /GLAccountOverride[?scope=]            → overrides
// GET    /GLAccountOverride/Groups              → item groups / partner groups in use, and the roles each can override
// PUT    /GLAccountOverride                     → upsert one { scope, groupKey, field, accountCode }
// DELETE /GLAccountOverride/{id}
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("GLAccountOverride")]
public class GLAccountOverrideController : ControllerBase
{
    private readonly MyDbContext _db;
    public GLAccountOverrideController(MyDbContext db) => _db = db;

    public class OverrideRequest
    {
        public string? Scope { get; set; }
        public string? GroupKey { get; set; }
        public string? Field { get; set; }
        public string? AccountCode { get; set; }
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string? scope = null) =>
        Ok(_db.Set<GLAccountOverride>().Where(o => scope == null || o.Scope == scope)
            .OrderBy(o => o.Scope).ThenBy(o => o.GroupKey).ThenBy(o => o.Field)
            .Select(o => new { o.Id, o.Scope, o.GroupKey, o.Field, o.AccountCode }).ToList());

    [HttpGet("Groups")]
    public IActionResult Groups() => Ok(new
    {
        itemGroups = _db.Set<ItemEntity>().Where(i => i.ItemGroup != "").Select(i => i.ItemGroup).Distinct().OrderBy(x => x).ToList(),
        bpGroups   = _db.Set<BPEntity>().Where(b => b.GroupCode != null && b.GroupCode != "").Select(b => b.GroupCode!).Distinct().OrderBy(x => x).ToList(),
        itemGroupFields = GLOverrideFields.ItemGroupFields,
        bpGroupFields   = GLOverrideFields.BPGroupFields,
    });

    [HttpPut]
    public IActionResult Put([FromBody] OverrideRequest r)
    {
        var scope = (r.Scope ?? "").Trim();
        var group = (r.GroupKey ?? "").Trim();
        var field = (r.Field ?? "").Trim();
        var acct  = (r.AccountCode ?? "").Trim();

        if (scope != "ItemGroup" && scope != "BPGroup") return BadRequest("Scope must be ItemGroup or BPGroup.");
        var allowed = scope == "ItemGroup" ? GLOverrideFields.ItemGroupFields : GLOverrideFields.BPGroupFields;
        if (!allowed.Contains(field)) return BadRequest($"A {scope} override can be set for: {string.Join(", ", allowed)}.");
        if (group.Length == 0 || group.Length > 100) return BadRequest("Choose the group this override applies to.");
        if (acct.Length == 0) return BadRequest("Account is required.");

        var exists = scope == "ItemGroup"
            ? _db.Set<ItemEntity>().Any(i => i.ItemGroup.ToUpper() == group.ToUpper())
            : _db.Set<BPEntity>().Any(b => b.GroupCode != null && b.GroupCode.ToUpper() == group.ToUpper());
        if (!exists) return BadRequest($"No {(scope == "ItemGroup" ? "item" : "business partner")} belongs to a group called '{group}'.");

        var coa = _db.Set<CoaEntity>().FirstOrDefault(a => a.AcctCode == acct);
        if (coa == null) return BadRequest($"G/L account {acct} doesn't exist in the Chart of Accounts.");
        if (coa.IsGroup || !coa.IsActive || !coa.AllowPosting || coa.Frozen)
            return BadRequest($"G/L account {acct} can't be posted to (it's a title, inactive, frozen, or blocks posting).");

        // canonical group text: the spelling the master data uses
        group = scope == "ItemGroup"
            ? _db.Set<ItemEntity>().Where(i => i.ItemGroup.ToUpper() == group.ToUpper()).Select(i => i.ItemGroup).First()
            : _db.Set<BPEntity>().Where(b => b.GroupCode != null && b.GroupCode.ToUpper() == group.ToUpper()).Select(b => b.GroupCode!).First();

        var o = _db.Set<GLAccountOverride>().FirstOrDefault(x => x.Scope == scope && x.GroupKey == group && x.Field == field);
        if (o == null)
        {
            o = new GLAccountOverride { Scope = scope, GroupKey = group, Field = field, CreatedAt = DateTime.UtcNow, InActive = false };
            _db.Set<GLAccountOverride>().Add(o);
        }
        else o.UpdatedAt = DateTime.UtcNow;
        o.AccountCode = acct;
        _db.SaveChanges();
        return Ok(new { o.Id, o.Scope, o.GroupKey, o.Field, o.AccountCode });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var o = _db.Set<GLAccountOverride>().FirstOrDefault(x => x.Id == id);
        if (o == null) return NotFound();
        _db.Set<GLAccountOverride>().Remove(o);
        _db.SaveChanges();
        return NoContent();
    }
}
