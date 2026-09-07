using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.UserDefinedFields;

// ══════════════════════════════════════════════════════════════════
// USER-DEFINED FIELDS  (route: /UserDefinedField)
// GET    /UserDefinedField?table=ARInvoice → active fields for a table
// GET    /UserDefinedField/all             → every definition (admin)
// GET    /UserDefinedField/{id}            → one
// POST   /UserDefinedField                 → create a field
// PUT    /UserDefinedField/{id}            → update a field
// DELETE /UserDefinedField/{id}            → delete a field
// ══════════════════════════════════════════════════════════════════
public class UserDefinedFieldController : MyController
{
    private static readonly string[] AllowedTypes =
        { "Text", "Number", "Date", "Checkbox", "Dropdown" };

    private readonly IUserDefinedFieldRepository _repo;
    private readonly IMapper _mapper;

    public UserDefinedFieldController(IUserDefinedFieldRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // Active fields for a given form/table (used by the document forms).
    [AllowAnonymous]
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
    [AllowAnonymous]
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
        e.TableName = req.TableName.Trim();
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
        if (req.FieldType == "Dropdown" && req.ValidValues.Count == 0)
            return "A Dropdown field needs at least one valid value";

        var name  = req.FieldName.Trim();
        var table = req.TableName.Trim();
        var clash = _repo.GetAll()
            .Any(x => x.TableName == table && x.FieldName == name && (id == null || x.Id != id));
        if (clash) return $"Field '{name}' already exists on {table}";

        return null;
    }
}
