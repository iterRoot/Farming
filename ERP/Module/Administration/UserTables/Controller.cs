using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Administration.UserDefinedFields;

namespace FarmingApi.Modules.Administration.UserTables;

// ═══════════════════════════════════════════════════════════════
// USER-DEFINED TABLES  (route: /UserTable)
// GET    /UserTable                          → tables with column and row counts
// GET    /UserTable/{code}                   → one table with its columns
// POST   /UserTable                          → create  { code, name, description, fields[] }
// PUT    /UserTable/{code}                   → rename and replace the column list
// DELETE /UserTable/{code}                   → refused while it has rows or a field draws choices from it
// GET    /UserTable/{code}/Rows[?search=]    → rows
// POST   /UserTable/{code}/Rows              → add a row      { key, name, values{} }
// PUT    /UserTable/{code}/Rows/{rowId}      → change a row
// DELETE /UserTable/{code}/Rows/{rowId}
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("UserTable")]
public class UserTableController : ControllerBase
{
    private static readonly Regex CodePattern = new("^[A-Z][A-Z0-9_]{0,19}$", RegexOptions.Compiled);
    private static readonly Regex FieldPattern = new("^[A-Za-z][A-Za-z0-9_]{0,29}$", RegexOptions.Compiled);
    private static readonly Regex KeyPattern = new(@"^[A-Za-z0-9][A-Za-z0-9 _.\-/]{0,49}$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly MyDbContext _db;
    public UserTableController(MyDbContext db) => _db = db;

    public class FieldDto
    {
        public string FieldCode { get; set; } = "";
        public string Label { get; set; } = "";
        public string DataType { get; set; } = "Text";
        public int? Length { get; set; }
        public bool Required { get; set; }
        public string? DefaultValue { get; set; }
        public List<string>? ValidValues { get; set; }
    }
    public class TableRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<FieldDto>? Fields { get; set; }
    }
    public class RowRequest
    {
        public string? Key { get; set; }
        public string? Name { get; set; }
        public Dictionary<string, string?>? Values { get; set; }
    }

    private static object ToDto(UserTable t, int? rowCount = null) => new
    {
        t.Id, t.Code, t.Name, t.Description,
        fieldCount = t.Fields.Count, rowCount = rowCount ?? t.Rows.Count,
        fields = t.Fields.OrderBy(f => f.SortOrder).ThenBy(f => f.Id).Select(f => new
        { f.FieldCode, f.Label, f.DataType, f.Length, f.Required, f.DefaultValue, f.ValidValues }),
    };

    private UserTable? Load(string code, bool rows = false)
    {
        var c = (code ?? "").Trim().ToUpperInvariant();
        var q = _db.Set<UserTable>().Include(t => t.Fields).AsQueryable();
        if (rows) q = q.Include(t => t.Rows);
        return q.FirstOrDefault(t => t.Code == c);
    }

    private static Dictionary<string, string> Values(UserTableRow r)
    {
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(r.Data) ?? new(); } catch { return new(); }
    }

    // ── tables ────────────────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(_db.Set<UserTable>().Include(t => t.Fields).OrderBy(t => t.Code).ToList()
            .Select(t => ToDto(t, _db.Set<UserTableRow>().Count(r => r.UserTableId == t.Id))).ToList());

    [HttpGet("{code}")]
    public IActionResult Get(string code)
    {
        var t = Load(code);
        return t == null ? NotFound($"User table '{code}' not found.") : Ok(ToDto(t, _db.Set<UserTableRow>().Count(r => r.UserTableId == t.Id)));
    }

    private static string? CheckFields(List<FieldDto> fields)
    {
        if (fields.Count > 40) return "A user table can have at most 40 columns.";
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var f in fields)
        {
            var code = (f.FieldCode ?? "").Trim();
            if (!FieldPattern.IsMatch(code)) return $"Column code '{f.FieldCode}' must start with a letter and use only letters, digits and '_' (max 30).";
            if (!seen.Add(code)) return $"Column '{code}' appears twice.";
            if (string.IsNullOrWhiteSpace(f.Label)) return $"Column '{code}' needs a label.";
            if (!UserValue.Types.Contains(f.DataType)) return $"Column '{code}': type must be one of {string.Join(", ", UserValue.Types)}.";
            if (f.DataType == "Dropdown" && (f.ValidValues == null || f.ValidValues.Count(v => !string.IsNullOrWhiteSpace(v)) == 0)) return $"Column '{code}': a dropdown needs at least one allowed value.";
            if (f.Length is < 1 or > UserValue.MaxText) return $"Column '{code}': length must be between 1 and {UserValue.MaxText}.";
            if (!string.IsNullOrWhiteSpace(f.DefaultValue))
            {
                var err = UserValue.Check(f.Label, f.DataType, f.Length, false, f.ValidValues, f.DefaultValue, out _);
                if (err != null) return $"Column '{code}' default: {err}";
            }
        }
        return null;
    }

    private static void ApplyField(UserTableField e, FieldDto f, int order)
    {
        e.FieldCode = f.FieldCode.Trim(); e.Label = f.Label.Trim(); e.DataType = f.DataType; e.Length = f.DataType == "Text" ? f.Length : null;
        e.Required = f.Required; e.DefaultValue = string.IsNullOrWhiteSpace(f.DefaultValue) ? null : f.DefaultValue.Trim();
        e.ValidValues = f.DataType == "Dropdown" ? f.ValidValues!.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList() : new();
        e.SortOrder = order;
    }

    [HttpPost]
    public IActionResult Create([FromBody] TableRequest r)
    {
        var code = (r.Code ?? "").Trim().ToUpperInvariant();
        if (!CodePattern.IsMatch(code)) return BadRequest("Code must start with a letter and use only capital letters, digits and '_' (max 20), e.g. SEASONS.");
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest("Name is required.");
        if (_db.Set<UserTable>().Any(t => t.Code == code)) return BadRequest($"A user table '{code}' already exists.");
        var fields = r.Fields ?? new();
        var err = CheckFields(fields);
        if (err != null) return BadRequest(err);

        var t = new UserTable { Code = code, Name = r.Name.Trim(), Description = r.Description?.Trim(), CreatedAt = DateTime.UtcNow, InActive = false };
        var i = 0; foreach (var f in fields) { var e = new UserTableField(); ApplyField(e, f, i++); t.Fields.Add(e); }
        _db.Set<UserTable>().Add(t);
        _db.SaveChanges();
        return Ok(ToDto(t, 0));
    }

    [HttpPut("{code}")]
    public IActionResult Update(string code, [FromBody] TableRequest r)
    {
        var t = Load(code, rows: true);
        if (t == null) return NotFound($"User table '{code}' not found.");
        if (string.IsNullOrWhiteSpace(r.Name)) return BadRequest("Name is required.");
        var fields = r.Fields ?? new();
        var err = CheckFields(fields);
        if (err != null) return BadRequest(err);

        // Columns that already hold values can't be dropped, retyped or made mandatory over gaps.
        var rows = t.Rows.Select(x => (Row: x, Vals: Values(x))).ToList();
        foreach (var old in t.Fields)
        {
            var nf = fields.FirstOrDefault(f => string.Equals(f.FieldCode.Trim(), old.FieldCode, StringComparison.OrdinalIgnoreCase));
            var used = rows.Count(x => x.Vals.TryGetValue(old.FieldCode, out var v) && !string.IsNullOrEmpty(v));
            if (nf == null && used > 0) return BadRequest($"Column '{old.FieldCode}' holds values in {used} row(s) and can't be removed. Clear them first.");
            if (nf != null && nf.DataType != old.DataType && used > 0) return BadRequest($"Column '{old.FieldCode}' holds values in {used} row(s), so its type can't change.");
            if (nf != null && nf.DataType == "Dropdown" && used > 0)
            {
                var allowed = nf.ValidValues ?? new();
                var bad = rows.Select(x => x.Vals.GetValueOrDefault(old.FieldCode)).FirstOrDefault(v => !string.IsNullOrEmpty(v) && !allowed.Any(a => string.Equals(a, v, StringComparison.OrdinalIgnoreCase)));
                if (bad != null) return BadRequest($"Column '{old.FieldCode}': '{bad}' is used by a row and can't be taken off the allowed values.");
            }
        }
        foreach (var nf in fields.Where(f => f.Required))
        {
            var missing = rows.Count(x => string.IsNullOrEmpty(x.Vals.GetValueOrDefault(nf.FieldCode.Trim())) && string.IsNullOrWhiteSpace(nf.DefaultValue));
            if (missing > 0 && !t.Fields.Any(o => string.Equals(o.FieldCode, nf.FieldCode.Trim(), StringComparison.OrdinalIgnoreCase) && o.Required))
                return BadRequest($"Column '{nf.FieldCode}' can't become mandatory: {missing} row(s) have no value for it.");
        }

        t.Name = r.Name.Trim(); t.Description = r.Description?.Trim(); t.UpdatedAt = DateTime.UtcNow;
        var order = 0;
        foreach (var f in fields)
        {
            var e = t.Fields.FirstOrDefault(x => string.Equals(x.FieldCode, f.FieldCode.Trim(), StringComparison.OrdinalIgnoreCase));
            if (e == null) { e = new UserTableField(); t.Fields.Add(e); }
            ApplyField(e, f, order++);
        }
        foreach (var gone in t.Fields.Where(x => !fields.Any(f => string.Equals(f.FieldCode.Trim(), x.FieldCode, StringComparison.OrdinalIgnoreCase))).ToList())
            _db.Set<UserTableField>().Remove(gone);
        _db.SaveChanges();
        return Ok(ToDto(t));
    }

    [HttpDelete("{code}")]
    public IActionResult Delete(string code)
    {
        var t = Load(code);
        if (t == null) return NotFound($"User table '{code}' not found.");
        var rows = _db.Set<UserTableRow>().Count(r => r.UserTableId == t.Id);
        if (rows > 0) return BadRequest($"'{t.Code}' still has {rows} row(s). Delete them first.");
        var links = _db.Set<UserDefinedField>().Where(f => f.LinkedTable == t.Code).Select(f => f.TableName + "." + f.FieldName).ToList();
        if (links.Count > 0) return BadRequest($"'{t.Code}' can't be deleted: user-defined field(s) {string.Join(", ", links)} take their choices from it.");
        _db.Set<UserTable>().Remove(t);
        _db.SaveChanges();
        return NoContent();
    }

    // ── rows ──────────────────────────────────────────────────
    private object RowDto(UserTable t, UserTableRow r)
    {
        var v = Values(r);
        return new { r.Id, key = r.RowKey, name = r.RowName, values = t.Fields.OrderBy(f => f.SortOrder).ToDictionary(f => f.FieldCode, f => v.GetValueOrDefault(f.FieldCode)) };
    }

    [HttpGet("{code}/Rows")]
    public IActionResult Rows(string code, [FromQuery] string? search = null)
    {
        var t = Load(code);
        if (t == null) return NotFound($"User table '{code}' not found.");
        var q = _db.Set<UserTableRow>().Where(r => r.UserTableId == t.Id);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToUpperInvariant();
            q = q.Where(r => r.RowKey.ToUpper().Contains(s) || (r.RowName != null && r.RowName.ToUpper().Contains(s)));
        }
        return Ok(q.OrderBy(r => r.RowKey).ToList().Select(r => RowDto(t, r)).ToList());
    }

    private string? BuildValues(UserTable t, RowRequest r, out Dictionary<string, string> stored)
    {
        stored = new();
        var given = r.Values ?? new();
        foreach (var k in given.Keys)
            if (!t.Fields.Any(f => string.Equals(f.FieldCode, k, StringComparison.OrdinalIgnoreCase))) return $"'{k}' isn't a column of {t.Code}.";
        foreach (var f in t.Fields)
        {
            var raw = given.FirstOrDefault(kv => string.Equals(kv.Key, f.FieldCode, StringComparison.OrdinalIgnoreCase)).Value;
            if (string.IsNullOrWhiteSpace(raw)) raw = f.DefaultValue;
            var err = UserValue.Check(f.Label, f.DataType, f.Length, f.Required, f.ValidValues, raw, out var val);
            if (err != null) return err;
            if (val.Length > 0) stored[f.FieldCode] = val;
        }
        return null;
    }

    private string? CheckKey(UserTable t, RowRequest r, int? ignoreId)
    {
        var key = (r.Key ?? "").Trim();
        if (!KeyPattern.IsMatch(key)) return "Code must be 1–50 letters, digits, spaces or . _ - / and start with a letter or digit.";
        if ((r.Name ?? "").Length > 200) return "Name can't be longer than 200 characters.";
        var up = key.ToUpperInvariant();
        if (_db.Set<UserTableRow>().Any(x => x.UserTableId == t.Id && x.RowKey.ToUpper() == up && (ignoreId == null || x.Id != ignoreId))) return $"A row with code '{key}' already exists.";
        return null;
    }

    [HttpPost("{code}/Rows")]
    public IActionResult AddRow(string code, [FromBody] RowRequest r)
    {
        var t = Load(code);
        if (t == null) return NotFound($"User table '{code}' not found.");
        var err = CheckKey(t, r, null) ?? BuildValues(t, r, out _);
        if (err != null) return BadRequest(err);
        BuildValues(t, r, out var stored);
        var row = new UserTableRow { UserTableId = t.Id, RowKey = r.Key!.Trim(), RowName = r.Name?.Trim(), Data = JsonSerializer.Serialize(stored, Json) };
        _db.Set<UserTableRow>().Add(row);
        _db.SaveChanges();
        return Ok(RowDto(t, row));
    }

    [HttpPut("{code}/Rows/{rowId:int}")]
    public IActionResult UpdateRow(string code, int rowId, [FromBody] RowRequest r)
    {
        var t = Load(code);
        if (t == null) return NotFound($"User table '{code}' not found.");
        var row = _db.Set<UserTableRow>().FirstOrDefault(x => x.Id == rowId && x.UserTableId == t.Id);
        if (row == null) return NotFound("Row not found.");
        var err = CheckKey(t, r, rowId) ?? BuildValues(t, r, out _);
        if (err != null) return BadRequest(err);
        if (!string.Equals(row.RowKey, r.Key!.Trim(), StringComparison.Ordinal))
        {
            var use = _db.Set<UdfValue>().Any(v => v.Value == row.RowKey && _db.Set<UserDefinedField>().Any(f => f.LinkedTable == t.Code && f.TableName == v.TableName && f.FieldName == v.FieldName));
            if (use) return BadRequest($"Code '{row.RowKey}' is used as a field value on records, so it can't be renamed.");
        }
        BuildValues(t, r, out var stored);
        row.RowKey = r.Key!.Trim(); row.RowName = r.Name?.Trim(); row.Data = JsonSerializer.Serialize(stored, Json); row.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Ok(RowDto(t, row));
    }

    [HttpDelete("{code}/Rows/{rowId:int}")]
    public IActionResult DeleteRow(string code, int rowId)
    {
        var t = Load(code);
        if (t == null) return NotFound($"User table '{code}' not found.");
        var row = _db.Set<UserTableRow>().FirstOrDefault(x => x.Id == rowId && x.UserTableId == t.Id);
        if (row == null) return NotFound("Row not found.");
        var use = _db.Set<UdfValue>().Count(v => v.Value == row.RowKey && _db.Set<UserDefinedField>().Any(f => f.LinkedTable == t.Code && f.TableName == v.TableName && f.FieldName == v.FieldName));
        if (use > 0) return BadRequest($"Code '{row.RowKey}' is used as a field value on {use} record(s) and can't be deleted.");
        _db.Set<UserTableRow>().Remove(row);
        _db.SaveChanges();
        return NoContent();
    }
}
