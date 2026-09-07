// using System.Net.Http.Headers;
// using FarmingApi.Core;

// namespace EventHubChatting.Core;

// public class ServiceHttpClient
// {
// 	private readonly HttpClient _httpClient;

// 	public ServiceHttpClient(string path)
// 	{
// 		_httpClient = new HttpClient();
// 		var environment = _getEnv();
// 		_httpClient.BaseAddress = new Uri($"https://{path}-{environment}.eventhub.one");
// 		// _addToken();
// 	}

// 	private static string _getEnv()
// 	{
// 		var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// 		return env switch
// 		{
// 			"Production" => "prod",
// 			"Uat" => "uat",
// 			_ => "dev"
// 		};
// 	}

// 	// private void _addToken()
// 	// {
// 	// 	var id = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "93bfb7c4-dfde-487a-a5fb-0d01f77a64c5";
// 	// 	// var token = AuthenticationExtension.GenerateToken(Guid.Parse(id), "Service");
// 	// 	_httpClient.DefaultRequestHeaders.Authorization =
// 	// 		new AuthenticationHeaderValue("Bearer", token);
// 	// }

// 	public async Task<HttpResponseMessage> GetAsync(string path)
// 	{
// 		return await _httpClient.GetAsync(path);
// 	}

// 	public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
// 	{
// 		return await _httpClient.PostAsJsonAsync(path, body);
// 	}

// 	public async Task<HttpResponseMessage> PutAsync<T>(string path, T body)
// 	{
// 		return await _httpClient.PutAsJsonAsync(path, body);
// 	}

// 	public async Task<HttpResponseMessage> DeleteAsync(string path)
// 	{
// 		return await _httpClient.DeleteAsync(path);
// 	}
// }


using FarmingApi.Modules.Administration.DocumentNumberRange;
using FarmingApi.Modules.Financials.GLAccountDetermination;
using FarmingApi.Modules.Financials.JournalEntry;
using FarmingApi.Modules.SaleAR.ARInvoice;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
namespace FarmingApi.Services;

// ═══════════════════════════════════════════════════════════════════
// Place this file at:  /ERP/Services/DocumentNumberService.cs
// Register in Program.cs:
//   builder.Services.AddScoped<IDocumentNumberService, DocumentNumberService>();
// ═══════════════════════════════════════════════════════════════════

public interface IDocumentNumberService
{
    string Next(string documentType, string? seriesName = null);
    string Peek(string documentType, string? seriesName = null);
}

public class DocumentNumberService : IDocumentNumberService
{
    private readonly MyDbContext _db;

    public DocumentNumberService(MyDbContext db) => _db = db;

    public string Next(string documentType, string? seriesName = null)
    {
        var range = GetRange(documentType, seriesName);

        if (range == null)
            throw new InvalidOperationException(
                $"No number range configured for '{documentType}'. " +
                "Go to Administration → Document Numbering to create one.");

        if (range.IsLocked)
            throw new InvalidOperationException(
                $"Series '{range.SeriesName}' for '{documentType}' is locked.");

        if (range.LastNum.HasValue && range.NextNum > range.LastNum.Value)
            throw new InvalidOperationException(
                $"Number range for '{documentType}' is exhausted (max: {range.LastNum}).");

        int num = range.NextNum;
        range.NextNum++;
        range.UpdatedAt = DateTime.UtcNow;
        _db.SaveChanges();
        return Format(range, num);
    }

    public string Peek(string documentType, string? seriesName = null)
    {
        var range = GetRange(documentType, seriesName);
        if (range == null) return $"{documentType}-NOT-CONFIGURED";
        return Format(range, range.NextNum);
    }

    private DocumentNumberRange? GetRange(string documentType, string? seriesName)
    {
        var q = _db.Set<DocumentNumberRange>()
                   .Where(d => d.DocumentType == documentType && !d.IsLocked);
        return !string.IsNullOrEmpty(seriesName)
            ? q.FirstOrDefault(d => d.SeriesName == seriesName)
            : q.FirstOrDefault(d => d.IsDefault) ?? q.FirstOrDefault();
    }

    public static string Format(DocumentNumberRange range, int num)
    {
        var parts = new List<string> { range.Prefix };
        if (range.IncludeYear) parts.Add(DateTime.Now.Year.ToString());
        parts.Add(num.ToString().PadLeft(range.PadLength, '0'));
        return string.Join("-", parts);
    }

// ═══════════════════════════════════════════════════════════════════
// 2. AR INVOICE JOURNAL SERVICE
//    Auto-creates a Journal Entry when an AR Invoice is posted.
//
//    DR  Accounts Receivable    Total (incl. tax)
//    CR  Sales Revenue          Total - TaxAmount
//    CR  Output VAT             TaxAmount (only if > 0)
//
//    KGLD field priority:
//      AccountsReceivable → ARControlAccount (fallback)
//      SalesRevenue       → RevenueAccount   (fallback)
//      SalesTax           → TaxOutputAccount  (fallback)
//
//    Uses MyDbContext directly — does NOT call SaveChanges.
//    The controller wraps both invoice + JE in one transaction.
// ═══════════════════════════════════════════════════════════════════
 

public class ARInvoiceJournalService : IARInvoiceJournalService
{
    private readonly MyDbContext _db;
 
    public ARInvoiceJournalService(MyDbContext db)
    {
        _db = db;
    }
 
    /// <summary>Pick the first non-empty account code from a list of candidates.</summary>
    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
 
    public JournalEntry CreateJournalEntry(ARInvoice invoice, BPEntity customer)
    {
        // ── 1. Load KGLD row ──────────────────────────────────────
        var gl = _db.Set<GLAccountDetermination>().FirstOrDefault()
            ?? throw new InvalidOperationException(
                "GL Account Determination (KGLD) not configured. " +
                "Go to Financials → GL Account Determination and set " +
                "AccountsReceivable, SalesRevenue, SalesTax.");
 
        // ── 2. Resolve account codes ──────────────────────────────
        //    Priority: BP override → KGLD primary → KGLD fallback
        var arCode = FirstNonEmpty(
            customer.ARControlAccount,   // BP-level override
            gl.AccountsReceivable,       // your KGLD field
            gl.ARControlAccount);        // fallback
 
        var revenueCode = FirstNonEmpty(
            gl.SalesRevenue,             // your KGLD field
            gl.RevenueAccount);          // fallback
 
        var taxCode = FirstNonEmpty(
            gl.SalesTax,                 // your KGLD field
            gl.TaxOutputAccount);        // fallback
 
        if (string.IsNullOrWhiteSpace(arCode))
            throw new InvalidOperationException(
                "Accounts Receivable account not set. Configure 'AccountsReceivable' " +
                "in GL Account Determination (or on the Business Partner).");
        if (string.IsNullOrWhiteSpace(revenueCode))
            throw new InvalidOperationException(
                "Sales Revenue account not set. Configure 'SalesRevenue' " +
                "in GL Account Determination.");
 
        // ── 3. Amounts ────────────────────────────────────────────
        var grandTotal = invoice.Total;
        var taxAmount  = invoice.TaxAmount;
        var netRevenue = grandTotal - taxAmount;
 
        if (grandTotal <= 0)
            throw new InvalidOperationException(
                $"Cannot post AR Invoice {invoice.DocNum}: total is {grandTotal}.");
 
        // ── 4. Build JE header ────────────────────────────────────
        var postingDate = invoice.PostingDate ?? DateTime.UtcNow;
 
        var je = new JournalEntry
        {
            JrnlNo          = GenerateJournalNumber(),
            TransDate       = postingDate,
            DueDate         = invoice.DueDate ?? postingDate,
            DocDate         = postingDate,
            RefNo           = invoice.DocNum,
            Memo            = $"AR Invoice {invoice.DocNum} — {customer.CardName}",
            TransType       = "AR",
            BaseDocEntry    = invoice.Id,
            BaseDocType     = "KARI",
            Currency        = "USD",
            ExchangeRate    = 1,
            TotalDebit      = 0,
            TotalCredit     = 0,
            TotalLC         = 0,
            TotalFC         = 0,
            Status          = "O",
            IsAutoGenerated = true,
            VersionNum      = 1,
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };
 
        int lineNum = 1;
 
        // ── 5. DR: Accounts Receivable (full incl. tax) ───────────
        je.Lines.Add(new JournalEntryLine
        {
            LineNum     = lineNum++,
            AccountCode = arCode!,
            AccountName = "Accounts Receivable",
            CardCode    = customer.Code,
            CardName    = customer.CardName,
            Debit       = grandTotal,
            Credit      = 0,
            DebitLC     = grandTotal,
            CreditLC    = 0,
            DebitFC     = 0,
            CreditFC    = 0,
            LineMemo    = $"AR — {customer.Code} {customer.CardName}",
            TaxAmount   = 0,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        });
 
        // ── 6. CR: Sales Revenue (net excl. tax) ──────────────────
        je.Lines.Add(new JournalEntryLine
        {
            LineNum     = lineNum++,
            AccountCode = revenueCode!,
            AccountName = "Sales Revenue",
            CardCode    = null,
            CardName    = null,
            Debit       = 0,
            Credit      = netRevenue,
            DebitLC     = 0,
            CreditLC    = netRevenue,
            DebitFC     = 0,
            CreditFC    = 0,
            LineMemo    = $"Revenue — {invoice.DocNum}",
            TaxAmount   = 0,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        });
 
        // ── 7. CR: Output VAT (only when tax > 0) ────────────────
        if (taxAmount > 0)
        {
            if (string.IsNullOrWhiteSpace(taxCode))
                throw new InvalidOperationException(
                    "Invoice has tax but 'SalesTax' account is not set in " +
                    "GL Account Determination.");
 
            je.Lines.Add(new JournalEntryLine
            {
                LineNum     = lineNum++,
                AccountCode = taxCode!,
                AccountName = "Output VAT",
                CardCode    = null,
                CardName    = null,
                Debit       = 0,
                Credit      = taxAmount,
                DebitLC     = 0,
                CreditLC    = taxAmount,
                DebitFC     = 0,
                CreditFC    = 0,
                LineMemo    = $"Output VAT — {invoice.DocNum}",
                TaxAmount   = taxAmount,
                CreatedAt   = DateTime.UtcNow,
                InActive    = false,
            });
        }
 
        // ── 8. Balance check + totals ─────────────────────────────
        var totalDr = je.Lines.Sum(l => l.Debit);
        var totalCr = je.Lines.Sum(l => l.Credit);
 
        if (Math.Abs(totalDr - totalCr) > 0.01m)
            throw new InvalidOperationException(
                $"Journal entry unbalanced: Dr={totalDr:N2} Cr={totalCr:N2}. " +
                $"Check invoice Total ({grandTotal}) vs TaxAmount ({taxAmount}).");
 
        je.TotalDebit  = totalDr;
        je.TotalCredit = totalCr;
        je.TotalLC     = totalDr;
        je.TotalFC     = totalDr;
 
        // ── 9. Add to DbContext (controller calls SaveChanges) ────
        _db.Set<JournalEntry>().Add(je);
 
        return je;
    }
 
    // ── Generate JE number: JE-2026-00001 ─────────────────────────
    private string GenerateJournalNumber()
    {
        var year   = DateTime.Now.Year;
        var prefix = $"JE-{year}-";
 
        var last = _db.Set<JournalEntry>()
            .Where(j => j.JrnlNo.StartsWith(prefix))
            .OrderByDescending(j => j.Id)
            .FirstOrDefault();
 
        if (last == null)
            return $"{prefix}00001";
 
        var parts = last.JrnlNo.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNum))
            return $"{prefix}{(lastNum + 1):D5}";
 
        return $"{prefix}00001";
    }
}
}