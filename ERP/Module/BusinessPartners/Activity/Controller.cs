// using AutoMapper;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using FarmingApi.Core;

// namespace FarmingApi.Modules.BusinessPartner.Activity;

// public class ActivityController : MyController
// {
//     private readonly IMapper              _mapper;
//     private readonly IActivityRepository  _repository;

//     public ActivityController(IActivityRepository repository, IMapper mapper)
//     {
//         _mapper     = mapper;
//         _repository = repository;
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // GET ALL  — filter by status, type, assigned user, BP
//     // ═══════════════════════════════════════════════════════════════
//     [AllowAnonymous]
//     [HttpGet]
//     public IActionResult Gets(
//         [FromQuery] bool?   closed       = null,
//         [FromQuery] bool?   inactive     = null,
//         [FromQuery] string? activityType = null,
//         [FromQuery] string? assignedTo   = null,
//         [FromQuery] string? cardCode     = null,
//         [FromQuery] string? priority     = null,
//         [FromQuery] DateTime? from       = null,
//         [FromQuery] DateTime? to         = null)
//     {
//         var query = _repository.GetAll().AsQueryable();

//         if (closed.HasValue)                       query = query.Where(a => a.Closed   == closed.Value);
//         if (inactive.HasValue)                     query = query.Where(a => a.Inactive == inactive.Value);
//         if (!string.IsNullOrEmpty(activityType))   query = query.Where(a => a.ActivityType == activityType);
//         if (!string.IsNullOrEmpty(assignedTo))     query = query.Where(a => a.AssignedTo   == assignedTo);
//         if (!string.IsNullOrEmpty(cardCode))       query = query.Where(a => a.CardCode     == cardCode);
//         if (!string.IsNullOrEmpty(priority))       query = query.Where(a => a.Priority     == priority);
//         if (from.HasValue)                         query = query.Where(a => a.StartDate    >= from.Value);
//         if (to.HasValue)                           query = query.Where(a => a.StartDate    <= to.Value);

//         var result = query
//             .OrderByDescending(a => a.StartDate)
//             .ThenByDescending(a => a.Id)
//             .ToList();

//         return Ok(_mapper.Map<List<ActivityResponse>>(result));
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // GET BY ID
//     // ═══════════════════════════════════════════════════════════════
//     [AllowAnonymous]
//     [HttpGet("{id:int}")]
//     public IActionResult Get(int id)
//     {
//         var activity = _repository.GetSingle(a => a.Id == id);
//         if (activity == null) return NotFound($"Activity {id} not found");
//         return Ok(_mapper.Map<ActivityResponse>(activity));
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // CREATE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost]
//     public IActionResult Create([FromBody] ActivityCreateRequest request)
//     {
//         if (!ModelState.IsValid) return BadRequest(ModelState);

//         var entity = _mapper.Map<Activity>(request);
//         entity.Number    = GenerateNumber();
//         entity.Inactive  = false;
//         entity.Closed    = false;
//         entity.CreatedAt = DateTime.UtcNow;
//         entity.InActive  = false;

//         // Auto-calculate duration from start/end times
//         if (entity.EndDate >= entity.StartDate)
//         {
//             var startDt = entity.StartDate.Date + entity.StartTime;
//             var endDt   = entity.EndDate.Date   + entity.EndTime;
//             var span    = endDt - startDt;
//             if (span.TotalMinutes > 0)
//                 entity.Duration = FormatDuration(span);
//         }

//         _repository.Add(entity);
//         _repository.Commit();

//         return Ok(new
//         {
//             message  = "Activity created successfully",
//             id       = entity.Id,
//             number   = entity.Number,
//         });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // UPDATE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPut("{id:int}")]
//     public IActionResult Update(int id, [FromBody] ActivityUpdateRequest request)
//     {
//         var activity = _repository.GetSingle(a => a.Id == id);
//         if (activity == null) return NotFound($"Activity {id} not found");

//         _mapper.Map(request, activity);
//         activity.UpdatedAt = DateTime.UtcNow;

//         // Close date
//         if (request.Closed && activity.ClosedDate == null)
//             activity.ClosedDate = DateTime.UtcNow;
//         else if (!request.Closed)
//             activity.ClosedDate = null;

//         // Recalculate duration
//         var startDt = activity.StartDate.Date + activity.StartTime;
//         var endDt   = activity.EndDate.Date   + activity.EndTime;
//         var span    = endDt - startDt;
//         if (span.TotalMinutes > 0)
//             activity.Duration = FormatDuration(span);

//         _repository.Update(activity);
//         _repository.Commit();
//         return NoContent();
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // CLOSE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Close")]
//     public IActionResult Close(int id)
//     {
//         var activity = _repository.GetSingle(a => a.Id == id);
//         if (activity == null) return NotFound();
//         if (activity.Closed) return BadRequest("Activity is already closed");

//         activity.Closed     = true;
//         activity.ClosedDate = DateTime.UtcNow;
//         activity.UpdatedAt  = DateTime.UtcNow;

//         _repository.Update(activity);
//         _repository.Commit();
//         return Ok(new { message = "Activity closed", number = activity.Number });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // REOPEN
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/Reopen")]
//     public IActionResult Reopen(int id)
//     {
//         var activity = _repository.GetSingle(a => a.Id == id);
//         if (activity == null) return NotFound();
//         if (!activity.Closed) return BadRequest("Activity is not closed");

//         activity.Closed     = false;
//         activity.ClosedDate = null;
//         activity.UpdatedAt  = DateTime.UtcNow;

//         _repository.Update(activity);
//         _repository.Commit();
//         return Ok(new { message = "Activity reopened" });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // FOLLOW UP — create a new linked activity
//     // ═══════════════════════════════════════════════════════════════
//     [HttpPost("{id:int}/FollowUp")]
//     public IActionResult FollowUp(int id, [FromBody] ActivityCreateRequest request)
//     {
//         var original = _repository.GetSingle(a => a.Id == id);
//         if (original == null) return NotFound($"Activity {id} not found");

//         var followUp = _mapper.Map<Activity>(request);
//         followUp.Number      = GenerateNumber();
//         followUp.FollowUpId  = id;            // link back to original
//         followUp.CardCode    = request.CardCode    ?? original.CardCode;
//         followUp.CardName    = request.CardName    ?? original.CardName;
//         followUp.AssignedTo  = request.AssignedTo  ?? original.AssignedTo;
//         followUp.Inactive    = false;
//         followUp.Closed      = false;
//         followUp.CreatedAt   = DateTime.UtcNow;
//         followUp.InActive    = false;

//         _repository.Add(followUp);

//         // Close original after follow-up
//         original.Closed     = true;
//         original.ClosedDate = DateTime.UtcNow;
//         original.UpdatedAt  = DateTime.UtcNow;
//         _repository.Update(original);

//         _repository.Commit();

//         return Ok(new
//         {
//             message       = "Follow-up activity created",
//             id            = followUp.Id,
//             number        = followUp.Number,
//             originalNumber= original.Number,
//         });
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // DELETE
//     // ═══════════════════════════════════════════════════════════════
//     [HttpDelete("{id:int}")]
//     public IActionResult Delete(int id)
//     {
//         var activity = _repository.GetSingle(a => a.Id == id);
//         if (activity == null) return NotFound($"Activity {id} not found");
//         if (activity.Closed) return BadRequest("Cannot delete a Closed activity");

//         activity.DeletedAt = DateTime.UtcNow;
//         _repository.Remove(activity);
//         _repository.Commit();
//         return NoContent();
//     }

//     // ═══════════════════════════════════════════════════════════════
//     // HELPERS
//     // ═══════════════════════════════════════════════════════════════
//     private int GenerateNumber()
//     {
//         var last = _repository.GetAll()
//             .OrderByDescending(a => a.Number)
//             .FirstOrDefault();
//         return (last?.Number ?? 0) + 1;
//     }

//     private static string FormatDuration(TimeSpan span)
//     {
//         if (span.TotalHours >= 1)
//             return $"{(int)span.TotalHours} Hour{((int)span.TotalHours != 1 ? "s" : "")} {span.Minutes} Minutes";
//         return $"{(int)span.TotalMinutes} Minutes";
//     }
// }