using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.CRM.KnowledgeBase;

// ═══════════════════════════════════════════════════════════════════
// KNOWLEDGE BASE  (route: /KnowledgeBase)
// GET /KnowledgeBase?status=Published&q=text
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class KnowledgeBaseController : ControllerBase
{
    private readonly IKnowledgeBaseArticleRepository _repository;
    private readonly IMapper _mapper;

    public KnowledgeBaseController(IKnowledgeBaseArticleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper     = mapper;
    }

    [HttpGet]
    public IActionResult GetAll([FromQuery] string? status = null, [FromQuery] string? q = null)
    {
        var list = _repository.GetAll().ToList();
        if (!string.IsNullOrWhiteSpace(status))
            list = list.Where(x => x.Status == status).ToList();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            list = list.Where(x =>
                (x.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Symptom?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.Tags?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        }
        var res = _mapper.Map<List<KnowledgeBaseArticleResponse>>(
            list.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToList());
        return Ok(res);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        // reading an article bumps its view count
        e.ViewCount += 1;
        _repository.Update(e);
        _repository.Commit();
        return Ok(_mapper.Map<KnowledgeBaseArticleResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] KnowledgeBaseArticleRequest dto)
    {
        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new KnowledgeBaseArticle
        {
            Code      = NextCode(),
            Title     = dto.Title.Trim(),
            Category  = dto.Category,
            ItemCode  = dto.ItemCode,
            Symptom   = dto.Symptom,
            Cause     = dto.Cause,
            Solution  = dto.Solution.Trim(),
            Status    = NormalizeStatus(dto.Status),
            Tags      = dto.Tags,
            CreatedAt = DateTime.UtcNow,
            InActive  = false,
        };
        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<KnowledgeBaseArticleResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] KnowledgeBaseArticleRequest dto)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();

        var error = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.Title     = dto.Title.Trim();
        entity.Category  = dto.Category;
        entity.ItemCode  = dto.ItemCode;
        entity.Symptom   = dto.Symptom;
        entity.Cause     = dto.Cause;
        entity.Solution  = dto.Solution.Trim();
        entity.Status    = NormalizeStatus(dto.Status);
        entity.Tags      = dto.Tags;
        entity.UpdatedAt = DateTime.UtcNow;
        _repository.Update(entity);
        _repository.Commit();
        return Ok(_mapper.Map<KnowledgeBaseArticleResponse>(entity));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repository.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repository.Remove(entity);
        _repository.Commit();
        return NoContent();
    }

    private static string? Validate(KnowledgeBaseArticleRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))    return "Title is required.";
        if (string.IsNullOrWhiteSpace(dto.Solution)) return "A solution is required.";
        return null;
    }

    private static string NormalizeStatus(string? s) =>
        s is "Draft" or "Published" or "Archived" ? s : "Draft";

    private string NextCode()
    {
        var highest = _repository.GetAll().Select(x => x.Code).ToList()
            .Select(c => int.TryParse(c.Replace("KB-", ""), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"KB-{highest + 1:D4}";
    }
}
