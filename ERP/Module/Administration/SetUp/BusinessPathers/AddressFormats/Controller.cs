using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.AddressFormats;

[ApiController]
[Route("[controller]")]
public class AddressFormatController : ControllerBase
{
    private readonly IAddressFormatRepository _repo;
    private readonly IMapper                  _mapper;
    private readonly MyDbContext              _db;

    public AddressFormatController(
        IAddressFormatRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /AddressFormat ─────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .Include(x => x.Lines.OrderBy(l => l.LineOrder))
            .OrderBy(x => x.IsDefault ? 0 : 1)
            .ThenBy(x => x.CountryCode)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<AddressFormatResponse>>(list));
    }

    // ── GET /AddressFormat/{id} ────────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Lines.OrderBy(l => l.LineOrder))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<AddressFormatResponse>(e));
    }

    // ── GET /AddressFormat/ByCountry/{code} ───────────────────
    [AllowAnonymous][HttpGet("ByCountry/{code}")]
    public IActionResult GetByCountry(string code)
    {
        var e = _repo.GetAll()
            .Include(x => x.Lines.OrderBy(l => l.LineOrder))
            .FirstOrDefault(x =>
                x.CountryCode == code.Trim().ToUpper() && x.IsActive)
            ?? _repo.GetAll()
                .Include(x => x.Lines.OrderBy(l => l.LineOrder))
                .FirstOrDefault(x => x.IsDefault && x.IsActive);

        if (e == null) return NotFound(new { message = "No address format found" });
        return Ok(_mapper.Map<AddressFormatResponse>(e));
    }

    // ── GET /AddressFormat/Default ────────────────────────────
    [AllowAnonymous][HttpGet("Default")]
    public IActionResult GetDefault()
    {
        var e = _repo.GetAll()
            .Include(x => x.Lines.OrderBy(l => l.LineOrder))
            .FirstOrDefault(x => x.IsDefault && x.IsActive)
            ?? _repo.GetAll()
                .Include(x => x.Lines.OrderBy(l => l.LineOrder))
                .FirstOrDefault();
        if (e == null) return NotFound();
        return Ok(_mapper.Map<AddressFormatResponse>(e));
    }

    // ── POST /AddressFormat ────────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] AddressFormatRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Address format code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(0);

        var entity           = _mapper.Map<AddressFormat>(dto);
        entity.Code          = code;
        entity.CountryCode   = dto.CountryCode?.Trim().ToUpper();
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;

        int order = 1;
        foreach (var l in entity.Lines) l.LineOrder = l.LineOrder > 0 ? l.LineOrder : order++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<AddressFormatResponse>(entity));
    }

    // ── PUT /AddressFormat/{id} ────────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AddressFormatRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Address format code '{code}' already exists");

        if (dto.IsDefault) UnsetDefault(id);

        _db.RemoveRange(entity.Lines);
        _mapper.Map(dto, entity);
        entity.Code        = code;
        entity.CountryCode = dto.CountryCode?.Trim().ToUpper();
        entity.UpdatedAt   = DateTime.UtcNow;

        int order = 1;
        foreach (var l in entity.Lines) l.LineOrder = l.LineOrder > 0 ? l.LineOrder : order++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<AddressFormatResponse>(entity));
    }

    // ── POST /AddressFormat/{id}/SetDefault ───────────────────
    [AllowAnonymous][HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        UnsetDefault(id);
        entity.IsDefault = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = $"'{entity.Name}' is now the default address format" });
    }

    // ── POST /AddressFormat/{id}/Preview ──────────────────────
    // Renders sample data through this format's template
    [AllowAnonymous][HttpPost("{id:int}/Preview")]
    public IActionResult Preview(int id, [FromBody] AddressPreviewRequest data)
    {
        var format = _repo.GetAll()
            .Include(x => x.Lines.OrderBy(l => l.LineOrder))
            .FirstOrDefault(x => x.Id == id);
        if (format == null) return NotFound();

        var fieldMap = BuildFieldMap(data);
        var rendered = new List<string>();

        foreach (var line in format.Lines.OrderBy(l => l.LineOrder))
        {
            var parts = new List<string>();
            var val1  = line.Field1 != null && fieldMap.TryGetValue(line.Field1, out var v1) ? v1 : "";
            var val2  = line.Field2 != null && fieldMap.TryGetValue(line.Field2, out var v2) ? v2 : "";
            var val3  = line.Field3 != null && fieldMap.TryGetValue(line.Field3, out var v3) ? v3 : "";

            if (!string.IsNullOrWhiteSpace(val1)) parts.Add(val1);
            if (!string.IsNullOrWhiteSpace(val2)) parts.Add(val2);
            if (!string.IsNullOrWhiteSpace(val3)) parts.Add(val3);

            if (parts.Count == 0 && !line.PrintIfEmpty) continue;

            // Join with separators
            string lineText = "";
            if (parts.Count >= 1) lineText = parts[0];
            if (parts.Count >= 2) lineText += (line.Separator12 ?? ", ") + parts[1];
            if (parts.Count >= 3) lineText += (line.Separator23 ?? " ") + parts[2];

            if (!string.IsNullOrWhiteSpace(lineText))
                rendered.Add(lineText.Trim());
        }

        return Ok(new AddressPreviewResponse
        {
            Lines      = rendered,
            Formatted  = string.Join("\n", rendered),
            SingleLine = string.Join(", ", rendered),
        });
    }

    // ── DELETE /AddressFormat/{id} ─────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsDefault)
            return BadRequest("Cannot delete the default address format. Set another as default first.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ─── Helpers ──────────────────────────────────────────────
    private void UnsetDefault(int exceptId)
    {
        var others = _repo.GetAll().Where(x => x.IsDefault && x.Id != exceptId).ToList();
        foreach (var d in others) { d.IsDefault = false; _repo.Update(d); }
    }

    private static Dictionary<string, string> BuildFieldMap(AddressPreviewRequest d) => new()
    {
        { "Attention",   d.Attention   ?? "" },
        { "CompanyName", d.CompanyName ?? "" },
        { "Street",      d.Street      ?? "" },
        { "HouseNumber", d.HouseNumber ?? "" },
        { "Building",    d.Building    ?? "" },
        { "Floor",       d.Floor       ?? "" },
        { "Apartment",   d.Apartment   ?? "" },
        { "Commune",     d.Commune     ?? "" },
        { "District",    d.District    ?? "" },
        { "Province",    d.Province    ?? "" },
        { "City",        d.City        ?? "" },
        { "PostalCode",  d.PostalCode  ?? "" },
        { "StateRegion", d.StateRegion ?? "" },
        { "Country",     d.Country     ?? "" },
        { "POBox",       d.POBox       ?? "" },
    };
}