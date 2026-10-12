using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Employee = FarmingApi.Modules.Sales.SaleEmployeeBuyer.SaleEmployeeBuyer;

namespace FarmingApi.Core;

// Request bodies that carry a sales employee (sales documents) or a buyer (purchase documents).
public interface IHasSalesEmployee { string? SalesEmployeeCode { get; set; } }
public interface IHasBuyer { string? BuyerCode { get; set; } }

// ═══════════════════════════════════════════════════════════════
// Checks the sales employee / buyer on a document before the controller runs:
// blank means "none"; otherwise the code must exist, be active, and be
// eligible for the role (SalesEmployee or Both / Buyer or Both). The code is
// stored in its canonical (upper-case) form.
// ═══════════════════════════════════════════════════════════════
public class EmployeeTagFilter : IAsyncActionFilter
{
    private readonly MyDbContext _db;
    public EmployeeTagFilter(MyDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var method = context.HttpContext.Request.Method;
        if (method == "POST" || method == "PUT")
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg is IHasSalesEmployee s)
                {
                    var err = await Check(s.SalesEmployeeCode, "Sales employee", "SalesEmployee", c => s.SalesEmployeeCode = c);
                    if (err != null) { context.Result = new BadRequestObjectResult(err); return; }
                }
                if (arg is IHasBuyer b)
                {
                    var err = await Check(b.BuyerCode, "Buyer", "Buyer", c => b.BuyerCode = c);
                    if (err != null) { context.Result = new BadRequestObjectResult(err); return; }
                }
            }
        }
        await next();
    }

    private async Task<string?> Check(string? code, string label, string role, Action<string?> set)
    {
        if (string.IsNullOrWhiteSpace(code)) { set(null); return null; }
        var c = code.Trim().ToUpperInvariant();
        var e = await _db.Set<Employee>().AsNoTracking().FirstOrDefaultAsync(x => x.Code == c);
        if (e == null) return $"{label} '{code.Trim()}' does not exist.";
        if (!e.IsActive) return $"{label} '{c}' is inactive.";
        if (e.EmployeeType != "Both" && e.EmployeeType != role) return $"'{c}' is a {e.EmployeeType}, not a {label.ToLower()}.";
        set(c);
        return null;
    }
}
