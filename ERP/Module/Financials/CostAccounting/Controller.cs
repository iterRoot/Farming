// using AutoMapper;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.Financials.CostAccounting;

// // ═══════════════════════════════════════════════════════════════════
// // COST CENTER CONTROLLER
// // ═══════════════════════════════════════════════════════════════════
// [Route("api/[controller]")]
// [ApiController]
// public class CostCenterController : MyController
// {
//     private readonly IMapper               _mapper;
//     private readonly ICostCenterRepository _repository;

//     public CostCenterController(ICostCenterRepository repository, IMapper mapper)
//     {
//         _mapper     = mapper;
//         _repository = repository;
//     }

//     [AllowAnonymous][HttpGet]
//     public IActionResult Gets([FromQuery] bool? active = null)
//     {
//         var query = _repository.GetAll().AsQueryable();
//         if (active.HasValue) query = query.Where(c => c.IsActive == active.Value);
//         var result = _mapper.ProjectTo<CostCenterResponse>(query.OrderBy(c => c.CostCenterCode)).ToList();
//         return Ok(result);
//     }

//     [AllowAnonymous][HttpGet("{id:int}")]
//     public IActionResult Get(int id)
//     {
//         var cc = _repository.GetSingle(c => c.Id == id);
//         if (cc == null) return NotFound($"Cost Center {id} not found");
//         return Ok(_mapper.Map<CostCenterResponse>(cc));
//     }

//     [HttpPost]
//     public IActionResult Create([FromBody] CostCenterRequest request)
//     {
//         if (!ModelState.IsValid) return BadRequest(ModelState);
//         if (string.IsNullOrWhiteSpace(request.CostCenterCode))
//             return BadRequest("Cost Center Code is required");
//         if (string.IsNullOrWhiteSpace(request.CostCenterName))
//             return BadRequest("Cost Center Name is required");

//         // Check duplicate code
//         var exists = _repository.GetAll().Any(c => c.CostCenterCode == request.CostCenterCode);
//         if (exists) return BadRequest($"Cost Center code '{request.CostCenterCode}' already exists");

//         var entity = _mapper.Map<CostCenter>(request);
//         entity.VersionNum = 1;
//         entity.CreatedAt  = DateTime.UtcNow;
//         entity.InActive   = false;

//         _repository.Add(entity);
//         _repository.Commit();

//         return Ok(new { message = "Cost Center created", id = entity.Id, code = entity.CostCenterCode });
//     }

//     [HttpPut("{id:int}")]
//     public IActionResult Update(int id, [FromBody] CostCenterUpdateRequest request)
//     {
//         var cc = _repository.GetSingle(c => c.Id == id);
//         if (cc == null) return NotFound($"Cost Center {id} not found");

//         _mapper.Map(request, cc);
//         cc.UpdatedAt   = DateTime.UtcNow;
//         cc.VersionNum += 1;

//         _repository.Update(cc);
//         _repository.Commit();
//         return NoContent();
//     }

//     [HttpDelete("{id:int}")]
//     public IActionResult Delete(int id)
//     {
//         var cc = _repository.GetSingle(c => c.Id == id);
//         if (cc == null) return NotFound($"Cost Center {id} not found");
//         if (cc.Balance != 0)
//             return BadRequest("Cannot delete Cost Center with non-zero balance");

//         cc.DeletedAt = DateTime.UtcNow;
//         _repository.Remove(cc);
//         _repository.Commit();
//         return NoContent();
//     }
// }

// // ═══════════════════════════════════════════════════════════════════
// // DISTRIBUTION RULE CONTROLLER
// // ═══════════════════════════════════════════════════════════════════
// [Route("api/[controller]")]
// [ApiController]
// public class DistributionRuleController : MyController
// {
//     private readonly IMapper                      _mapper;
//     private readonly IDistributionRuleRepository  _repository;

//     public DistributionRuleController(IDistributionRuleRepository repository, IMapper mapper)
//     {
//         _mapper     = mapper;
//         _repository = repository;
//     }

//     [AllowAnonymous][HttpGet]
//     public IActionResult Gets()
//     {
//         var query  = _repository.GetAll().Include(d => d.Lines).OrderBy(d => d.RuleCode);
//         var result = _mapper.ProjectTo<DistributionRuleResponse>(query).ToList();
//         return Ok(result);
//     }

//     [AllowAnonymous][HttpGet("{id:int}")]
//     public IActionResult Get(int id)
//     {
//         var dr = _repository.GetAll().Include(d => d.Lines).FirstOrDefault(d => d.Id == id);
//         if (dr == null) return NotFound($"Distribution Rule {id} not found");
//         return Ok(_mapper.Map<DistributionRuleResponse>(dr));
//     }

//     [HttpPost]
//     public IActionResult Create([FromBody] DistributionRuleRequest request)
//     {
//         if (!ModelState.IsValid) return BadRequest(ModelState);
//         if (string.IsNullOrWhiteSpace(request.RuleCode))
//             return BadRequest("Rule Code is required");

//         var exists = _repository.GetAll().Any(d => d.RuleCode == request.RuleCode);
//         if (exists) return BadRequest($"Rule code '{request.RuleCode}' already exists");

//         // Validate percentage total = 100
//         if (request.Lines.Any())
//         {
//             var total = request.Lines.Sum(l => l.Percentage);
//             if (Math.Abs(total - 100m) > 0.01m)
//                 return BadRequest($"Percentages must total 100% (current: {total:N2}%)");
//         }

//         var entity = _mapper.Map<DistributionRule>(request);
//         entity.VersionNum = 1;
//         entity.CreatedAt  = DateTime.UtcNow;
//         entity.InActive   = false;

//         int lineNum = 1;
//         foreach (var lr in request.Lines)
//         {
//             var line = _mapper.Map<DistributionRuleLine>(lr);
//             line.LineNum          = lineNum++;
//             line.CostCenterName   = lr.CostCenterName ?? lr.CostCenterCode;
//             line.CreatedAt        = DateTime.UtcNow;
//             line.InActive         = false;
//             entity.Lines.Add(line);
//         }

//         _repository.Add(entity);
//         _repository.Commit();

//         return Ok(new { message = "Distribution Rule created", id = entity.Id, ruleCode = entity.RuleCode });
//     }

//     [HttpPut("{id:int}")]
//     public IActionResult Update(int id, [FromBody] DistributionRuleUpdateRequest request)
//     {
//         var dr = _repository.GetAll().Include(d => d.Lines).FirstOrDefault(d => d.Id == id);
//         if (dr == null) return NotFound($"Distribution Rule {id} not found");
//         if (dr.InUse)   return BadRequest("Cannot edit a Distribution Rule that is in use by Journal Entries");

//         if (request.Lines.Any())
//         {
//             var total = request.Lines.Sum(l => l.Percentage);
//             if (Math.Abs(total - 100m) > 0.01m)
//                 return BadRequest($"Percentages must total 100% (current: {total:N2}%)");
//         }

//         _mapper.Map(request, dr);
//         dr.UpdatedAt   = DateTime.UtcNow;
//         dr.VersionNum += 1;

//         dr.Lines.Clear();
//         int lineNum = 1;
//         foreach (var lr in request.Lines)
//         {
//             var line = _mapper.Map<DistributionRuleLine>(lr);
//             line.LineNum        = lineNum++;
//             line.CostCenterName = lr.CostCenterName ?? lr.CostCenterCode;
//             line.CreatedAt      = DateTime.UtcNow;
//             line.InActive       = false;
//             dr.Lines.Add(line);
//         }

//         _repository.Update(dr);
//         _repository.Commit();
//         return NoContent();
//     }

//     [HttpDelete("{id:int}")]
//     public IActionResult Delete(int id)
//     {
//         var dr = _repository.GetSingle(d => d.Id == id);
//         if (dr == null) return NotFound($"Distribution Rule {id} not found");
//         if (dr.InUse)   return BadRequest("Cannot delete a Distribution Rule that is in use");

//         dr.DeletedAt = DateTime.UtcNow;
//         _repository.Remove(dr);
//         _repository.Commit();
//         return NoContent();
//     }
// }

// // ═══════════════════════════════════════════════════════════════════
// // PROJECT CONTROLLER
// // ═══════════════════════════════════════════════════════════════════
// [Route("api/[controller]")]
// [ApiController]
// public class ProjectController : MyController
// {
//     private readonly IMapper             _mapper;
//     private readonly IProjectRepository  _repository;

//     public ProjectController(IProjectRepository repository, IMapper mapper)
//     {
//         _mapper     = mapper;
//         _repository = repository;
//     }

//     [AllowAnonymous][HttpGet]
//     public IActionResult Gets([FromQuery] string? status = null)
//     {
//         var query = _repository.GetAll().AsQueryable();
//         if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);
//         var result = _mapper.ProjectTo<ProjectResponse>(query.OrderBy(p => p.ProjectCode)).ToList();
//         return Ok(result);
//     }

//     [AllowAnonymous][HttpGet("{id:int}")]
//     public IActionResult Get(int id)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound($"Project {id} not found");
//         return Ok(_mapper.Map<ProjectResponse>(proj));
//     }

//     [HttpPost]
//     public IActionResult Create([FromBody] ProjectRequest request)
//     {
//         if (!ModelState.IsValid) return BadRequest(ModelState);
//         if (string.IsNullOrWhiteSpace(request.ProjectCode))
//             return BadRequest("Project Code is required");
//         if (string.IsNullOrWhiteSpace(request.ProjectName))
//             return BadRequest("Project Name is required");

//         var exists = _repository.GetAll().Any(p => p.ProjectCode == request.ProjectCode);
//         if (exists) return BadRequest($"Project code '{request.ProjectCode}' already exists");

//         var entity = _mapper.Map<Project>(request);
//         entity.Status    = "P";   // Planning
//         entity.IsActive  = true;
//         entity.VersionNum = 1;
//         entity.CreatedAt = DateTime.UtcNow;
//         entity.InActive  = false;

//         _repository.Add(entity);
//         _repository.Commit();

//         return Ok(new { message = "Project created", id = entity.Id, projectCode = entity.ProjectCode });
//     }

//     [HttpPut("{id:int}")]
//     public IActionResult Update(int id, [FromBody] ProjectUpdateRequest request)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound($"Project {id} not found");
//         if (proj.Status == "C") return BadRequest("Cannot edit a Completed project");
//         if (proj.Status == "X") return BadRequest("Cannot edit a Cancelled project");

//         _mapper.Map(request, proj);
//         proj.UpdatedAt   = DateTime.UtcNow;
//         proj.VersionNum += 1;

//         _repository.Update(proj);
//         _repository.Commit();
//         return NoContent();
//     }

//     // ── Status transitions ──────────────────────────────────────
//     [HttpPost("{id:int}/Activate")]
//     public IActionResult Activate(int id)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound();
//         if (proj.Status != "P") return BadRequest("Only Planning projects can be activated");
//         proj.Status = "A"; proj.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(proj); _repository.Commit();
//         return Ok(new { message = "Project activated" });
//     }

//     [HttpPost("{id:int}/Complete")]
//     public IActionResult Complete(int id)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound();
//         if (proj.Status != "A") return BadRequest("Only Active projects can be completed");
//         proj.Status = "C"; proj.IsActive = false; proj.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(proj); _repository.Commit();
//         return Ok(new { message = "Project completed" });
//     }

//     [HttpPost("{id:int}/Cancel")]
//     public IActionResult Cancel(int id)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound();
//         if (proj.Status == "C") return BadRequest("Cannot cancel a completed project");
//         proj.Status = "X"; proj.IsActive = false; proj.UpdatedAt = DateTime.UtcNow;
//         _repository.Update(proj); _repository.Commit();
//         return Ok(new { message = "Project cancelled" });
//     }

//     [HttpDelete("{id:int}")]
//     public IActionResult Delete(int id)
//     {
//         var proj = _repository.GetSingle(p => p.Id == id);
//         if (proj == null) return NotFound($"Project {id} not found");
//         if (proj.Status == "A") return BadRequest("Cannot delete an Active project. Cancel it first.");

//         proj.DeletedAt = DateTime.UtcNow;
//         _repository.Remove(proj);
//         _repository.Commit();
//         return NoContent();
//     }
// }