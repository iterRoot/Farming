

// ═══════════════════════════════════════════════════════════════
// FILE: Controller.cs
// Single-record settings table — GET returns the one record,
// POST creates it if missing, PUT updates it.
// ═══════════════════════════════════════════════════════════════
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.GLAccountDetermination;

[ApiController]
[Route("[controller]")]
public class GLAccountDeterminationController : ControllerBase
{
    private readonly IGLAccountDeterminationRepository _repository;
    private readonly IMapper _mapper;

    public GLAccountDeterminationController(
        IGLAccountDeterminationRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    // ── GET /GLAccountDetermination ───────────────────────────
    // Returns the single settings record (or empty defaults)
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        var record = _repository.GetAll().FirstOrDefault();
        if (record == null)
            return Ok(new GLAccountDeterminationResponse()); // empty defaults
        return Ok(_mapper.Map<GLAccountDeterminationResponse>(record));
    }

    // ── POST /GLAccountDetermination ──────────────────────────
    // Creates the record (only if none exists)
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] GLAccountDeterminationRequest dto)
    {
        var existing = _repository.GetAll().FirstOrDefault();
        if (existing != null)
            return BadRequest("GL Account Determination already exists. Use PUT to update.");

        var entity = _mapper.Map<GLAccountDetermination>(dto);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<GLAccountDeterminationResponse>(entity));
    }

    // ── PUT /GLAccountDetermination ───────────────────────────
    // Updates the single record (upsert — creates if missing)
    [AllowAnonymous]
    [HttpPut]
    public IActionResult Update([FromBody] GLAccountDeterminationRequest dto)
    {
        var entity = _repository.GetAll().FirstOrDefault();

        if (entity == null)
        {
            // Auto-create on first save
            entity           = _mapper.Map<GLAccountDetermination>(dto);
            entity.CreatedAt = DateTime.UtcNow;
            entity.InActive  = false;
            _repository.Add(entity);
        }
        else
        {
            _mapper.Map(dto, entity);
            entity.UpdatedAt = DateTime.UtcNow;
            _repository.Update(entity);
        }

        _repository.Commit();
        return Ok(_mapper.Map<GLAccountDeterminationResponse>(entity));
    }
}