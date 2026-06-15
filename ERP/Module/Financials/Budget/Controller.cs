using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.ChartOfAccounts;

namespace FarmingApi.Modules.Financials.Budget;

public class BudgetController : MyController
{
    private readonly IMapper                   _mapper;
    private readonly IBudgetRepository         _repository;
    private readonly IChartOfAccountsRepository _coaRepository;

    public BudgetController(
        IBudgetRepository         repository,
        IChartOfAccountsRepository coaRepository,
        IMapper mapper)
    {
        _mapper        = mapper;
        _repository    = repository;
        _coaRepository = coaRepository;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets([FromQuery] int? year = null, [FromQuery] string? status = null)
    {
        var query = _repository.GetAll().Include(b => b.Lines).AsQueryable();

        if (year.HasValue)   query = query.Where(b => b.FiscalYear == year.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(b => b.Status == status);

        var budgets = query.OrderByDescending(b => b.FiscalYear)
                           .ThenBy(b => b.BudgetNo)
                           .ToList();

        var results = budgets.Select(b => MapWithTotals(b)).ToList();
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var budget = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (budget == null) return NotFound($"Budget {id} not found");
        return Ok(MapWithTotals(budget));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] BudgetCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.BudgetName))
            return BadRequest("Budget Name is required");
        if (request.StartDate >= request.EndDate)
            return BadRequest("End Date must be after Start Date");

        var entity = _mapper.Map<Budget>(request);
        entity.BudgetNo  = GenerateBudgetNumber(request.FiscalYear);
        entity.Status    = "D";   // Draft
        entity.IsActive  = true;
        entity.VersionNum = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;

        int lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<BudgetLine>(lr);
            line.LineNum    = lineNum++;
            line.AcctName   = string.IsNullOrWhiteSpace(lr.AcctName) ? lr.AcctCode : lr.AcctName.Trim();
            line.AcctType   = lr.AcctType ?? "A";
            // Auto-distribute annual to months if months are all zero
            if (lr.Annual > 0 && AllMonthsZero(lr))
                DistributeEvenly(line, lr.Annual);
            line.CreatedAt  = DateTime.UtcNow;
            line.InActive   = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new { message = "Budget created successfully", id = entity.Id, budgetNo = entity.BudgetNo });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BudgetUpdateRequest request)
    {
        var budget = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (budget == null) return NotFound($"Budget {id} not found");
        if (budget.Status == "A")
            return BadRequest("Cannot edit an Approved budget. Close it first or create a new version.");
        if (budget.Status == "C")
            return BadRequest("Cannot edit a Closed budget.");

        _mapper.Map(request, budget);
        budget.UpdatedAt  = DateTime.UtcNow;
        budget.VersionNum += 1;

        budget.Lines.Clear();
        int lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<BudgetLine>(lr);
            line.LineNum   = lineNum++;
            line.AcctName  = string.IsNullOrWhiteSpace(lr.AcctName) ? lr.AcctCode : lr.AcctName.Trim();
            line.AcctType  = lr.AcctType ?? "A";
            if (lr.Annual > 0 && AllMonthsZero(lr))
                DistributeEvenly(line, lr.Annual);
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            budget.Lines.Add(line);
        }

        _repository.Update(budget);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // APPROVE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Approve")]
    public IActionResult Approve(int id)
    {
        var budget = _repository.GetSingle(b => b.Id == id);
        if (budget == null) return NotFound($"Budget {id} not found");
        if (budget.Status != "D")
            return BadRequest($"Only Draft budgets can be approved (current: {budget.Status})");
        if (!budget.Lines.Any())
            return BadRequest("Cannot approve a budget with no lines");

        budget.Status    = "A";
        budget.UpdatedAt = DateTime.UtcNow;
        _repository.Update(budget);
        _repository.Commit();

        return Ok(new { message = "Budget approved successfully", budgetNo = budget.BudgetNo });
    }

    // ═══════════════════════════════════════════════════════════════
    // CLOSE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Close")]
    public IActionResult Close(int id)
    {
        var budget = _repository.GetSingle(b => b.Id == id);
        if (budget == null) return NotFound($"Budget {id} not found");
        if (budget.Status == "C")
            return BadRequest("Budget is already closed");

        budget.Status    = "C";
        budget.UpdatedAt = DateTime.UtcNow;
        _repository.Update(budget);
        _repository.Commit();

        return Ok(new { message = "Budget closed" });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var budget = _repository.GetSingle(b => b.Id == id);
        if (budget == null) return NotFound($"Budget {id} not found");
        if (budget.Status == "A")
            return BadRequest("Cannot delete an Approved budget.");

        budget.DeletedAt = DateTime.UtcNow;
        _repository.Remove(budget);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // BUDGET VS ACTUAL
    // Compares budget lines against current KCOA account balances
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}/VsActual")]
    public IActionResult GetVsActual(int id)
    {
        var budget = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (budget == null) return NotFound($"Budget {id} not found");

        var accounts = _coaRepository.GetAll()
            .Where(a => !a.IsGroup && a.IsActive)
            .ToList();

        var result = budget.Lines.Select(line =>
        {
            var acct      = accounts.FirstOrDefault(a => a.AcctCode == line.AcctCode);
            var actual    = acct?.Balance ?? 0m;
            var variance  = actual - line.Annual;
            var varPct    = line.Annual != 0 ? (variance / line.Annual) * 100 : 0;

            // Over budget definition depends on type:
            // Expense (C/X/N/T): actual > budget = over (bad)
            // Income (I):        actual < budget = under (bad)
            var isIncome  = line.AcctType == "I";
            var isOver    = isIncome ? actual < line.Annual : actual > line.Annual;

            return new BudgetVsActualResponse
            {
                AcctCode    = line.AcctCode,
                AcctName    = line.AcctName,
                AcctType    = line.AcctType,
                BudgetAmt   = line.Annual,
                ActualAmt   = actual,
                Variance    = variance,
                VariancePct = Math.Round(varPct, 1),
                IsOver      = isOver,
            };
        }).ToList();

        return Ok(new
        {
            budgetNo   = budget.BudgetNo,
            budgetName = budget.BudgetName,
            fiscalYear = budget.FiscalYear,
            status     = budget.Status,
            lines      = result,
            summary = new
            {
                totalBudget = result.Sum(r => r.BudgetAmt),
                totalActual = result.Sum(r => r.ActualAmt),
                totalVariance = result.Sum(r => r.Variance),
                overBudgetCount = result.Count(r => r.IsOver),
            }
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private BudgetResponse MapWithTotals(Budget b)
    {
        var response = _mapper.Map<BudgetResponse>(b);
        // Income types: I
        response.TotalIncome  = b.Lines.Where(l => l.AcctType == "I").Sum(l => l.Annual);
        // Expense types: C, X, N, T
        response.TotalExpense = b.Lines.Where(l => "CXNT".Contains(l.AcctType)).Sum(l => l.Annual);
        response.NetBudget    = response.TotalIncome - response.TotalExpense;
        return response;
    }

    private static bool AllMonthsZero(BudgetLineRequest lr) =>
        lr.Jan == 0 && lr.Feb == 0 && lr.Mar == 0 && lr.Apr == 0 &&
        lr.May == 0 && lr.Jun == 0 && lr.Jul == 0 && lr.Aug == 0 &&
        lr.Sep == 0 && lr.Oct == 0 && lr.Nov == 0 && lr.Dec == 0;

    private static void DistributeEvenly(BudgetLine line, decimal annual)
    {
        var monthly = Math.Round(annual / 12, 2);
        var remainder = annual - (monthly * 12);
        line.Jan = monthly; line.Feb = monthly; line.Mar = monthly;
        line.Apr = monthly; line.May = monthly; line.Jun = monthly;
        line.Jul = monthly; line.Aug = monthly; line.Sep = monthly;
        line.Oct = monthly; line.Nov = monthly; line.Dec = monthly + remainder;
    }

    private string GenerateBudgetNumber(int year)
    {
        var prefix = $"BG-{year}-";
        var last   = _repository.GetAll()
            .Where(b => b.BudgetNo.StartsWith(prefix))
            .OrderByDescending(b => b.Id)
            .FirstOrDefault();

        if (last == null) return $"{prefix}00001";
        var parts = last.BudgetNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n)
            ? $"{prefix}{(n + 1):D5}"
            : $"{prefix}00001";
    }
}