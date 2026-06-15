using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.DashboardManager;

[ApiController]
[Route("[controller]")]
public class DashboardManagerController : ControllerBase
{
    private readonly IDashboardManagerRepository _repo;
    private readonly IMapper                     _mapper;
    private readonly MyDbContext                 _db;

    public DashboardManagerController(
        IDashboardManagerRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /DashboardManager ──────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .Include(x => x.Widgets.OrderBy(w => w.Position))
            .OrderBy(x => x.IsDefault ? 0 : 1)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<DashboardConfigResponse>>(list));
    }

    // ── GET /DashboardManager/{id} ─────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Widgets.OrderBy(w => w.Position))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<DashboardConfigResponse>(e));
    }

    // ── GET /DashboardManager/Default ─────────────────────────
    [AllowAnonymous][HttpGet("Default")]
    public IActionResult GetDefault([FromQuery] string? role = null)
    {
        var query = _repo.GetAll()
            .Include(x => x.Widgets.OrderBy(w => w.Position))
            .Where(x => x.IsActive);

        DashboardConfig? result = null;

        // 1. Try role-specific default
        if (!string.IsNullOrEmpty(role))
            result = query.FirstOrDefault(x => x.IsDefault && x.RoleTarget == role);

        // 2. Fallback to global default
        result ??= query.FirstOrDefault(x => x.IsDefault && x.RoleTarget == "All");

        // 3. Fallback to any
        result ??= query.OrderByDescending(x => x.CreatedAt).FirstOrDefault();

        if (result == null)
            return NotFound(new { message = "No dashboard configured yet." });

        return Ok(_mapper.Map<DashboardConfigResponse>(result));
    }

    // ── POST /DashboardManager ─────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] DashboardConfigRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // If this is being set as default, unset others for same role
        if (dto.IsDefault)
            UnsetDefaultForRole(dto.RoleTarget, 0);

        var entity       = _mapper.Map<DashboardConfig>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        // Assign positions
        int pos = 1;
        foreach (var w in entity.Widgets) w.Position = pos++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<DashboardConfigResponse>(entity));
    }

    // ── PUT /DashboardManager/{id} ─────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DashboardConfigRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Widgets)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        if (dto.IsDefault)
            UnsetDefaultForRole(dto.RoleTarget, id);

        _db.RemoveRange(entity.Widgets);
        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;

        int pos = 1;
        foreach (var w in entity.Widgets) w.Position = pos++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<DashboardConfigResponse>(entity));
    }

    // ── POST /DashboardManager/{id}/SetDefault ─────────────────
    [AllowAnonymous][HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        UnsetDefaultForRole(entity.RoleTarget, id);
        entity.IsDefault = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.Name}' set as default dashboard" });
    }

    // ── POST /DashboardManager/{id}/Duplicate ──────────────────
    [AllowAnonymous][HttpPost("{id:int}/Duplicate")]
    public IActionResult Duplicate(int id)
    {
        var source = _repo.GetAll()
            .Include(x => x.Widgets)
            .FirstOrDefault(x => x.Id == id);
        if (source == null) return NotFound();

        var copy = new DashboardConfig
        {
            Name        = source.Name + " (Copy)",
            Description = source.Description,
            RoleTarget  = source.RoleTarget,
            LayoutType  = source.LayoutType,
            Columns     = source.Columns,
            ThemeColor  = source.ThemeColor,
            IsDefault   = false,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
            Widgets     = source.Widgets.Select(w => new DashboardWidget
            {
                Position  = w.Position, WidgetKey = w.WidgetKey,
                Title     = w.Title,    Module    = w.Module,
                WidgetType= w.WidgetType,Icon     = w.Icon,
                Route     = w.Route,    ColSpan   = w.ColSpan,
                IsVisible = w.IsVisible,Settings  = w.Settings,
            }).ToList(),
        };
        _repo.Add(copy);
        _repo.Commit();
        return Ok(_mapper.Map<DashboardConfigResponse>(copy));
    }

    // ── PUT /DashboardManager/{id}/Reorder ─────────────────────
    // Drag-and-drop widget reorder / toggle visibility
    [AllowAnonymous][HttpPut("{id:int}/Reorder")]
    public IActionResult Reorder(int id, [FromBody] WidgetReorderRequest dto)
    {
        var widgets = _db.Set<DashboardWidget>()
            .Where(w => w.DashboardConfigId == id)
            .ToList();

        foreach (var item in dto.Items)
        {
            var w = widgets.FirstOrDefault(x => x.Id == item.WidgetId);
            if (w == null) continue;
            w.Position  = item.Position;
            w.IsVisible = item.IsVisible;
        }
        _db.SaveChanges();
        return Ok(new { message = "Widget order updated" });
    }

    // ── DELETE /DashboardManager/{id} ─────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsDefault)
            return BadRequest("Cannot delete the default dashboard. Set another as default first.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ─── Helpers ──────────────────────────────────────────────
    private void UnsetDefaultForRole(string role, int exceptId)
    {
        var others = _repo.GetAll()
            .Where(x => x.IsDefault && x.RoleTarget == role && x.Id != exceptId)
            .ToList();
        foreach (var d in others) { d.IsDefault = false; _repo.Update(d); }
    }
}