using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.UserTables;

namespace FarmingApi.Modules.Administration.UserDefinedFields;

// ══════════════════════════════════════════════════════════════════
// USER-DEFINED FIELDS  (route: /UserDefinedField)
// GET    /UserDefinedField?table=ARInvoice → active fields for a table
// GET    /UserDefinedField/all             → every definition (admin)
// GET    /UserDefinedField/{id}            → one
// POST   /UserDefinedField                 → create a field
// PUT    /UserDefinedField/{id}            → update a field
// DELETE /UserDefinedField/{id}            → delete a field
// GET    /UserDefinedField/Targets         → the forms fields can be added to
// GET    /UserDefinedField/Values?table=&id=   → a record's field values
// PUT    /UserDefinedField/Values?table=&id=   → set them (validated against the field definitions)
// ══════════════════════════════════════════════════════════════════
public class UserDefinedFieldController : MyController
{
    private static readonly string[] AllowedTypes =
        { "Text", "Number", "Date", "Checkbox", "Dropdown" };

    // The forms a user-defined field can be added to: key → (label, the table's entity).
    public static readonly Dictionary<string, (string Label, string Entity)> Targets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BusinessPartner"] = ("Business Partner", "BusinessPartnersMaster"),
        ["Item"]            = ("Item Master", "ItemsMaster"),
        ["SaleOrder"]       = ("Sales Order", "SaleOrder"),
        ["ARInvoice"]       = ("AR Invoice", "ARInvoice"),
        ["PurchaseOrder"]   = ("Purchase Order", "PurchaseOrder"),
        ["APInvoice"]       = ("AP Invoice", "APInvoice"),
    };

    private readonly IUserDefinedFieldRepository _repo;
    private readonly IMapper _mapper;
    private readonly MyDbContext _db;

    public UserDefinedFieldController(IUserDefinedFieldRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    [HttpGet("Targets")]
    public IActionResult GetTargets() => Ok(Targets.Select(t => new { key = t.Key, label = t.Value.Label }));

    public class ValuesRequest { public Dictionary<string, string?>? Values { get; set; } }

    // Choices a Dropdown field allows: its own list, or the row codes of the user table it draws from.
    private List<string> Choices(UserDefinedField f) =>
        string.IsNullOrEmpty(f.LinkedTable) ? f.ValidValues
            : _db.Set<UserTableRow>().Where(r => r.UserTable!.Code == f.LinkedTable).Select(r => r.RowKey).OrderBy(x => x).ToList();

    private bool RecordExists(string entityName, int id)
    {
        var et = _db.Model.GetEntityTypes().FirstOrDefault(t => string.Equals(t.ClrType.Name, entityName, StringComparison.OrdinalIgnoreCase) && t.GetTableName() != null);
        if (et == null) return false;
        var pk = et.FindPrimaryKey()!.Properties[0].GetColumnName();
        return _db.Database.SqlQueryRaw<int>($"SELECT 1 AS \"Value\" FROM \"{et.GetTableName()}\" WHERE \"{pk}\" = {{0}}", id).AsEnumerable().Any();
    }

    [HttpGet("Values")]
    public IActionResult GetValues([FromQuery] string table, [FromQuery] int id)
    {
        if (!Targets.TryGetValue(table ?? "", out var tg)) return BadRequest($"'{table}' isn't a form user-defined fields can be added to.");
        var key = Targets.Keys.First(k => string.Equals(k, table, StringComparison.OrdinalIgnoreCase));
        return Ok(_db.Set<UdfValue>().Where(v => v.TableName == key && v.RecordId == id).ToDictionary(v => v.FieldName, v => v.Value));
    }

    [HttpPut("Values")]
    public IActionResult PutValues([FromQuery] string table, [FromQuery] int id, [FromBody] ValuesRequest req)
    {
        if (!Targets.TryGetValue(table ?? "", out var tg)) return BadRequest($"'{table}' isn't a form user-defined fields can be added to.");
        var key = Targets.Keys.First(k => string.Equals(k, table, StringComparison.OrdinalIgnoreCase));
        if (!RecordExists(tg.Entity, id)) return NotFound($"{tg.Label} {id} doesn't exist.");

        var defs = _db.Set<UserDefinedField>().Where(f => f.TableName == key && f.Active).OrderBy(f => f.SortOrder).ToList();
        var given = req.Values ?? new();
        foreach (var k in given.Keys)
            if (!defs.Any(d => string.Equals(d.FieldName, k, StringComparison.OrdinalIgnoreCase))) return BadRequest($"'{k}' isn't an active user-defined field on {tg.Label}.");

        var existing = _db.Set<UdfValue>().Where(v => v.TableName == key && v.RecordId == id).ToList();
        var final = new Dictionary<string, string>();
        foreach (var d in defs)
        {
            var hasGiven = given.Keys.Any(k => string.Equals(k, d.FieldName, StringComparison.OrdinalIgnoreCase));
            string? raw = hasGiven ? given.First(kv => string.Equals(kv.Key, d.FieldName, StringComparison.OrdinalIgnoreCase)).Value
                                   : existing.FirstOrDefault(e => e.FieldName == d.FieldName)?.Value;
            if (string.IsNullOrWhiteSpace(raw) && !hasGiven && existing.All(e => e.FieldName != d.FieldName)) raw = d.DefaultValue;
            var err = UserValue.Check(d.Label, d.FieldType, d.Length, d.Mandatory, d.FieldType == "Dropdown" ? Choices(d) : null, raw, out var val);
            if (err != null) return BadRequest(err);
            if (val.Length > 0) final[d.FieldName] = val;
        }

        foreach (var e in existing.Where(e => !final.ContainsKey(e.FieldName) && defs.Any(d => d.FieldName == e.FieldName))) _db.Set<UdfValue>().Remove(e);
        foreach (var (name, val) in final)
        {
            var e = existing.FirstOrDefault(x => x.FieldName == name);
            if (e == null) _db.Set<UdfValue>().Add(new UdfValue { TableName = key, RecordId = id, FieldName = name, Value = val });
            else e.Value = val;
        }
        _db.SaveChanges();
        return Ok(final);
    }

    // Active fields for a given form/table (used by the document forms).
    [HttpGet]
    public IActionResult Gets([FromQuery] string? table = null)
    {
        var q = _repo.GetAll().Where(x => x.Active);
        if (!string.IsNullOrWhiteSpace(table))
            q = q.Where(x => x.TableName == table);

        var list = q.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        return Ok(_mapper.Map<List<UserDefinedFieldResponse>>(list));
    }

    // Every definition including inactive ones (for the admin screen).
    [HttpGet("all")]
    public IActionResult GetAllDefs([FromQuery] string? table = null)
    {
        var q = _repo.GetAll();
        if (!string.IsNullOrWhiteSpace(table))
            q = q.Where(x => x.TableName == table);

        var list = q.OrderBy(x => x.TableName).ThenBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        return Ok(_mapper.Map<List<UserDefinedFieldResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        return e == null ? NotFound() : Ok(_mapper.Map<UserDefinedFieldResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] UserDefinedFieldRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = _mapper.Map<UserDefinedField>(req);
        e.FieldName = req.FieldName.Trim();
        e.TableName = Targets.Keys.FirstOrDefault(k => string.Equals(k, req.TableName.Trim(), StringComparison.OrdinalIgnoreCase)) ?? req.TableName.Trim();
        e.CreatedAt = DateTime.UtcNow;

        _repo.Add(e);
        _repo.Commit();

        return Ok(_mapper.Map<UserDefinedFieldResponse>(e));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] UserDefinedFieldUpdateRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"UDF {id} not found");

        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        // FieldName + TableName are the identity of the field — keep them stable.
        var name  = e.FieldName;
        var table = e.TableName;
        _mapper.Map(req, e);
        e.FieldName = name;
        e.TableName = table;
        e.UpdatedAt = DateTime.UtcNow;

        _repo.Update(e);
        _repo.Commit();

        return Ok(_mapper.Map<UserDefinedFieldResponse>(e));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"UDF {id} not found");

        _db.Set<UdfValue>().RemoveRange(_db.Set<UdfValue>().Where(v => v.TableName == e.TableName && v.FieldName == e.FieldName));
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private string? Validate(UserDefinedFieldRequest req, int? id)
    {
        if (string.IsNullOrWhiteSpace(req.TableName)) return "TableName is required";
        if (string.IsNullOrWhiteSpace(req.FieldName)) return "FieldName is required";
        if (string.IsNullOrWhiteSpace(req.Label))     return "Label is required";
        if (!AllowedTypes.Contains(req.FieldType))
            return $"FieldType must be one of: {string.Join(", ", AllowedTypes)}";
        if (!string.IsNullOrEmpty(req.LinkedTable))
        {
            if (req.FieldType != "Dropdown") return "Only a Dropdown field can take its choices from a user-defined table";
            var lt = req.LinkedTable.Trim().ToUpperInvariant();
            if (!_db.Set<UserTable>().Any(t => t.Code == lt)) return $"User-defined table '{req.LinkedTable}' doesn't exist";
            req.LinkedTable = lt;
            req.ValidValues = new();
        }
        else if (req.FieldType == "Dropdown" && req.ValidValues.Count == 0)
            return "A Dropdown field needs at least one valid value, or a user-defined table to take them from";
        if (id == null && !Targets.ContainsKey(req.TableName.Trim()))
            return $"Fields can be added to: {string.Join(", ", Targets.Keys)}";
        if (!string.IsNullOrWhiteSpace(req.DefaultValue))
        {
            var dv = UserValue.Check(req.Label, req.FieldType, req.Length, false, req.FieldType == "Dropdown" && string.IsNullOrEmpty(req.LinkedTable) ? req.ValidValues : null, req.DefaultValue, out _);
            if (dv != null && string.IsNullOrEmpty(req.LinkedTable)) return $"Default value: {dv}";
        }

        var name  = req.FieldName.Trim();
        var table = req.TableName.Trim();
        var clash = _repo.GetAll()
            .Any(x => x.TableName == table && x.FieldName == name && (id == null || x.Id != id));
        if (clash) return $"Field '{name}' already exists on {table}";

        return null;
    }
}


// When a record is deleted, its user-defined field values go with it.
public class UdfCleanupFilter : IAsyncActionFilter
{
    private readonly MyDbContext _db;
    public UdfCleanupFilter(MyDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext ctx, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (ctx.HttpContext.Request.Method != "DELETE" || executed.Exception != null) return;
        if (executed.Result is not (NoContentResult or OkResult or OkObjectResult)) return;
        if (ctx.ActionDescriptor is not Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad) return;
        if (!ctx.RouteData.Values.TryGetValue("id", out var idObj) || !int.TryParse(idObj?.ToString(), out var id)) return;
        var keys = UserDefinedFieldController.Targets.Where(t => string.Equals(t.Value.Entity, cad.ControllerName, StringComparison.OrdinalIgnoreCase)).Select(t => t.Key).ToList();
        if (keys.Count == 0) return;
        await _db.Set<UdfValue>().Where(v => keys.Contains(v.TableName) && v.RecordId == id).ExecuteDeleteAsync();
    }
}
