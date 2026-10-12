using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Administration.ReportLayoutManager;

namespace FarmingApi.Core;

// ══════════════════════════════════════════════════════════════════════════════
// Central hook every document's own GET {id}/Pdf action calls first. If an
// Active, IsDefault, LayoutEngine=="HTML" ReportLayout is registered for that
// document type, its HtmlTemplate is merged against the live document and
// returned instead of the document's hardcoded QuestPDF renderer — the one
// thing that actually makes the Report & Layout Manager registry affect real
// output. No matching layout → null, caller falls back to its existing PDF.
// ══════════════════════════════════════════════════════════════════════════════
public interface IPrintLayoutRenderService
{
    Task<string?> TryRenderAsync(string documentType, object header, IEnumerable? lines = null);

    // For the layout editor's live preview: the most recently created real
    // document of this type (and its lines, when that document type has
    // any), so a template can be previewed against real data before saving.
    Task<(object? Header, IEnumerable? Lines)> GetSampleAsync(string documentType);
}

public class PrintLayoutRenderService : IPrintLayoutRenderService
{
    private readonly MyDbContext _db;
    public PrintLayoutRenderService(MyDbContext db) { _db = db; }

    public async Task<(object? Header, IEnumerable? Lines)> GetSampleAsync(string documentType)
    {
        switch (documentType)
        {
            case "SaleQuotion":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.SaleQuotion.SaleQuotion>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "SaleBlanketAgreement":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.SaleBlanketAgreement.SaleBlanketAgreement>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "SaleOrder":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.SaleOrder.SaleOrder>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "ArDownPaymentRequest":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.ArDownPaymentRequest.ArDownPaymentRequest>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "Delivery":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.Delivery.Delivery>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "ARInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "ARReserveInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.ARReserveInvoice.ARReserveInvoice>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "ARCreditNote":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.ARCreditNote.ARCreditNote>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "ARDownPaymentInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.SaleAR.ARDownPaymentInvoice.ARDownPaymentInvoice>()
                    .Include(x => x.Customer).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "PurchaseQuotation":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.PurchaseQuotation.PurchaseQuotation>()
                    .Include(x => x.Vendor).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "PurchaseBlanketAgreement":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.PurchaseBlanketAgreement.PurchaseBlanketAgreement>()
                    .Include(x => x.Vendor).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "PurchaseOrder":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.PurchaseOrder.PurchaseOrder>()
                    .Include(x => x.Vendor).Include(x => x.Lines).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Lines);
            }
            case "APDownPaymentRequest":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.APDownPaymentRequest.APDownPaymentRequest>()
                    .Include(x => x.Vendor)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, null);
            }
            case "GoodsReceiptPO":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.GoodsReceiptPO.GoodsReceiptPO>()
                    .Include(x => x.Vendor).Include(x => x.Lines).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Lines);
            }
            case "APInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.APInvoice.APInvoice>()
                    .Include(x => x.Vendor).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "APCreditNote":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.APCreditNote.APCreditNote>()
                    .Include(x => x.Vendor).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "APReserveInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.APReserveInvoice.APReserveInvoice>()
                    .Include(x => x.Vendor).Include(x => x.Items).ThenInclude(l => l.Item)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, e?.Items);
            }
            case "APDownPaymentInvoice":
            {
                var e = await _db.Set<FarmingApi.Modules.PurchaseAP.APDownPaymentInvoice.APDownPaymentInvoice>()
                    .Include(x => x.Vendor)
                    .OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                return (e, null);
            }
            default:
                return (null, null);
        }
    }

    public async Task<string?> TryRenderAsync(string documentType, object header, IEnumerable? lines = null)
    {
        var layout = await _db.Set<ReportLayout>()
            .Where(x => x.DocumentType == documentType
                     && x.LayoutType   == "PrintLayout"
                     && x.LayoutEngine == "HTML"
                     && x.IsDefault
                     && x.IsActive
                     && x.HtmlTemplate != null)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        return layout?.HtmlTemplate == null ? null : Merge(layout.HtmlTemplate, header, lines);
    }

    // {{#each Items}}...{{/each}} — the block name itself is cosmetic (there is
    // only ever one line collection per document), repeated once per row in
    // `lines` with {{Field}}/{{Nav.Field}} resolved against that row.
    private static readonly Regex EachBlock =
        new(@"\{\{#each\s+\w+\}\}(.*?)\{\{/each\}\}", RegexOptions.Singleline | RegexOptions.Compiled);

    // {{Field}} or {{Nav.Field}} — dot-path property lookup via reflection.
    private static readonly Regex Token =
        new(@"\{\{\s*([\w\.]+)\s*\}\}", RegexOptions.Compiled);

    public static string Merge(string template, object header, IEnumerable? lines)
    {
        var withLines = EachBlock.Replace(template, m =>
        {
            if (lines == null) return "";
            var rowTemplate = m.Groups[1].Value;
            var sb = new StringBuilder();
            foreach (var row in lines)
                sb.Append(Token.Replace(rowTemplate, t => Format(GetValue(row, t.Groups[1].Value))));
            return sb.ToString();
        });

        return Token.Replace(withLines, t => Format(GetValue(header, t.Groups[1].Value)));
    }

    private static object? GetValue(object? obj, string path)
    {
        foreach (var part in path.Split('.'))
        {
            if (obj == null) return null;
            var prop = obj.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return null;
            obj = prop.GetValue(obj);
        }
        return obj;
    }

    private static string Format(object? value) => value switch
    {
        null        => "",
        DateTime dt => dt.ToString("yyyy-MM-dd"),
        decimal d   => d.ToString("N2"),
        double d    => d.ToString("N2"),
        bool b      => b ? "Yes" : "No",
        _           => value.ToString() ?? "",
    };
}
