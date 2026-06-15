using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DocumentNumberRange;

public class DocumentNumberRangeController : MyController
{
    private readonly IDocumentNumberRangeRepository _repository;

    public DocumentNumberRangeController(IDocumentNumberRangeRepository repository)
        => _repository = repository;

    // ═══════════════════════════════════════════════════════════════
    // GET ALL — grouped by module
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets([FromQuery] string? documentType = null)
    {
        var query = _repository.GetAll().AsQueryable();
        if (!string.IsNullOrEmpty(documentType))
            query = query.Where(d => d.DocumentType == documentType);

        var list = query.OrderBy(d => d.DocumentType)
                        .ThenByDescending(d => d.IsDefault)
                        .ThenBy(d => d.SeriesName)
                        .ToList();

        return Ok(list.Select(MapResponse));
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var d = _repository.GetSingle(x => x.Id == id);
        if (d == null) return NotFound();
        return Ok(MapResponse(d));
    }

    // ═══════════════════════════════════════════════════════════════
    // GET NEXT NUMBER — called by other modules when creating a doc
    // GET /DocumentNumberRange/Next/{documentType}
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("Next/{documentType}")]
    public IActionResult GetNext(string documentType, [FromQuery] string? series = null)
    {
        try
        {
            var result = _repository.GenerateNextNumber(documentType, series);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PEEK NEXT — preview next number WITHOUT reserving it
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("Peek/{documentType}")]
    public IActionResult PeekNext(string documentType, [FromQuery] string? series = null)
    {
        var range = string.IsNullOrEmpty(series)
            ? _repository.GetDefault(documentType)
            : _repository.GetAll().FirstOrDefault(d =>
                d.DocumentType == documentType && d.SeriesName == series);

        if (range == null) return NotFound($"No number range for '{documentType}'");

        return Ok(new NextDocNumberResponse
        {
            DocNo      = DocumentNumberRangeRepository.FormatDocNo(range, range.NextNum),
            Number     = range.NextNum,
            SeriesName = range.SeriesName,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] DocumentNumberRangeCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.DocumentType))
            return BadRequest("Document Type is required");
        if (string.IsNullOrWhiteSpace(req.Prefix))
            return BadRequest("Prefix is required");

        // Resolve doc type info
        var info = DocTypes.Find(req.DocumentType);
        if (info == null) return BadRequest($"Unknown document type '{req.DocumentType}'");

        // Check duplicate series
        var exists = _repository.GetAll().Any(d =>
            d.DocumentType == req.DocumentType && d.SeriesName == req.SeriesName);
        if (exists) return BadRequest($"Series '{req.SeriesName}' already exists for '{req.DocumentType}'");

        // Clear default if this is new default
        if (req.IsDefault) ClearDefault(req.DocumentType);

        var entity = new DocumentNumberRange
        {
            DocumentType     = req.DocumentType,
            DocumentTypeCode = info.DefaultPrefix,
            SeriesName       = req.SeriesName,
            IsDefault        = req.IsDefault,
            IsLocked         = false,
            Prefix           = req.Prefix.ToUpper().Trim(),
            IncludeYear      = req.IncludeYear,
            PadLength        = Math.Max(1, Math.Min(10, req.PadLength)),
            FirstNum         = req.FirstNum > 0 ? req.FirstNum : 1,
            NextNum          = req.FirstNum > 0 ? req.FirstNum : 1,
            LastNum          = req.LastNum,
            Remarks          = req.Remarks,
            CreatedAt        = DateTime.UtcNow,
            InActive         = false,
        };

        _repository.Add(entity);
        _repository.Commit();
        return Ok(new { message = "Document Number Range created", id = entity.Id, preview = DocumentNumberRangeRepository.FormatDocNo(entity, entity.NextNum) });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] DocumentNumberRangeUpdateRequest req)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        // Validate: NextNum cannot go backward below used numbers
        int used = entity.NextNum - entity.FirstNum;
        if (req.NextNum < entity.FirstNum)
            return BadRequest($"Next Number cannot be less than First Number ({entity.FirstNum})");

        if (req.IsDefault) ClearDefault(entity.DocumentType);

        entity.SeriesName    = req.SeriesName;
        entity.IsDefault     = req.IsDefault;
        entity.IsLocked      = req.IsLocked;
        entity.Prefix        = req.Prefix.ToUpper().Trim();
        entity.IncludeYear   = req.IncludeYear;
        entity.PadLength     = Math.Max(1, Math.Min(10, req.PadLength));
        entity.FirstNum      = req.FirstNum;
        entity.NextNum       = req.NextNum;
        entity.LastNum       = req.LastNum;
        entity.Remarks       = req.Remarks;
        entity.UpdatedAt     = DateTime.UtcNow;

        _repository.Update(entity);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // LOCK / UNLOCK
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Lock")]
    public IActionResult Lock(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsLocked  = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"Series '{entity.SeriesName}' locked" });
    }

    [HttpPost("{id:int}/Unlock")]
    public IActionResult Unlock(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        entity.IsLocked  = false;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"Series '{entity.SeriesName}' unlocked" });
    }

    // ═══════════════════════════════════════════════════════════════
    // SET DEFAULT
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/SetDefault")]
    public IActionResult SetDefault(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        ClearDefault(entity.DocumentType);
        entity.IsDefault = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(new { message = $"'{entity.SeriesName}' set as default for {entity.DocumentType}" });
    }

    // ═══════════════════════════════════════════════════════════════
    // SEED — creates default ranges for ALL document types
    // POST /DocumentNumberRange/Seed
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("Seed")]
    public IActionResult Seed()
    {
        var created = new List<string>();
        foreach (var docType in DocTypes.All)
        {
            bool exists = _repository.GetAll()
                .Any(d => d.DocumentType == docType.Key && d.SeriesName == "Primary");
            if (exists) continue;

            _repository.Add(new DocumentNumberRange
            {
                DocumentType     = docType.Key,
                DocumentTypeCode = docType.DefaultPrefix,
                SeriesName       = "Primary",
                IsDefault        = true,
                IsLocked         = false,
                Prefix           = docType.DefaultPrefix,
                IncludeYear      = true,
                PadLength        = 5,
                FirstNum         = 1,
                NextNum          = 1,
                LastNum          = null,
                CreatedAt        = DateTime.UtcNow,
                InActive         = false,
            });
            created.Add(docType.Key);
        }
        _repository.Commit();
        return Ok(new { message = $"Seeded {created.Count} document number ranges", created });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (entity.IsDefault) return BadRequest("Cannot delete the default series. Set another as default first.");
        if (entity.NextNum > entity.FirstNum) return BadRequest("Cannot delete a series that has already been used.");

        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private void ClearDefault(string documentType)
    {
        foreach (var d in _repository.GetAll().Where(x => x.DocumentType == documentType && x.IsDefault))
            d.IsDefault = false;
        _repository.Commit();
    }

    private static DocumentNumberRangeResponse MapResponse(DocumentNumberRange d)
    {
        var info     = DocTypes.Find(d.DocumentType);
        var preview  = DocumentNumberRangeRepository.FormatDocNo(d, d.NextNum);
        return new DocumentNumberRangeResponse
        {
            Id               = d.Id,
            DocumentType     = d.DocumentType,
            DocumentTypeCode = d.DocumentTypeCode,
            DocumentLabel    = info?.Label    ?? d.DocumentType,
            Module           = info?.Module   ?? "—",
            SeriesName       = d.SeriesName,
            IsDefault        = d.IsDefault,
            IsLocked         = d.IsLocked,
            Prefix           = d.Prefix,
            IncludeYear      = d.IncludeYear,
            PadLength        = d.PadLength,
            FirstNum         = d.FirstNum,
            NextNum          = d.NextNum,
            LastNum          = d.LastNum,
            Remarks          = d.Remarks,
            PreviewFormat    = preview,
            UsedCount        = d.NextNum - d.FirstNum,
            CreatedAt        = d.CreatedAt,
            UpdatedAt        = d.UpdatedAt,
        };
    }
}