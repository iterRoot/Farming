using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Sales.CommissionGroup;

[ApiController]
[Route("[controller]")]
public class CommissionGroupController : ControllerBase
{
    private readonly ICommissionGroupRepository _repo;
    private readonly IMapper                    _mapper;
    private readonly MyDbContext                _db;

    public CommissionGroupController(
        ICommissionGroupRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /CommissionGroup ───────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll()
            .Include(x => x.Tiers.OrderBy(t => t.TierOrder))
            .OrderBy(x => x.Code)
            .ToList();
        return Ok(_mapper.Map<List<CommissionGroupResponse>>(list));
    }

    // ── GET /CommissionGroup/{id} ──────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Tiers.OrderBy(t => t.TierOrder))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<CommissionGroupResponse>(e));
    }

    // ── POST /CommissionGroup ──────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] CommissionGroupRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (_repo.GetAll().Any(x => x.Code == dto.Code.Trim()))
            return BadRequest($"Commission group code '{dto.Code}' already exists");

        var entity       = _mapper.Map<CommissionGroup>(dto);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        // Assign tier order
        int order = 1;
        foreach (var tier in entity.Tiers)
            tier.TierOrder = order++;

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CommissionGroupResponse>(entity));
    }

    // ── PUT /CommissionGroup/{id} ──────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CommissionGroupRequest dto)
    {
        var entity = _repo.GetAll()
            .Include(x => x.Tiers)
            .FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (_repo.GetAll().Any(x => x.Code == dto.Code.Trim() && x.Id != id))
            return BadRequest($"Commission group code '{dto.Code}' already exists");

        // Remove old tiers, re-add from request
        _db.RemoveRange(entity.Tiers);
        _mapper.Map(dto, entity);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.UpdatedAt = DateTime.UtcNow;

        int order = 1;
        foreach (var tier in entity.Tiers)
            tier.TierOrder = order++;

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<CommissionGroupResponse>(entity));
    }

    // ── DELETE /CommissionGroup/{id} ───────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── GET /CommissionGroup/{id}/Calculate?amount=5000 ────────
    // Helper: calculate commission for a given sale amount
    [AllowAnonymous][HttpGet("{id:int}/Calculate")]
    public IActionResult Calculate(int id, [FromQuery] decimal amount)
    {
        var group = _repo.GetAll()
            .Include(x => x.Tiers.OrderBy(t => t.TierOrder))
            .FirstOrDefault(x => x.Id == id);
        if (group == null) return NotFound();
        if (group.MinSaleAmount.HasValue && amount < group.MinSaleAmount)
            return Ok(new { amount, commission = 0m, reason = "Below minimum sale amount" });

        decimal commission = 0;
        switch (group.CommissionType)
        {
            case "Percentage":
                commission = Math.Round(amount * group.CommissionRate / 100, 2);
                break;
            case "Fixed":
                commission = group.CommissionRate;
                break;
            case "Tiered":
                foreach (var tier in group.Tiers)
                {
                    if (amount >= tier.FromAmount &&
                        (tier.ToAmount == 0 || amount <= tier.ToAmount))
                    {
                        commission = Math.Round(amount * tier.Rate / 100, 2);
                        break;
                    }
                }
                break;
        }

        if (group.MaxCommission.HasValue)
            commission = Math.Min(commission, group.MaxCommission.Value);

        return Ok(new {
            amount,
            commission,
            commissionType = group.CommissionType,
            rate           = group.CommissionRate,
            currency       = group.Currency,
        });
    }
}