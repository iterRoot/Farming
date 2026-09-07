using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using SO = FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder;

namespace FarmingApi.Modules.InventoryManagement.PickAndPack;

// ══════════════════════════════════════════════════════════════════
// PICK & PACK  (route: /pickpack)
// GET    /pickpack/criteria        → all saved criteria
// GET    /pickpack/{id}            → one criteria + generated pick lines
// POST   /pickpack/criteria        → save criteria (no generation)
// POST   /pickpack/generate        → save + generate a pick list, returns { id }
// POST   /pickpack/{id}/run        → regenerate the pick list for a criteria
// DELETE /pickpack/criteria/{id}   → delete
// ══════════════════════════════════════════════════════════════════
[ApiController]
[Route("pickpack")]
public class PickPackController : ControllerBase
{
    private readonly IPickPackRepository _repo;
    private readonly MyDbContext _db;
    private readonly IMapper _mapper;

    public PickPackController(IPickPackRepository repo, MyDbContext db, IMapper mapper)
    {
        _repo = repo; _db = db; _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet("criteria")]
    public IActionResult List()
    {
        var list = _repo.GetAll().Include(x => x.Lines)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ToList();
        return Ok(_mapper.Map<List<PickPackCriteriaResponse>>(list));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();
        var res = _mapper.Map<PickPackCriteriaResponse>(e);
        res.Lines = _mapper.Map<List<PickPackLineDto>>(e.Lines.OrderBy(l => l.LineNum));
        return Ok(res);
    }

    [AllowAnonymous]
    [HttpPost("criteria")]
    public IActionResult Save([FromBody] PickPackCriteriaRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CriteriaName))
            return BadRequest(new { message = "Criteria name is required" });

        var e = _mapper.Map<PickPackCriteria>(dto);
        e.Status = string.IsNullOrWhiteSpace(dto.Status) ? "Open" : dto.Status!;
        e.CreatedAt = DateTime.UtcNow;
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { message = "Selection criteria saved.", id = e.Id });
    }

    [AllowAnonymous]
    [HttpPost("generate")]
    public IActionResult Generate([FromBody] PickPackCriteriaRequest dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CriteriaName))
            return BadRequest(new { message = "Criteria name is required" });

        var e = _mapper.Map<PickPackCriteria>(dto);
        e.CreatedAt = DateTime.UtcNow;
        BuildLines(e);
        e.Status = dto.ReleaseManually ? "Open" : "Released";

        _repo.Add(e);
        _repo.Commit();
        return Ok(new { message = $"Generated {e.Lines.Count} pick line(s).", id = e.Id, lines = e.Lines.Count });
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/run")]
    public IActionResult Run(int id)
    {
        var e = _repo.GetAll().Include(x => x.Lines).FirstOrDefault(x => x.Id == id);
        if (e == null) return NotFound();

        _db.RemoveRange(e.Lines);
        e.Lines.Clear();
        BuildLines(e);
        e.Status = e.ReleaseManually ? "Open" : "Released";
        e.UpdatedAt = DateTime.UtcNow;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { message = $"Regenerated {e.Lines.Count} pick line(s).", id = e.Id, lines = e.Lines.Count });
    }

    [AllowAnonymous]
    [HttpDelete("criteria/{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    // ── Build pick lines from the selected open source documents ──
    private void BuildLines(PickPackCriteria e)
    {
        var lines = new List<PickPackLine>();

        if (e.ManageSalesOrders)
        {
            var orders = _db.Set<SO>()
                .Include(o => o.Items)
                .Include(o => o.Customer)
                .Where(o => o.Status == "O")
                .ToList();

            foreach (var o in orders)
                foreach (var l in o.Items)
                    lines.Add(new PickPackLine
                    {
                        BaseDocType  = "SaleOrder",
                        BaseDocEntry = o.Id,
                        BaseDocNum   = o.DocNum,
                        CustomerName = o.Customer?.CardName,
                        DueDate      = o.DeliveryDate,
                        ItemCode     = l.ItemCode ?? "",
                        ItemName     = l.ItemName,
                        RequiredQty  = l.Quantity,
                        Status       = "Open",
                        CreatedAt    = DateTime.UtcNow,
                    });
        }

        // Sort per criteria
        IEnumerable<PickPackLine> sorted = e.SortBy switch
        {
            "Customer" => lines.OrderBy(x => x.CustomerName),
            "Item"     => lines.OrderBy(x => x.ItemCode),
            _           => lines.OrderBy(x => x.DueDate ?? DateTime.MaxValue), // Delivery/Due Date
        };

        int n = 1;
        foreach (var l in sorted) { l.LineNum = n++; e.Lines.Add(l); }
    }
}
