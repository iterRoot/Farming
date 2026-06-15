using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FarmingApi.Modules.Administration.ElectronicDocuments;

[ApiController]
[Route("[controller]")]
public class ElectronicDocumentController : ControllerBase
{
    private readonly IElectronicDocumentRepository _repo;
    private readonly IMapper                       _mapper;
    private readonly MyDbContext                   _db;

    public ElectronicDocumentController(
        IElectronicDocumentRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── GET /ElectronicDocument ────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? documentType = null,
        [FromQuery] string? status       = null,
        [FromQuery] string? baseDocType  = null,
        [FromQuery] string? channel      = null,
        [FromQuery] string? bpCode       = null,
        [FromQuery] int     page         = 1,
        [FromQuery] int     pageSize     = 50)
    {
        var query = _repo.GetAll()
            .Where(x => documentType == null || x.DocumentType == documentType)
            .Where(x => status       == null || x.Status       == status)
            .Where(x => baseDocType  == null || x.BaseDocType  == baseDocType)
            .Where(x => channel      == null || x.Channel      == channel)
            .Where(x => bpCode       == null || x.BPCode       == bpCode)
            .OrderByDescending(x => x.CreatedAt);

        var total = query.Count();
        var list  = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Ok(new
        {
            total,
            page,
            pageSize,
            pages = (int)Math.Ceiling(total / (double)pageSize),
            data  = _mapper.Map<List<ElectronicDocumentResponse>>(list),
        });
    }

    // ── GET /ElectronicDocument/{id} ───────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetAll()
            .Include(x => x.Logs.OrderByDescending(l => l.LoggedAt))
            .FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(e));
    }

    // ── GET /ElectronicDocument/ByNo/{docNo} ──────────────────
    [AllowAnonymous][HttpGet("ByNo/{docNo}")]
    public IActionResult GetByNo(string docNo)
    {
        var e = _repo.GetAll()
            .Include(x => x.Logs)
            .FirstOrDefault(x => x.DocumentNo == docNo.Trim().ToUpper());
        if (e == null) return NotFound();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(e));
    }

    // ── GET /ElectronicDocument/Summary ───────────────────────
    [AllowAnonymous][HttpGet("Summary")]
    public IActionResult GetSummary()
    {
        var all = _repo.GetAll().ToList();
        return Ok(new EDocumentSummary
        {
            Total        = all.Count,
            Draft        = all.Count(x => x.Status == "Draft"),
            Submitted    = all.Count(x => x.Status == "Submitted"),
            Accepted     = all.Count(x => x.Status == "Accepted"),
            Rejected     = all.Count(x => x.Status == "Rejected"),
            Cancelled    = all.Count(x => x.Status == "Cancelled"),
            PendingRetry = all.Count(x => x.NextRetryAt.HasValue && x.NextRetryAt <= DateTime.UtcNow),
        });
    }

    // ── POST /ElectronicDocument ───────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] ElectronicDocumentRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity           = _mapper.Map<ElectronicDocument>(dto);
        entity.DocumentNo    = GenerateDocNo(dto.DocumentType);
        entity.Status        = "Draft";
        entity.GeneratedAt   = DateTime.UtcNow;
        entity.IsActive      = true;
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;

        AddLog(entity, "Generated", "Draft", "Draft", null, null, "Document generated");

        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(entity));
    }

    // ── PUT /ElectronicDocument/{id} ───────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ElectronicDocumentRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        if (!new[] { "Draft", "Rejected" }.Contains(entity.Status))
            return BadRequest($"Cannot edit a document in '{entity.Status}' status");

        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(entity));
    }

    // ── POST /ElectronicDocument/{id}/Submit ───────────────────
    [AllowAnonymous][HttpPost("{id:int}/Submit")]
    public IActionResult Submit(int id, [FromBody] SubmitRequest dto)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (!new[] { "Draft", "Rejected" }.Contains(entity.Status))
            return BadRequest($"Cannot submit a document in '{entity.Status}' status");

        var prev         = entity.Status;
        entity.Status    = "Submitted";
        entity.SubmittedAt  = DateTime.UtcNow;
        entity.SubmittedBy  = dto.SubmittedBy;
        entity.ExternalRef  = dto.ExternalRef;
        entity.SubmissionId = dto.SubmissionId;
        entity.RetryCount  += 1;
        entity.NextRetryAt  = null;
        entity.UpdatedAt    = DateTime.UtcNow;

        AddLog(entity, "Submitted", prev, "Submitted", dto.SubmittedBy, null, dto.Notes);
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(entity));
    }

    // ── POST /ElectronicDocument/{id}/Acknowledge ─────────────
    [AllowAnonymous][HttpPost("{id:int}/Acknowledge")]
    public IActionResult Acknowledge(int id, [FromBody] AcknowledgeRequest dto)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Submitted")
            return BadRequest($"Document must be 'Submitted' to acknowledge (current: {entity.Status})");

        var prev                = entity.Status;
        entity.Status           = dto.IsAccepted ? "Accepted" : "Rejected";
        entity.IsValid          = dto.IsAccepted;
        entity.AcknowledgedAt   = DateTime.UtcNow;
        entity.ExternalRef      = dto.ExternalRef ?? entity.ExternalRef;
        entity.ResponseCode     = dto.ResponseCode;
        entity.ResponseMessage  = dto.ResponseMessage;
        entity.UpdatedAt        = DateTime.UtcNow;

        if (!dto.IsAccepted)
        {
            entity.NextRetryAt = DateTime.UtcNow.AddHours(1);
        }

        AddLog(entity,
            dto.IsAccepted ? "Accepted" : "Rejected",
            prev, entity.Status, null,
            dto.ResponseCode, dto.ResponseMessage,
            dto.IsAccepted);

        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<ElectronicDocumentResponse>(entity));
    }

    // ── POST /ElectronicDocument/{id}/Sign ────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Sign")]
    public IActionResult Sign(int id,
        [FromQuery] string? certThumbprint = null,
        [FromQuery] string? signedBy       = null)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        entity.IsSigned      = true;
        entity.SignedAt      = DateTime.UtcNow;
        entity.SignatureCert = certThumbprint;
        entity.UpdatedAt     = DateTime.UtcNow;

        AddLog(entity, "Signed", entity.Status, entity.Status, signedBy, null, "Digitally signed");
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = "Document signed successfully", signedAt = entity.SignedAt });
    }

    // ── POST /ElectronicDocument/{id}/Retry ───────────────────
    [AllowAnonymous][HttpPost("{id:int}/Retry")]
    public IActionResult Retry(int id)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Rejected")
            return BadRequest("Only rejected documents can be retried");

        entity.Status      = "Draft";
        entity.IsValid     = false;
        entity.NextRetryAt = null;
        entity.UpdatedAt   = DateTime.UtcNow;

        AddLog(entity, "Retried", "Rejected", "Draft", null, null, $"Retry #{entity.RetryCount + 1}");
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = "Document reset to Draft for resubmission" });
    }

    // ── POST /ElectronicDocument/{id}/Cancel ──────────────────
    [AllowAnonymous][HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id, [FromQuery] string? reason = null)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Accepted")
            return BadRequest("Accepted documents cannot be cancelled directly. Issue a reversal.");

        var prev         = entity.Status;
        entity.Status    = "Cancelled";
        entity.IsActive  = false;
        entity.UpdatedAt = DateTime.UtcNow;

        AddLog(entity, "Cancelled", prev, "Cancelled", null, null, reason ?? "Cancelled by user");
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = "Document cancelled" });
    }

    // ── POST /ElectronicDocument/{id}/Archive ─────────────────
    [AllowAnonymous][HttpPost("{id:int}/Archive")]
    public IActionResult Archive(int id)
    {
        var entity = _repo.GetAll().Include(x => x.Logs).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status != "Accepted")
            return BadRequest("Only accepted documents can be archived");

        var prev         = entity.Status;
        entity.Status    = "Archived";
        entity.UpdatedAt = DateTime.UtcNow;

        AddLog(entity, "Archived", prev, "Archived", null, null, "Moved to archive");
        _repo.Update(entity);
        _repo.Commit();
        return Ok(new { message = "Document archived" });
    }

    // ── DELETE /ElectronicDocument/{id} ───────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.Status == "Accepted")
            return BadRequest("Accepted documents cannot be deleted for compliance reasons");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ─── Helpers ──────────────────────────────────────────────
    private string GenerateDocNo(string docType)
    {
        var prefix = docType switch
        {
            "EInvoice"      => "EINV",
            "ECreditNote"   => "ECNV",
            "EDeliveryNote" => "EDLN",
            "EStatement"    => "ESTE",
            "ERemittance"   => "ERM",
            "EOrder"        => "EORD",
            _               => "EDOC",
        };
        var year  = DateTime.UtcNow.Year;
        var count = _repo.GetAll().Count(x => x.DocumentType == docType) + 1;
        return $"{prefix}-{year}-{count:D5}";
    }

    private static void AddLog(
        ElectronicDocument entity,
        string eventType,
        string from, string to,
        string? by     = null,
        string? code   = null,
        string? msg    = null,
        bool success   = true)
    {
        entity.Logs.Add(new ElectronicDocumentLog
        {
            LoggedAt     = DateTime.UtcNow,
            EventType    = eventType,
            FromStatus   = from,
            ToStatus     = to,
            PerformedBy  = by,
            ResponseCode = code,
            Message      = msg,
            IsSuccess    = success,
        });
    }
}