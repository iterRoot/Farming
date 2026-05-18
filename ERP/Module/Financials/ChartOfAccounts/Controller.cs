using AutoMapper;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.ChartOfAccounts;

public class ChartOfAccountsController : MyController
{
    private readonly IMapper _mapper;
    private readonly IChartOfAccountsRepository _repository;

    public ChartOfAccountsController(IChartOfAccountsRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    // ── GET TYPES (Used by React side-tabs) ───────────────────────
    [AllowAnonymous]
    [HttpGet("Types")]
    public IActionResult GetAccountTypes()
    {
        var types = new List<AccountTypeResponse>
        {
            new() { Code = "A", Label = "Assets" },
            new() { Code = "L", Label = "Liabilities" },
            new() { Code = "E", Label = "Equity" },
            new() { Code = "I", Label = "Revenues" },
            new() { Code = "X", Label = "Expenses" }
        };
        return Ok(types);
    }

    // ── GET ALL ───────────────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var accounts = _repository.GetAll().OrderBy(a => a.AcctCode).ToList();
        return Ok(_mapper.Map<List<ChartOfAccountsResponse>>(accounts));
    }

    // ── CREATE ────────────────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ChartOfAccountsCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var fatherNum = string.IsNullOrWhiteSpace(request.FatherNum) || 
                        request.FatherNum.Trim().ToLower() == "null" 
                        ? null 
                        : request.FatherNum.Trim();

        if (_repository.GetAll().Any(a => a.AcctCode == request.AcctCode.ToUpper()))
            return BadRequest($"Account code '{request.AcctCode}' already exists");

        // Hierarchy Level Restrictions (Starting from Level 1)
        if (request.Level < 1) 
            return BadRequest("Account hierarchy must start at Level 1.");

        if (request.Level == 1 && fatherNum != null)
            return BadRequest("Root accounts (Level 1) cannot have a parent account.");

        if (request.Level > 1)
        {
            if (string.IsNullOrEmpty(fatherNum))
                return BadRequest($"Level {request.Level} accounts require a parent account");

            var parent = _repository.GetAll().FirstOrDefault(a => a.AcctCode == fatherNum);
            if (parent == null) return BadRequest($"Parent account '{fatherNum}' not found");
            if (!parent.IsGroup) return BadRequest($"Parent account '{fatherNum}' must be a Group account");
        }

        var entity = _mapper.Map<ChartOfAccounts>(request);
        entity.FatherNum = fatherNum;
        entity.AcctCode = entity.AcctCode.Trim().ToUpper();
        entity.CreatedAt = DateTime.UtcNow;

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

        if (account.IsGroup != request.IsGroup && _repository.GetAll().Any(a => a.FatherNum == account.AcctCode))
        {
            return BadRequest("Cannot change group status when account has child accounts");
        }

        _mapper.Map(request, account);
        account.UpdatedAt = DateTime.UtcNow;

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
            return BadRequest("Cannot delete an account with existing transactions");

        if (_repository.GetAll().Any(a => a.FatherNum == account.AcctCode))
            return BadRequest("Cannot delete an account that has child accounts");

        _repository.Remove(account);
        _repository.Commit();

        return NoContent();
    }
}