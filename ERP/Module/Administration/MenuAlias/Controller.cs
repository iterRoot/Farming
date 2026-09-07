using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.MenuAlias;

// ═══════════════════════════════════════════════════════════════
// MENU ALIAS FOR SEARCHING  (route: /MenuAlias)
// GET    /MenuAlias           → all aliases (optional ?search=)
// GET    /MenuAlias/{id}      → one
// POST   /MenuAlias           → create
// PUT    /MenuAlias/{id}      → update
// DELETE /MenuAlias/{id}      → delete
// ═══════════════════════════════════════════════════════════════
public class MenuAliasController : MyController
{
    private readonly IMenuAliasRepository _repo;

    public MenuAliasController(IMenuAliasRepository repo) => _repo = repo;

    private static MenuAliasDto ToDto(MenuAliasEntry x) => new()
    {
        Id = x.Id, Alias = x.Alias, MenuName = x.MenuName, MenuPath = x.MenuPath,
        Module = x.Module, Language = x.Language, InActive = x.InActive ?? false,
    };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? search)
    {
        var q = _repo.GetAll().AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLowerInvariant();
            q = q.Where(x =>
                x.Alias.ToLowerInvariant().Contains(s) ||
                x.MenuName.ToLowerInvariant().Contains(s));
        }
        var list = q.OrderBy(x => x.Alias).Select(ToDto).ToList();
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Alias {id} not found.");
        return Ok(ToDto(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] MenuAliasSaveRequest req)
    {
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        var e = new MenuAliasEntry { CreatedAt = DateTime.UtcNow };
        Apply(e, req);
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Alias created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] MenuAliasSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Alias {id} not found.");
        var err = Validate(req);
        if (err != null) return BadRequest(err);

        Apply(e, req);
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Alias updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Alias {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private static void Apply(MenuAliasEntry e, MenuAliasSaveRequest r)
    {
        e.Alias = r.Alias.Trim();
        e.MenuName = r.MenuName.Trim();
        e.MenuPath = string.IsNullOrWhiteSpace(r.MenuPath) ? null : r.MenuPath.Trim();
        e.Module = string.IsNullOrWhiteSpace(r.Module) ? null : r.Module.Trim();
        e.Language = string.IsNullOrWhiteSpace(r.Language) ? null : r.Language.Trim();
        e.InActive = r.InActive;
    }

    private static string? Validate(MenuAliasSaveRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.Alias))    return "Alias keyword is required.";
        if (string.IsNullOrWhiteSpace(r.MenuName)) return "Menu item name is required.";
        return null;
    }
}
