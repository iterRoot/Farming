using System.Text.RegularExpressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommissionGroupEntity = FarmingApi.Modules.Sales.CommissionGroup.CommissionGroup;
using CommissionCalc = FarmingApi.Modules.Sales.CommissionGroup.CommissionCalc;
using BPEntity   = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using ARInvoice  = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;
using ARCredit   = FarmingApi.Modules.SaleAR.ARCreditNote.ARCreditNote;

namespace FarmingApi.Modules.Sales.SaleEmployeeBuyer;

// ═══════════════════════════════════════════════════════════════
// SALES EMPLOYEES / BUYERS
// GET    /SaleEmployeeBuyer[?type=]      → all (type: SalesEmployee | Buyer, "Both" always included)
// GET    /SaleEmployeeBuyer/SalesEmployees | /Buyers   → active ones eligible for that role
// GET    /SaleEmployeeBuyer/{id} | /ByCode/{code}
// POST/PUT/DELETE                        → validated; delete refused while anything uses the person
// GET    /SaleEmployeeBuyer/Commission?from=&to=[&code=]  → commission earned on AR invoices less credit notes
//
// Sales documents carry a sales employee and purchase documents a buyer
// (validated centrally by EmployeeTagFilter); business partners carry a default.
// ═══════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class SaleEmployeeBuyerController : ControllerBase
{
    private static readonly string[] Types = { "SalesEmployee", "Buyer", "Both" };
    private static readonly Regex CodePattern = new("^[A-Z0-9][A-Z0-9_-]{0,49}$", RegexOptions.Compiled);
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly ISaleEmployeeBuyerRepository _repo;
    private readonly IMapper                      _mapper;
    private readonly MyDbContext                  _db;

    public SaleEmployeeBuyerController(ISaleEmployeeBuyerRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo   = repo;
        _mapper = mapper;
        _db     = db;
    }

    // ── reads ──────────────────────────────────────────────────
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? type = null)
    {
        var query = _repo.GetAll()
            .Where(x => type == null || x.EmployeeType == type || x.EmployeeType == "Both")
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(query));
    }

    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(e));
    }

    [HttpGet("ByCode/{code}")]
    public IActionResult GetByCode(string code)
    {
        var c = code.Trim().ToUpper();
        var e = _repo.GetSingle(x => x.Code == c);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(e));
    }

    [HttpGet("SalesEmployees")]
    public IActionResult GetSalesEmployees()
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.EmployeeType == "SalesEmployee" || x.EmployeeType == "Both"))
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(list));
    }

    [HttpGet("Buyers")]
    public IActionResult GetBuyers()
    {
        var list = _repo.GetAll()
            .Where(x => x.IsActive && (x.EmployeeType == "Buyer" || x.EmployeeType == "Both"))
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToList();
        return Ok(_mapper.Map<List<SaleEmployeeBuyerResponse>>(list));
    }

    // ── validation ─────────────────────────────────────────────
    private string? Validate(SaleEmployeeBuyerRequest dto, int? ignoreId)
    {
        var code = (dto.Code ?? "").Trim().ToUpper();
        if (!CodePattern.IsMatch(code)) return "Code must be 1–50 letters, digits, '-' or '_' (e.g. SE-001).";
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName)) return "First and last name are required.";
        if (!Types.Contains(dto.EmployeeType)) return $"Type must be one of: {string.Join(", ", Types)}.";
        if (!string.IsNullOrWhiteSpace(dto.Email) && !EmailPattern.IsMatch(dto.Email.Trim())) return "Email address isn't valid.";
        if (_repo.GetAll().Any(x => x.Code == code && (ignoreId == null || x.Id != ignoreId)))
            return $"Employee code '{code}' already exists.";

        if (dto.CommissionGroupId != null)
        {
            var g = _db.Set<CommissionGroupEntity>().FirstOrDefault(x => x.Id == dto.CommissionGroupId);
            if (g == null) return "The commission group doesn't exist.";
            if (!g.IsActive) return $"Commission group '{g.Code}' is inactive.";
            dto.CommissionGroupCode = g.Code;      // canonical, whatever the client sent
            dto.CommissionGroupName = g.Name;
        }
        else { dto.CommissionGroupCode = null; dto.CommissionGroupName = null; }
        return null;
    }

    private static string DocLabel(string table) => table switch
    {
        "KASQ" => "sales quotation(s)", "KASO" => "sales order(s)", "KADV" => "delivery note(s)", "KARI" => "AR invoice(s)",
        "KACN" => "AR credit note(s)", "KPPQ" => "purchase quotation(s)", "KPPO" => "purchase order(s)",
        "KPGR" => "goods receipt PO(s)", "KPAI" => "AP invoice(s)", _ => $"{table} document(s)",
    };

    // Everything that points at this code: documents (catalog-driven), business partners.
    private string? UsageOf(string code)
    {
        var uses = new List<string>();
        foreach (var et in _db.Model.GetEntityTypes())
        {
            var col = et.FindProperty("SalesEmployeeCode") ?? et.FindProperty("BuyerCode");
            var table = et.GetTableName();
            if (col == null || table == null) continue;
            var colName = col.GetColumnName();
            var n = _db.Database.SqlQueryRaw<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"{table}\" WHERE \"{colName}\" = {{0}}", code).AsEnumerable().FirstOrDefault();
            if (n > 0) uses.Add($"{n} {DocLabel(table)}");
        }
        var bps = _db.Set<BPEntity>().Count(b => b.SalesEmployee == code);
        if (bps > 0) uses.Add($"{bps} business partner(s)");
        return uses.Count == 0 ? null : string.Join(", ", uses);
    }

    // ── writes ─────────────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] SaleEmployeeBuyerRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var err = Validate(dto, null);
        if (err != null) return BadRequest(err);

        var entity       = _mapper.Map<SaleEmployeeBuyer>(dto);
        entity.Code      = dto.Code.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(entity));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] SaleEmployeeBuyerRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var err = Validate(dto, id);
        if (err != null) return BadRequest(err);

        var code = dto.Code.Trim().ToUpper();
        if (code != entity.Code)
        {
            var use = UsageOf(entity.Code);
            if (use != null) return BadRequest($"The code can't change: it is used on {use}.");
        }
        // A role can't be taken away while documents of that kind still carry the person.
        if (dto.EmployeeType != "Both" && dto.EmployeeType != entity.EmployeeType)
        {
            var lost = dto.EmployeeType == "Buyer" ? "SalesEmployeeCode" : "BuyerCode";
            foreach (var et in _db.Model.GetEntityTypes())
            {
                var col = et.FindProperty(lost); var table = et.GetTableName();
                if (col == null || table == null) continue;
                var n = _db.Database.SqlQueryRaw<int>($"SELECT COUNT(*)::int AS \"Value\" FROM \"{table}\" WHERE \"{col.GetColumnName()}\" = {{0}}", entity.Code).AsEnumerable().FirstOrDefault();
                if (n > 0) return BadRequest($"Can't become a {dto.EmployeeType} only: {n} {DocLabel(table)} still carry this person as a {(lost == "BuyerCode" ? "buyer" : "sales employee")}.");
            }
        }

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<SaleEmployeeBuyerResponse>(entity));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        var use = UsageOf(entity.Code);
        if (use != null) return BadRequest($"'{entity.Code}' can't be deleted: it is used on {use}. Mark the person inactive instead.");
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── commission ─────────────────────────────────────────────
    // Each posted (Open/Closed) AR invoice tagged with the sales employee earns commission
    // on its net amount (total less tax) under the employee's commission group; each posted
    // credit note takes the same calculation back. Dates are posting dates, both inclusive.
    [HttpGet("Commission")]
    public IActionResult Commission([FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? code = null)
    {
        if (to.Date < from.Date) return BadRequest("The end date is before the start date.");
        var f = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
        var t = DateTime.SpecifyKind(to.Date.AddDays(1), DateTimeKind.Utc);
        var c = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpper();

        var people = _repo.GetAll().Where(e => c == null || e.Code == c).OrderBy(e => e.Code).ToList();
        if (c != null && people.Count == 0) return NotFound($"Sales employee '{c}' not found.");
        var groups = _db.Set<CommissionGroupEntity>().Include(g => g.Tiers).ToDictionary(g => g.Id);

        var invoices = _db.Set<ARInvoice>().AsNoTracking()
            .Where(i => i.SalesEmployeeCode != null && (c == null || i.SalesEmployeeCode == c)
                        && (i.Status == "O" || i.Status == "C") && i.PostingDate >= f && i.PostingDate < t).ToList();
        var credits = _db.Set<ARCredit>().AsNoTracking()
            .Where(i => i.SalesEmployeeCode != null && (c == null || i.SalesEmployeeCode == c)
                        && (i.Status == "O" || i.Status == "C") && i.PostingDate >= f && i.PostingDate < t).ToList();

        var result = new List<object>();
        foreach (var p in people)
        {
            groups.TryGetValue(p.CommissionGroupId ?? 0, out var g);
            var docs = new List<object>();
            decimal sales = 0, credited = 0, commission = 0;
            foreach (var i in invoices.Where(x => x.SalesEmployeeCode == p.Code).OrderBy(x => x.PostingDate))
            {
                var net = i.Total - i.TaxAmount;
                var com = g == null ? 0m : CommissionCalc.Compute(g, net);
                sales += net; commission += com;
                docs.Add(new { type = "AR Invoice", i.DocNum, date = i.PostingDate, net, commission = com });
            }
            foreach (var cn in credits.Where(x => x.SalesEmployeeCode == p.Code).OrderBy(x => x.PostingDate))
            {
                var net = cn.Total - cn.TaxAmount;
                var com = g == null ? 0m : CommissionCalc.Compute(g, net);
                credited += net; commission -= com;
                docs.Add(new { type = "Credit Note", cn.DocNum, date = cn.PostingDate, net = -net, commission = -com });
            }
            if (c == null && docs.Count == 0) continue;
            result.Add(new
            {
                code = p.Code, name = $"{p.FirstName} {p.LastName}".Trim(),
                commissionGroup = g?.Code, commissionRule = g == null ? null : $"{g.CommissionType} {g.CommissionRate:0.####}",
                sales, credited, netSales = sales - credited, commission,
                note = g == null ? "No commission group assigned — no commission is calculated." : null,
                documents = docs,
            });
        }
        return Ok(new { from = f, to = to.Date, employees = result });
    }
}
