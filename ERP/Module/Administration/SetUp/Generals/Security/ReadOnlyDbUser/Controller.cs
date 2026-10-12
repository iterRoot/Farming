using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.Security;

public class ReadOnlyDbUserController : MyController
{
    private readonly IReadOnlyDbUserRepository _repository;

    public ReadOnlyDbUserController(IReadOnlyDbUserRepository repository)
    {
        _repository = repository;
    }

    private static ReadOnlyDbUserListResponse ToDto(ReadOnlyDbUser u) => new()
    {
        Id = u.Id,
        Username = u.Username,
        Description = u.Description,
        Enabled = u.Enabled,
        CreatedAt = u.CreatedAt,
    };

    // ── GET /ReadOnlyDbUser ─────────────────────────────────────
    [HttpGet]
    public IActionResult Gets()
    {
        var rows = _repository.GetAll().OrderBy(x => x.Username).ToList().Select(ToDto);
        return Ok(rows);
    }

    // ── GET /ReadOnlyDbUser/{id} ─────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");
        return Ok(ToDto(item));
    }

    // ── POST /ReadOnlyDbUser ─────────────────────────────────────
    [HttpPost]
    public IActionResult Insert([FromBody] ReadOnlyDbUserRequest request)
    {
        var name = (request?.Username ?? "").Trim();
        if (name.Length == 0) return BadRequest("Username is required.");

        if (_repository.Existed(x => x.Username == name))
            return BadRequest($"'{name}' is already registered.");

        var item = new ReadOnlyDbUser
        {
            Username = name,
            Description = request!.Description?.Trim(),
            Enabled = request.Enabled,
            CreatedAt = DateTime.UtcNow,
        };
        _repository.Add(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    // ── PUT /ReadOnlyDbUser/{id} ─────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ReadOnlyDbUserRequest request)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");

        var name = (request?.Username ?? "").Trim();
        if (name.Length == 0) return BadRequest("Username is required.");

        if (name != item.Username && _repository.Existed(x => x.Username == name))
            return BadRequest($"'{name}' is already registered.");

        item.Username = name;
        item.Description = request!.Description?.Trim();
        item.Enabled = request.Enabled;
        item.UpdatedAt = DateTime.UtcNow;

        _repository.Update(item);
        _repository.Commit();
        return Ok(ToDto(item));
    }

    // ── DELETE /ReadOnlyDbUser?id= ────────────────────────────────
    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);
        if (item == null) return NotFound($"Item not found: {id}");
        _repository.Remove(item);
        _repository.Commit();
        return NoContent();
    }
}
