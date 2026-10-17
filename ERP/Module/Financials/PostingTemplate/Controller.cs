using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using Coa = FarmingApi.Modules.Financials.ChartOfAccounts.ChartOfAccounts;

namespace FarmingApi.Modules.Financials.PostingTemplate;

// ═══════════════════════════════════════════════════════════════════
// POSTING TEMPLATE  (route: /PostingTemplate)
// GET    /PostingTemplate
// GET    /PostingTemplate/{id}
// POST   /PostingTemplate
// PUT    /PostingTemplate/{id}
// DELETE /PostingTemplate/{id}
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class PostingTemplateController : ControllerBase
{
    private readonly IPostingTemplateRepository _repository;
    private readonly IMapper     _mapper;
    private readonly MyDbContext _db;

    public PostingTemplateController(IPostingTemplateRepository repository, IMapper mapper, MyDbContext db)
    {
        _repository = repository;
        _mapper     = mapper;
        _db         = db;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repository.GetAll().Include(x => x.Lines).OrderBy(x => x.Code).ToList();
        return Ok(_mapper.Map<List<PostingTemplateResponse>>(list));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PostingTemplateResponse>(e));
    }

    [HttpPost]
    public IActionResult Create([FromBody] PostingTemplateRequest dto)
    {
        var (error, accounts) = Validate(dto);
        if (error != null) return BadRequest(error);

        var entity = new PostingTemplate
        {
            Code        = NextCode(),
            Description = dto.Description.Trim(),
            Remarks     = dto.Remarks,
            IsActive    = dto.IsActive,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };
        AddLines(entity, dto, accounts!);

        _repository.Add(entity);
        _repository.Commit();
        return Ok(_mapper.Map<PostingTemplateResponse>(ReloadWithLines(entity.Id)));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PostingTemplateRequest dto)
    {
        var entity = _repository.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (entity == null) return NotFound();

        var (error, accounts) = Validate(dto);
        if (error != null) return BadRequest(error);

        entity.Lines.Clear();
        entity.Description = dto.Description.Trim();
        entity.Remarks     = dto.Remarks;
        entity.IsActive    = dto.IsActive;
        entity.UpdatedAt   = DateTime.UtcNow;
        AddLines(entity, dto, accounts!);

        _repository.Commit();
        return Ok(_mapper.Map<PostingTemplateResponse>(ReloadWithLines(entity.Id)));
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

    // A template is a skeleton for a balanced journal entry: every line names a
    // postable account and a side, each side's specified percentages distribute
    // a base amount, and both sides must be present so an applied base nets to
    // a balanced entry. A 0% line is filled in by hand when the template is used.
    private (string? error, Dictionary<string, Coa>? accounts) Validate(PostingTemplateRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Description)) return ("Description is required.", null);
        if (dto.Lines == null || dto.Lines.Count < 2)  return ("A posting template needs at least two lines.", null);

        foreach (var l in dto.Lines)
        {
            if (string.IsNullOrWhiteSpace(l.AccountCode)) return ("Every line needs a G/L account.", null);
            if (l.Side != "D" && l.Side != "C")           return ("Every line must be Debit or Credit.", null);
            if (l.Percentage < 0 || l.Percentage > 100)   return ("Each percentage must be between 0 and 100.", null);
        }

        var codes = dto.Lines.Select(l => l.AccountCode).Distinct().ToList();
        var accounts = _db.Set<Coa>().Where(a => codes.Contains(a.AcctCode))
            .ToDictionary(a => a.AcctCode, a => a);

        var missing = codes.Where(c => !accounts.ContainsKey(c)).ToList();
        if (missing.Count > 0) return ($"G/L account(s) not found: {string.Join(", ", missing)}.", null);

        var notPostable = dto.Lines
            .Where(l => accounts.TryGetValue(l.AccountCode, out var a)
                        && (a.IsGroup || !a.AllowPosting || a.BlockManualPosting || a.Frozen))
            .Select(l => l.AccountCode).Distinct().ToList();
        if (notPostable.Count > 0)
            return ($"These accounts don't allow manual posting: {string.Join(", ", notPostable)}.", null);

        if (!dto.Lines.Any(l => l.Side == "D")) return ("Add at least one Debit line.", null);
        if (!dto.Lines.Any(l => l.Side == "C")) return ("Add at least one Credit line.", null);

        foreach (var side in new[] { "D", "C" })
        {
            var sum = dto.Lines.Where(l => l.Side == side).Sum(l => l.Percentage);
            if (sum != 0 && Math.Abs(sum - 100m) > 0.01m)
            {
                var name = side == "D" ? "Debit" : "Credit";
                return ($"{name} percentages must total 100% (or 0% to key every {name.ToLower()} amount by hand). Currently {sum:0.##}%.", null);
            }
        }

        return (null, accounts);
    }

    private static void AddLines(PostingTemplate entity, PostingTemplateRequest dto, Dictionary<string, Coa> accounts)
    {
        var lineNum = 1;
        foreach (var l in dto.Lines)
        {
            entity.Lines.Add(new PostingTemplateLine
            {
                LineNum         = lineNum++,
                AccountCode     = l.AccountCode,
                AccountName     = accounts.TryGetValue(l.AccountCode, out var a) ? a.AcctName : l.AccountCode,
                Side            = l.Side,
                Percentage      = l.Percentage,
                LineDescription = l.LineDescription,
            });
        }
    }

    private string NextCode()
    {
        var highest = _repository.GetAll().Select(t => t.Code).ToList()
            .Select(c => int.TryParse(c.Replace("PT-", ""), out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"PT-{highest + 1:D4}";
    }

    private PostingTemplate ReloadWithLines(int id) =>
        _repository.GetAll().Include(x => x.Lines).First(x => x.Id == id);
}
