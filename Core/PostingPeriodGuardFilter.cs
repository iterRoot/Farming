using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PostingPeriodEntity = FarmingApi.Modules.Administration.PostingPeriods.PostingPeriod;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// Wires the PostingPeriods status dropdown (Unlocked / Closing Period /
// Locked / Locked Except Sales — Administration > System Initialization >
// Posting Periods) to something that actually blocks postings. Before this
// filter, that status was cosmetic: nothing in the app ever read it back.
//
// Scope: gates Create/Update on the 29 controllers below whose request body
// carries a posting date, by finding the field on whatever DTO the action
// bound (reflection, not a per-controller edit — same "one central piece of
// wiring" approach as SubjectAuthorizationFilter). It does NOT re-check the
// date on a document's later Post/Close transition (those actions take no
// body — the id alone doesn't tell us which entity type or column to load
// without a second per-controller map) — a document drafted while its
// period was open and posted after the period was later locked is not
// caught here. Disclosed limitation, not an oversight.
//
// Matched on request PATH (not ControllerName), same reasoning as
// SubjectMap: SaleQuotion answers on both /SaleQuotion and /SaleQuotation.
// ═══════════════════════════════════════════════════════════════════════════
public static class PostingPeriodDocMap
{
    // path prefix -> category. Category decides what a "Closing Period" or
    // "Locked Except Sales" status still allows through.
    public static readonly (string Prefix, string Category)[] Flat =
    {
        // Sales / AR
        ("/ARCreditNote", "Sales"),
        ("/ARDownPaymentInvoice", "Sales"),
        ("/ARInvoice", "Sales"),
        ("/ARReserveInvoice", "Sales"),
        ("/ArDownPaymentRequest", "Sales"),
        ("/Delivery", "Sales"),
        ("/Return", "Sales"),
        ("/ReturnRequest", "Sales"),
        ("/SaleBlanketAgreement", "Sales"),
        ("/SaleOrder", "Sales"),
        ("/SaleQuotion", "Sales"),
        ("/SaleQuotation", "Sales"),
        ("/IncomingPayment", "Sales"),

        // Purchase / AP
        ("/APCreditNote", "Purchase"),
        ("/APDownPaymentInvoice", "Purchase"),
        ("/APDownPaymentRequest", "Purchase"),
        ("/APInvoice", "Purchase"),
        ("/APReserveInvoice", "Purchase"),
        ("/GoodsReceiptPO", "Purchase"),
        ("/GoodsReturn", "Purchase"),
        ("/GoodsReturnRequest", "Purchase"),
        ("/LandingCost", "Purchase"),
        ("/PurchaseBlanketAgreement", "Purchase"),
        ("/PurchaseOrder", "Purchase"),
        ("/PurchaseQuotation", "Purchase"),
        ("/PurchaseRequest", "Purchase"),
        ("/OutgoingPayment", "Purchase"),

        // Inventory
        ("/GoodsReceipt", "Inventory"),
        ("/InventoryJournal", "Inventory"),
        ("/StockRevaluation", "Inventory"),

        // Financial — the kind of adjusting/closing entry a "Closing
        // Period" status is meant to still allow.
        ("/JournalEntry", "Financial"),
        ("/ConversionDifferences", "Financial"),
        ("/ExchangeRateDifferences", "Financial"),
        ("/DepreciationRun", "Financial"),
        ("/BankStatement", "Financial"), // only the row Post action carries a PostingDate
        ("/InternalTransfer", "Financial"),
        ("/CreditCardSettlement", "Financial"),
        ("/PeriodEndClosing", "Financial"),
    };

    // Most request DTOs carry the date as "PostingDate"; a few name it
    // differently. Keyed on the same path prefix as above.
    private static readonly Dictionary<string, string> DateField = new(StringComparer.OrdinalIgnoreCase)
    {
        ["/JournalEntry"] = "TransDate",
        ["/IncomingPayment"] = "DocDate",
        ["/OutgoingPayment"] = "DocDate",
    };

    public static (string Category, string DateField)? Resolve(PathString path)
    {
        foreach (var (prefix, category) in Flat)
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                return (category, DateField.TryGetValue(prefix, out var f) ? f : "PostingDate");
        return null;
    }
}

public class PostingPeriodGuardFilter : IAsyncActionFilter
{
    private readonly MyDbContext _db;
    public PostingPeriodGuardFilter(MyDbContext db) => _db = db;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var method = httpContext.Request.Method;

        if (method != "POST" && method != "PUT")
        {
            await next();
            return;
        }

        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await next();
            return;
        }

        var match = PostingPeriodDocMap.Resolve(httpContext.Request.Path);
        if (match == null)
        {
            await next();
            return;
        }

        var (category, dateFieldName) = match.Value;

        DateTime? postingDate = null;
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;
            var prop = arg.GetType().GetProperty(dateFieldName);
            if (prop == null) continue;
            if (prop.GetValue(arg) is DateTime dt) { postingDate = dt; break; }
        }

        if (postingDate == null)
        {
            // Not a Create/Update call with a posting date in the body
            // (e.g. Post/Void/Delete by id) — nothing for this filter to
            // check.
            await next();
            return;
        }

        var date = DateOnly.FromDateTime(postingDate.Value);
        var period = await _db.Set<PostingPeriodEntity>().AsNoTracking()
            .Where(p => p.PostingDateFrom != null && p.PostingDateTo != null
                     && p.PostingDateFrom <= date && p.PostingDateTo >= date)
            .FirstOrDefaultAsync();

        if (period == null)
        {
            // No defined period covers this date — fail open, same
            // rollout stance as SubjectAuthorizationFilter's unconfigured
            // users: nothing breaks until an admin actually defines and
            // locks periods.
            await next();
            return;
        }

        bool allowed = period.PeriodStatus switch
        {
            "Locked" => false,
            "Locked Except Sales" => category == "Sales",
            "Closing Period" => category == "Financial",
            _ => true, // "Unlocked" and any unrecognized status
        };

        if (!allowed)
        {
            var label = string.IsNullOrWhiteSpace(period.PeriodName) ? period.PeriodCode : period.PeriodName;
            context.Result = new BadRequestObjectResult(
                $"Posting date {date:yyyy-MM-dd} falls in period '{label}', which is {period.PeriodStatus}. " +
                $"{category} documents cannot be posted in this period.");
            return;
        }

        await next();
    }
}
