using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.CostAccounting;

// ═══════════════════════════════════════════════════════════════════
// COST CENTRE  (route: /CostCenter)
// GET    /CostCenter            → all, optional ?active=true|false
// GET    /CostCenter/{id}
// POST   /CostCenter
// PUT    /CostCenter/{id}       → code is immutable
// DELETE /CostCenter/{id}       → blocked while the balance is non-zero
// ═══════════════════════════════════════════════════════════════════
public class CostCenterController : MyController
{
    private readonly IMapper                _mapper;
    private readonly ICostCenterRepository  _repository;
    private readonly MyDbContext            _db;

    public CostCenterController(ICostCenterRepository repository, IMapper mapper, MyDbContext db)
    {
        _mapper     = mapper;
        _repository = repository;
        _db         = db;
    }

    [HttpGet]
    public IActionResult Gets([FromQuery] bool? active = null)
    {
        var query = _repository.GetAll();
        if (active.HasValue) query = query.Where(c => c.IsActive == active.Value);
        var result = _mapper.ProjectTo<CostCenterResponse>(query.OrderBy(c => c.CostCenterCode)).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var cc = _repository.GetSingle(c => c.Id == id);
        if (cc == null) return NotFound($"Cost centre {id} not found");
        return Ok(_mapper.Map<CostCenterResponse>(cc));
    }

    [HttpPost]
    public IActionResult Create([FromBody] CostCenterRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.CostCenterCode))
            return BadRequest("Cost Centre Code is required");
        if (string.IsNullOrWhiteSpace(request.CostCenterName))
            return BadRequest("Cost Centre Name is required");

        var code = request.CostCenterCode.Trim().ToUpperInvariant();
        if (_repository.GetSingle(c => c.CostCenterCode == code) != null)
            return BadRequest($"Cost centre code '{code}' already exists");
        // A journal line's dimension field takes either, so the two can't share a code.
        if (_db.Set<DistributionRule>().Any(r => r.RuleCode == code))
            return BadRequest($"'{code}' is already a distribution rule code");

        if (!string.IsNullOrWhiteSpace(request.ParentCode))
        {
            var parentCode = request.ParentCode.Trim().ToUpperInvariant();
            if (parentCode == code)
                return BadRequest("A cost centre cannot be its own parent");
            if (_repository.GetSingle(c => c.CostCenterCode == parentCode) == null)
                return BadRequest($"Parent cost centre '{parentCode}' does not exist");
        }

        var entity = _mapper.Map<CostCenter>(request);
        entity.CostCenterCode = code;
        entity.ParentCode     = request.ParentCode?.Trim().ToUpperInvariant();
        entity.VersionNum     = 1;
        entity.CreatedAt      = DateTime.UtcNow;
        entity.InActive       = false;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Cost centre created", id = entity.Id, code = entity.CostCenterCode });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] CostCenterUpdateRequest request)
    {
        var cc = _repository.GetSingle(c => c.Id == id);
        if (cc == null) return NotFound($"Cost centre {id} not found");
        if (string.IsNullOrWhiteSpace(request.CostCenterName))
            return BadRequest("Cost Centre Name is required");

        if (!string.IsNullOrWhiteSpace(request.ParentCode))
        {
            var parentCode = request.ParentCode.Trim().ToUpperInvariant();
            if (parentCode == cc.CostCenterCode)
                return BadRequest("A cost centre cannot be its own parent");
            if (_repository.GetSingle(c => c.CostCenterCode == parentCode) == null)
                return BadRequest($"Parent cost centre '{parentCode}' does not exist");
        }

        _mapper.Map(request, cc);
        cc.ParentCode  = request.ParentCode?.Trim().ToUpperInvariant();
        cc.UpdatedAt   = DateTime.UtcNow;
        cc.VersionNum += 1;

        _repository.Update(cc);
        _repository.Commit();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var cc = _repository.GetSingle(c => c.Id == id);
        if (cc == null) return NotFound($"Cost centre {id} not found");
        if (cc.Balance != 0)
            return BadRequest("Cannot delete a cost centre with a non-zero balance");
        if (_repository.GetAll().Any(c => c.ParentCode == cc.CostCenterCode))
            return BadRequest("Cannot delete a cost centre that still has child centres");
        var usedBy = _db.Set<DistributionRuleLine>().Count(l => l.CostCenterCode == cc.CostCenterCode);
        if (usedBy > 0) return BadRequest($"Cannot delete a cost centre that {usedBy} distribution rule line(s) split into");
        var jl = _db.Set<FarmingApi.Modules.Financials.JournalEntry.JournalEntryLine>().Count(l => l.CostCenter == cc.CostCenterCode);
        if (jl > 0) return BadRequest($"Cannot delete a cost centre used on {jl} journal line(s). Mark it inactive instead");
        var bl = _db.Set<FarmingApi.Modules.Financials.Budget.BudgetLine>().Count(l => l.CostCenter == cc.CostCenterCode);
        if (bl > 0) return BadRequest($"Cannot delete a cost centre used on {bl} budget line(s). Mark it inactive instead");

        cc.DeletedAt = DateTime.UtcNow;
        _repository.Remove(cc);
        _repository.Commit();
        return NoContent();
    }
}

// ═══════════════════════════════════════════════════════════════════
// DISTRIBUTION RULE  (route: /DistributionRule)
// GET    /DistributionRule
// GET    /DistributionRule/{id}
// POST   /DistributionRule            → line percentages must total 100
// PUT    /DistributionRule/{id}       → blocked once the rule is in use
// DELETE /DistributionRule/{id}       → blocked once the rule is in use
// ═══════════════════════════════════════════════════════════════════
public class DistributionRuleController : MyController
{
    private readonly IMapper                       _mapper;
    private readonly IDistributionRuleRepository   _repository;
    private readonly MyDbContext                   _db;

    public DistributionRuleController(IDistributionRuleRepository repository, IMapper mapper, MyDbContext db)
    {
        _mapper     = mapper;
        _repository = repository;
        _db         = db;
    }

    private string? ValidatePercentages(IReadOnlyCollection<DistributionRuleLineRequest> lines)
    {
        if (lines.Count == 0) return null;   // an empty rule is allowed; a posting to it is refused until it has lines
        if (lines.Any(l => l.Percentage <= 0))
            return "Every line's percentage must be greater than zero";
        var dup = lines.GroupBy(l => (l.CostCenterCode ?? "").Trim().ToUpper()).FirstOrDefault(g => g.Count() > 1);
        if (dup != null) return $"Cost centre '{dup.Key}' appears more than once";
        foreach (var l in lines)
        {
            var c = (l.CostCenterCode ?? "").Trim().ToUpper();
            var center = _db.Set<CostCenter>().FirstOrDefault(x => x.CostCenterCode == c);
            if (center == null) return $"Cost centre '{l.CostCenterCode}' doesn't exist";
            if (!center.IsActive || center.InActive == true) return $"Cost centre '{center.CostCenterCode}' is inactive";
        }
        var total = lines.Sum(l => l.Percentage);
        return Math.Abs(total - 100m) > 0.01m
            ? $"Line percentages must total 100% (currently {total:N2}%)"
            : null;
    }

    [HttpGet]
    public IActionResult Gets()
    {
        var query  = _repository.GetAll().Include(d => d.Lines).OrderBy(d => d.RuleCode);
        var result = _mapper.ProjectTo<DistributionRuleResponse>(query).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var dr = _repository.GetAll().Include(d => d.Lines).FirstOrDefault(d => d.Id == id);
        if (dr == null) return NotFound($"Distribution rule {id} not found");
        return Ok(_mapper.Map<DistributionRuleResponse>(dr));
    }

    [HttpPost]
    public IActionResult Create([FromBody] DistributionRuleRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.RuleCode))
            return BadRequest("Rule Code is required");
        if (string.IsNullOrWhiteSpace(request.RuleName))
            return BadRequest("Rule Name is required");

        var code = request.RuleCode.Trim().ToUpperInvariant();
        if (_repository.GetSingle(d => d.RuleCode == code) != null)
            return BadRequest($"Rule code '{code}' already exists");
        if (_db.Set<CostCenter>().Any(c => c.CostCenterCode == code))
            return BadRequest($"'{code}' is already a cost centre code");

        var pctError = ValidatePercentages(request.Lines);
        if (pctError != null) return BadRequest(pctError);

        var entity = _mapper.Map<DistributionRule>(request);
        entity.RuleCode   = code;
        entity.VersionNum = 1;
        entity.CreatedAt  = DateTime.UtcNow;
        entity.InActive   = false;

        var lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<DistributionRuleLine>(lr);
            line.LineNum        = lineNum++;
            line.CostCenterCode = lr.CostCenterCode.Trim().ToUpperInvariant();
            line.CostCenterName = lr.CostCenterName ?? line.CostCenterCode;
            line.CreatedAt      = DateTime.UtcNow;
            line.InActive       = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Distribution rule created", id = entity.Id, ruleCode = entity.RuleCode });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DistributionRuleUpdateRequest request)
    {
        var dr = _repository.GetAll().Include(d => d.Lines).FirstOrDefault(d => d.Id == id);
        if (dr == null) return NotFound($"Distribution rule {id} not found");
        if (dr.InUse) return BadRequest("Cannot edit a distribution rule that is already in use by postings");
        if (string.IsNullOrWhiteSpace(request.RuleName))
            return BadRequest("Rule Name is required");

        var pctError = ValidatePercentages(request.Lines);
        if (pctError != null) return BadRequest(pctError);

        _mapper.Map(request, dr);
        dr.UpdatedAt   = DateTime.UtcNow;
        dr.VersionNum += 1;

        // Lines are replaced wholesale — the child rows are owned by the rule
        // and carry no external references.
        dr.Lines.Clear();
        var lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<DistributionRuleLine>(lr);
            line.LineNum        = lineNum++;
            line.CostCenterCode = lr.CostCenterCode.Trim().ToUpperInvariant();
            line.CostCenterName = lr.CostCenterName ?? line.CostCenterCode;
            line.CreatedAt      = DateTime.UtcNow;
            line.InActive       = false;
            dr.Lines.Add(line);
        }

        _repository.Update(dr);
        _repository.Commit();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var dr = _repository.GetSingle(d => d.Id == id);
        if (dr == null) return NotFound($"Distribution rule {id} not found");
        if (dr.InUse) return BadRequest("Cannot delete a distribution rule that is in use");

        dr.DeletedAt = DateTime.UtcNow;
        _repository.Remove(dr);
        _repository.Commit();
        return NoContent();
    }
}
