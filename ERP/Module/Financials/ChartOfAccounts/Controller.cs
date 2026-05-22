using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccountsController : MyController
{
    private readonly IMapper                      _mapper;
    private readonly IChartOfAccountsRepository  _repository;

    // ✅ All 8 SAP B1 types — matches right-panel tabs in screenshot
    private static readonly string[] ValidTypes = { "A", "L", "E", "I", "C", "X", "N", "T" };

    public ChartOfAccountsController(
        IChartOfAccountsRepository repository,
        IMapper mapper)
    {
        _mapper     = mapper;
        _repository = repository;
    }

    // ── GET TYPES ────────────────────────────────────────────────
    // ✅ Returns all 8 SAP B1 account types for the right-panel tabs
    [AllowAnonymous]
    [HttpGet("Types")]
    public IActionResult GetAccountTypes()
    {
        var types = new List<AccountTypeResponse>
        {
            new() { Code = "A", Label = "Assets" },
            new() { Code = "L", Label = "Liabilities" },
            new() { Code = "E", Label = "Capital and Reserves" },
            new() { Code = "I", Label = "Turnover" },
            new() { Code = "C", Label = "Cost of Sales" },
            new() { Code = "X", Label = "Operating Costs" },
            new() { Code = "N", Label = "Non-operating Income/Exp" },
            new() { Code = "T", Label = "Taxation & Extraordinary" },
        };
        return Ok(types);
    }

    // ── GET ALL (flat list) ───────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var accounts = _repository.GetAll()
            .OrderBy(a => a.AcctCode)
            .ToList();
        return Ok(_mapper.Map<List<ChartOfAccountsResponse>>(accounts));
    }

    // ── GET BY ID ─────────────────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var account = _repository.GetAll()
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .FirstOrDefault(a => a.Id == id);

        if (account == null) return NotFound($"Account {id} not found");
        return Ok(_mapper.Map<ChartOfAccountsResponse>(account));
    }

    // ── GET BY CODE ───────────────────────────────────────────────
    [HttpGet("Code/{code}")]
    public IActionResult GetByCode(string code)
    {
        var account = _repository.GetAll()
            .FirstOrDefault(a => a.AcctCode == code.ToUpper());

        if (account == null) return NotFound($"Account '{code}' not found");
        return Ok(_mapper.Map<ChartOfAccountsResponse>(account));
    }

    // ── GET TREE ──────────────────────────────────────────────────
    [HttpGet("Tree")]
    public IActionResult GetTree([FromQuery] string? acctType = null)
    {
        var query = _repository.GetAll().AsQueryable();
        if (!string.IsNullOrEmpty(acctType))
            query = query.Where(a => a.AcctType == acctType);

        var all  = query.OrderBy(a => a.AcctCode).ToList();
        var tree = BuildTree(all, null);
        return Ok(tree);
    }

    // ── GET POSTING ACCOUNTS (for Journal Entry dropdowns) ────────
    [HttpGet("Posting")]
    public IActionResult GetPostingAccounts([FromQuery] string? acctType = null)
    {
        var query = _repository.GetAll()
            .Where(a => !a.IsGroup
                     && a.AllowPosting
                     && !a.BlockManualPosting
                     && a.IsActive
                     && !a.Frozen);

        if (!string.IsNullOrEmpty(acctType))
            query = query.Where(a => a.AcctType == acctType);

        return Ok(_mapper.Map<List<ChartOfAccountsResponse>>(
            query.OrderBy(a => a.AcctCode).ToList()
        ));
    }

    // ── CREATE ────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ChartOfAccountsCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // ── Validate type (all 8 SAP B1 types) ───────────────────
        if (!ValidTypes.Contains(request.AcctType))
            return BadRequest($"Invalid account type '{request.AcctType}'. Valid: {string.Join(", ", ValidTypes)}");

        // ── Sanitize fatherNum ("null" string → actual null) ──────
        var fatherNum = string.IsNullOrWhiteSpace(request.FatherNum) ||
                        request.FatherNum.Trim().ToLower() == "null"
                        ? null
                        : request.FatherNum.Trim();

        // ── Validate level — SAP starts at Level 1 ────────────────
        if (request.Level < 1)
            return BadRequest("Account hierarchy must start at Level 1. Level 1 = Root.");

        if (request.Level == 1 && fatherNum != null)
            return BadRequest("Root accounts (Level 1) cannot have a parent account.");

        if (request.Level > 1)
        {
            if (string.IsNullOrEmpty(fatherNum))
                return BadRequest($"Level {request.Level} accounts require a parent account.");

            var parent = _repository.GetAll().FirstOrDefault(a => a.AcctCode == fatherNum);
            if (parent == null)
                return BadRequest($"Parent account '{fatherNum}' not found.");
            if (!parent.IsGroup)
                return BadRequest($"Parent account '{fatherNum}' must be a Title/Group account.");
        }

        // ── Duplicate check ───────────────────────────────────────
        var acctCode = request.AcctCode.Trim().ToUpper();
        if (_repository.GetAll().Any(a => a.AcctCode == acctCode))
            return BadRequest($"Account code '{acctCode}' already exists.");

        // ── Map and set system fields ─────────────────────────────
        var entity          = _mapper.Map<ChartOfAccounts>(request);
        entity.AcctCode     = acctCode;
        entity.FatherNum    = fatherNum;
        entity.DataSource   = "O";
        entity.VersionNum   = 1;
        entity.CreatedAt    = DateTime.UtcNow;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Account created successfully", id = entity.Id, acctCode = entity.AcctCode });
    }

    // ── UPDATE ────────────────────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ChartOfAccountsUpdateRequest request)
    {
        var account = _repository.GetSingle(e => e.Id == id);
        if (account == null) return NotFound($"Account {id} not found");

        // Validate type
        if (!ValidTypes.Contains(request.AcctType))
            return BadRequest($"Invalid account type '{request.AcctType}'.");

        // Block group change if has children
        if (account.IsGroup != request.IsGroup)
        {
            var hasChildren = _repository.GetAll().Any(a => a.FatherNum == account.AcctCode);
            if (hasChildren)
                return BadRequest("Cannot change group status when account has child accounts.");
        }

        _mapper.Map(request, account);
        account.UpdatedAt  = DateTime.UtcNow;
        account.VersionNum += 1;

        _repository.Update(account);
        _repository.Commit();

        return NoContent();
    }

    // ── DELETE ────────────────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var account = _repository.GetSingle(e => e.Id == id);
        if (account == null) return NotFound($"Account {id} not found");

        if (account.Balance != 0 || account.DebitBalance != 0 || account.CreditBalance != 0)
            return BadRequest("Cannot delete an account with existing transactions.");

        if (_repository.GetAll().Any(a => a.FatherNum == account.AcctCode))
            return BadRequest("Cannot delete an account that has child accounts.");

        _repository.Remove(account);
        _repository.Commit();

        return NoContent();
    }

    // ── HELPER: Build recursive tree ──────────────────────────────
    private List<ChartOfAccountsTreeResponse> BuildTree(
        List<ChartOfAccounts> all, string? parentCode)
    {
        return all
            .Where(a => (a.FatherNum ?? null) == parentCode)
            .Select(a => new ChartOfAccountsTreeResponse
            {
                Id           = a.Id,
                AcctCode     = a.AcctCode,
                AcctName     = a.AcctName,
                Level        = a.Level,
                FatherNum    = a.FatherNum,
                AcctType     = a.AcctType,
                IsGroup      = a.IsGroup,
                IsActive     = a.IsActive,
                Balance      = a.Balance,
                AllowPosting = a.AllowPosting,
                Children     = BuildTree(all, a.AcctCode),
            })
            .ToList();
    }
}