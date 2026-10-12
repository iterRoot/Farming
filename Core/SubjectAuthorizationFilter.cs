using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using UserAuthEntity = FarmingApi.Modules.Administration.Authorisations.UserAuthorization;

namespace FarmingApi.Core;

// ═══════════════════════════════════════════════════════════════════════════
// Maps a request path to one of the subject keys the Authorisations screen
// (KAMNTTRA src/.../Authorisations/shared/authSubjects.js) already lets an
// admin grant None/Read/Full for, per user. Built from the REAL registered
// route of each controller — verified from source, not guessed from folder
// or class names, which caught several real mismatches while extending this
// (e.g. PostingPeriodController's default route is the singular
// "/PostingPeriod", not "/PostingPeriods"; a "Company" controller nested
// under Companys/Company/ turned out to be the real Company Details backend
// that an earlier pass had marked as missing). Several controllers alias
// multiple paths (SaleQuotion also answers at /SaleQuotation), and two
// unrelated controllers both default to the class-derived name "UserGroup"
// but were deliberately split onto distinct literal routes to avoid an
// AmbiguousMatchException — so this always maps on the literal route
// PREFIX actually registered, never the bare class name.
//
// Every live controller (checked against a full source sweep — 117 route
// entries across 112 controllers, empty/fully-commented-out files excluded)
// now has an entry. A handful of small, low-stakes admin "preference" or
// inventory reference-data screens are bundled under one umbrella subject
// each (admin.systemPreferences, inv.setup) rather than given 25+ near-
// identical individual leaves — everything else is one controller, one
// subject. A route that somehow still isn't covered falls back to requiring
// Full rather than staying open.
// ═══════════════════════════════════════════════════════════════════════════
public static class SubjectMap
{
    public static readonly Dictionary<string, string[]> BySubject = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin.companyDetails"]      = new[] { "/Company" },
        ["admin.generalSettings"]     = new[] { "/GeneralSettings" },
        ["admin.postingPeriods"]      = new[] { "/PostingPeriod" },
        ["admin.users"]               = new[] { "/User" },
        ["admin.userGroups"]          = new[] { "/UserGroup" },
        ["admin.securityGroups"]      = new[] { "/SecurityUserGroup" },
        ["admin.authorisations"]      = new[] { "/UserAuthorization" },
        ["admin.branches"]            = new[] { "/Branch" },
        ["admin.docNumbering"]        = new[] { "/DocumentNumberRange" },
        ["admin.exchangeRates"]       = new[] { "/ExchangeRate", "/PriceIndex" },
        ["admin.currencies"]          = new[] { "/Currency", "/Currencies" },
        ["admin.taxCodes"]            = new[] { "/TaxCode" },
        ["admin.taxDetermination"]    = new[] { "/TaxDeterminationRule" },
        ["admin.withholdingTax"]      = new[] { "/WithholdingTaxCode" },
        ["admin.udf"]                 = new[] { "/UserDefinedField", "/UserTable" },
        ["admin.dataImportExport"]    = new[] { "/DataImportAndExport" },
        ["admin.licence"]             = new[] { "/LicensePackage" },
        ["admin.emailSettings"]       = new[] { "/EmailAccount" },
        ["admin.documentSettings"]    = new[] { "/DocumentSetting" },
        ["admin.menuAlias"]           = new[] { "/MenuAlias" },
        ["admin.electronicFileManager"] = new[] { "/ElectronicFile" },
        ["admin.userDefaults"]        = new[] { "/UserDefault" },
        ["admin.implementationCentre"] = new[] { "/ImplementationTask" },
        ["admin.addressFormats"]      = new[] { "/AddressFormat" },
        ["admin.bpPriorities"]        = new[] { "/BPProperty" },
        ["admin.electronicDocuments"] = new[] { "/ElectronicDocument" },
        ["admin.approvalProcess"]     = new[] { "/ApprovalProcess", "/ApprovalRequest" },
        ["admin.alerts"]              = new[] { "/AlertManagement" },
        ["admin.security"]            = new[] { "/PasswordPolicy", "/ReadOnlyDbUser" },
        // Low-stakes admin "preference" screens (print/report/dashboard
        // cosmetics) — one umbrella subject rather than 14 near-duplicate leaves.
        ["admin.systemPreferences"]   = new[]
        {
            "/CommissionGroup", "/SaleEmployeeBuyer", "/Territory", "/ElectronicCertificate",
            "/CrystalReportDefault", "/CrystalServerConfig", "/DashboardManager", "/DashboardParameter",
            "/MessagePreference", "/PredefinedText", "/ReferenceFieldLink", "/ReportLayout",
            "/SAPLink", "/ServerPrintConfig",
        },

        ["fin.chartOfAccounts"]       = new[] { "/ChartOfAccounts", "/EditChartOfAccounts" },
        ["fin.journalEntry"]          = new[] { "/JournalEntry" },
        ["fin.journalVouchers"]       = new[] { "/JournalVoucher" },
        ["fin.recurringPosting"]      = new[] { "/RecurringPosting" },
        ["fin.glDetermination"]       = new[] { "/GLAccountDetermination", "/GLAccountOverride" },
        ["fin.budget"]                = new[] { "/Budget" },
        ["fin.costAccounting"]        = new[] { "/CostCenter", "/DistributionRule" },
        ["fin.fixedAssets"]           = new[] { "/AssetsMaster", "/DepreciationRun" },
        ["fin.financialProjects"]     = new[] { "/FinancialProject" },
        ["fin.exchangeRateDifferences"] = new[] { "/ExchangeRateDifferences" },
        ["fin.conversionDifferences"] = new[] { "/ConversionDifferences" },

        ["sales.blanketAgreement"]    = new[] { "/SaleBlanketAgreement" },
        ["sales.quotation"]           = new[] { "/SaleQuotion", "/SaleQuotation" },
        ["sales.order"]               = new[] { "/SaleOrder" },
        ["sales.delivery"]            = new[] { "/Delivery" },
        ["sales.arInvoice"]           = new[] { "/ARInvoice" },
        ["sales.arReserveInvoice"]    = new[] { "/ARReserveInvoice" },
        ["sales.arCreditNote"]        = new[] { "/ARCreditNote" },
        ["sales.arDownPayment"]       = new[] { "/ARDownPaymentInvoice", "/ArDownPaymentRequest" },
        ["sales.returns"]             = new[] { "/Return", "/ReturnRequest" },
        ["sales.dunning"]             = new[] { "/Dunning", "/DunningTerm" },

        ["pur.blanketAgreement"]      = new[] { "/PurchaseBlanketAgreement" },
        ["pur.quotation"]             = new[] { "/PurchaseQuotation" },
        ["pur.request"]               = new[] { "/PurchaseRequest" },
        ["pur.order"]                 = new[] { "/PurchaseOrder" },
        ["pur.goodsReceiptPO"]        = new[] { "/GoodsReceiptPO" },
        ["pur.landedCost"]            = new[] { "/LandingCost" },
        ["pur.goodsReturn"]           = new[] { "/GoodsReturn", "/GoodsReturnRequest" },
        ["pur.apInvoice"]             = new[] { "/APInvoice" },
        ["pur.apReserveInvoice"]      = new[] { "/APReserveInvoice" },
        ["pur.apCreditNote"]          = new[] { "/APCreditNote" },
        ["pur.apDownPayment"]         = new[] { "/APDownPaymentInvoice", "/APDownPaymentRequest" },

        ["bp.master"]                 = new[] { "/BusinessPartnersMaster", "/Attachment" },
        ["bp.activity"]               = new[] { "/Activity" },
        ["bp.reconciliation"]         = new[] { "/BPReconciliation" },

        ["bank.incomingPayment"]      = new[] { "/IncomingPayment" },
        ["bank.outgoingPayment"]      = new[] { "/OutgoingPayment" },
        ["bank.checkRegister"]        = new[] { "/CheckRegister" },
        ["bank.deposit"]              = new[] { "/Deposit" },
        ["bank.paymentRun"]           = new[] { "/PaymentRun" },
        ["bank.reconciliation"]       = new[] { "/BankReconciliation" },
        ["bank.reconciliation"]       = new[] { "/BankReconciliation" },

        ["inv.itemMaster"]            = new[] { "/ItemsMaster" },
        ["inv.warehouse"]             = new[] { "/Warehouse" },
        ["inv.goodsReceipt"]          = new[] { "/GoodsReceipt" },
        ["inv.goodsIssue"]            = new[] { "/GoodsIssue" },
        ["inv.stockTransfer"]         = new[] { "/StockTransfer", "/StockTransferRequest" },
        ["inv.pickList"]              = new[] { "/PickList" },
        ["inv.pickAndPack"]           = new[] { "/pickpack" },
        ["inv.priceList"]             = new[] { "/PriceList" },
        ["inv.billOfMaterial"]        = new[] { "/BillOfMaterial" },
        ["inv.productionOrder"]       = new[] { "/ProductionOrder" },
        ["inv.mrp"]                   = new[] { "/MRP" },
        ["inv.stockRevaluation"]      = new[] { "/StockRevaluation" },
        ["inv.barCode"]               = new[] { "/BarCode" },
        ["inv.inventoryJournal"]      = new[] { "/InventoryJournal", "/StockBatch" },
        ["inv.records"]               = new[] { "/Inventory" },
        // Item groups, UoM, package/shipping types, locations, etc. — one
        // umbrella subject for 13 small inventory reference-data screens.
        ["inv.setup"]                 = new[]
        {
            "/CycleCountDetermination", "/DutyGroup", "/ItemGroup", "/ItemProperty",
            "/LengthWidthUom", "/Location", "/Manufacturer", "/PackageType",
            "/ShippingType", "/StockCycle", "/UnitOfMeasure", "/UomGroup", "/WeightUom",
        },

        ["crm.opportunity"]           = new[] { "/Opportunity" },
        ["crm.campaign"]              = new[] { "/Campaign" },
        ["crm.serviceCall"]           = new[] { "/ServiceCall" },
        ["crm.serviceContract"]       = new[] { "/ServiceContract" },
        ["crm.projectManagement"]     = new[] { "/Project", "/projects" },

        ["hr.employeeMaster"]         = new[] { "/EmployeeMaster", "/Staff" },
        ["hr.attendance"]             = new[] { "/Attendance" },
        ["hr.leaveRequest"]           = new[] { "/LeaveRequest" },
        ["hr.payroll"]                = new[] { "/Payroll" },
        ["hr.recruitment"]            = new[] { "/JobPosting", "/Candidate" },
        ["admin.savedSearch"]         = new[] { "/SavedSearch" },

        ["rep.financial"]             = new[] { "/FinancialReportTemplates", "/FinancialReport" },
        ["rep.inventory"]             = new[] { "/Reports/Inventory" },

        // Not mapped (no single matching controller — falls back to
        // requiring Full, same as any other unmapped subject):
        // general.login/.printing/.export, inv.inventoryCount,
        // rep.sales, rep.purchasing.
    };

    // Longest prefix first, so "/Reports/Inventory" is checked before any
    // shorter unrelated prefix that might otherwise match first.
    private static readonly List<(string Prefix, string Subject)> Flat =
        BySubject.SelectMany(kv => kv.Value.Select(p => (Prefix: p, Subject: kv.Key)))
                 .OrderByDescending(x => x.Prefix.Length)
                 .ToList();

    public static string? Resolve(PathString path)
    {
        foreach (var (prefix, subject) in Flat)
            if (path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase))
                return subject;
        return null;
    }
}

// ═══════════════════════════════════════════════════════════════════════════
// Runs after authentication (FallbackPolicy already rejected anonymous
// requests) and checks the caller's UserAuthorization row for the subject
// the request maps to. GET/HEAD needs Read-or-Full; everything else needs
// Full. No row at all → fail OPEN (unconfigured users, including the
// seeded Manager, keep full access until an admin configures them — a
// deliberate rollout choice, not an oversight).
// ═══════════════════════════════════════════════════════════════════════════
public class SubjectAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly MyDbContext _db;
    public SubjectAuthorizationFilter(MyDbContext db) => _db = db;

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private static bool IsFormHelper(string path, string method)
    {
        var p = path.ToLowerInvariant().TrimEnd('/');
        if (p.StartsWith("/userdefinedfield/values")) return true;   // read and set a record's field values
        if (method != "GET" && method != "HEAD") return false;
        return p == "/userdefinedfield" || p.StartsWith("/userdefinedfield/targets")
            || p.StartsWith("/doclink/remaining")
            || p == "/saleemployeebuyer/salesemployees" || p == "/saleemployeebuyer/buyers"
            || p.StartsWith("/stockbatch")
            || (p.StartsWith("/usertable/") && p.EndsWith("/rows"));
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;

        var endpoint = httpContext.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null) return;
        if (httpContext.User.Identity?.IsAuthenticated != true) return; // FallbackPolicy already rejects this

        var idClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var userId))
        {
            context.Result = new ObjectResult(new { message = "Could not identify the current user." }) { StatusCode = 403 };
            return;
        }

        var auth = await _db.Set<UserAuthEntity>().AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId);

        if (auth == null) return;               // fail-open: not configured yet
        if (auth.FullAuthorization) return;      // shortcut flag — everything granted

        // Lookups every form needs while a user is filling in a document (field definitions and values,
        // sales employees, copy-from balances, batch pickers, dropdown rows) aren't gated by the
        // administration subjects they happen to be served from — the document's own subject governs saving it.
        if (IsFormHelper(httpContext.Request.Path.Value ?? "", httpContext.Request.Method)) return;

        var subject = SubjectMap.Resolve(httpContext.Request.Path);

        var method = httpContext.Request.Method;
        var requiredLevel = (method == "GET" || method == "HEAD") ? "Read" : "Full";

        string level = "None";
        if (subject != null && !string.IsNullOrWhiteSpace(auth.Permissions))
        {
            var perms = JsonSerializer.Deserialize<Dictionary<string, string>>(auth.Permissions, JsonOpts) ?? new();
            perms.TryGetValue(subject, out var found);
            level = found ?? "None";
        }
        // subject == null: controller isn't in the catalogue yet — level stays
        // "None", so it requires Full same as any explicitly-denied subject.

        bool allowed = requiredLevel == "Read"
            ? level is "Read" or "Full"
            : level == "Full";

        if (!allowed)
        {
            context.Result = new ObjectResult(new
            {
                message = subject != null
                    ? $"You don't have permission for this ({subject})."
                    : "You don't have permission for this area.",
            })
            { StatusCode = 403 };
        }
    }
}
