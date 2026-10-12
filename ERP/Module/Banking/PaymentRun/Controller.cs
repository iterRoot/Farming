using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using BPEntity        = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;
using ARInvoiceEntity = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;
using APInvoiceEntity = FarmingApi.Modules.PurchaseAP.APInvoice.APInvoice;
using IPay             = FarmingApi.Modules.Banking.IncomingPayment.IncomingPayment;
using IPayInvoice       = FarmingApi.Modules.Banking.IncomingPayment.IncomingPaymentInvoice;
using IIncomingPaymentJournalService = FarmingApi.Modules.Banking.IncomingPayment.IIncomingPaymentJournalService;
using OPay              = FarmingApi.Modules.Banking.OutgoingPayment.OutgoingPayment;
using OPayInvoice       = FarmingApi.Modules.Banking.OutgoingPayment.OutgoingPaymentInvoice;
using IOutgoingPaymentJournalService = FarmingApi.Modules.Banking.OutgoingPayment.IOutgoingPaymentJournalService;

namespace FarmingApi.Modules.Banking.PaymentRun;

// ═══════════════════════════════════════════════════════════════════
// PAYMENT RUN  (route: /PaymentRun)
// A batch wizard, not a new document type — it creates real Incoming
// or Outgoing Payments, one per business partner, via the exact same
// journal-posting services those modules already use. Always pays a
// BP's full matching balance across every matching invoice; partial
// amounts, discounts and withholding tax stay on the single-payment
// screens where that detail belongs.
//   POST /PaymentRun/Preview  → who would be paid and how much
//   POST /PaymentRun/Execute  → actually create the payments
// ═══════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class PaymentRunController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IIncomingPaymentJournalService _incomingJournal;
    private readonly IOutgoingPaymentJournalService _outgoingJournal;

    public PaymentRunController(MyDbContext db,
        IIncomingPaymentJournalService incomingJournal, IOutgoingPaymentJournalService outgoingJournal)
    {
        _db = db;
        _incomingJournal = incomingJournal;
        _outgoingJournal = outgoingJournal;
    }

    [HttpPost("Preview")]
    public IActionResult Preview([FromBody] PaymentRunCriteria criteria)
    {
        var candidates = criteria.BpType == "Customer"
            ? OpenArByCustomer(criteria)
            : OpenApByVendor(criteria);
        return Ok(new PaymentRunPreviewResponse { Candidates = candidates });
    }

    [HttpPost("Execute")]
    public IActionResult Execute([FromBody] PaymentRunExecuteRequest req)
    {
        if (req.CardCodes == null || req.CardCodes.Count == 0)
            return BadRequest("Select at least one business partner to pay.");
        if (string.IsNullOrWhiteSpace(req.BankAccount) && req.PaymentMethod != "Cash")
            return BadRequest("A bank/cash account is required.");

        var candidates = (req.BpType == "Customer" ? OpenArByCustomer(req) : OpenApByVendor(req))
            .Where(c => req.CardCodes.Contains(c.CardCode))
            .ToList();

        var results = new List<PaymentRunResultLine>();
        foreach (var candidate in candidates)
        {
            try
            {
                var (id, docNum, closed) = req.BpType == "Customer"
                    ? CreateIncomingPayment(candidate, req)
                    : CreateOutgoingPayment(candidate, req);
                results.Add(new PaymentRunResultLine
                {
                    CardCode = candidate.CardCode, CardName = candidate.CardName, Success = true,
                    PaymentId = id, DocNum = docNum, Amount = candidate.TotalDue, InvoicesClosed = closed,
                });
            }
            catch (Exception ex)
            {
                // One BP's problem (usually missing GL setup) does not stop the rest of the run.
                results.Add(new PaymentRunResultLine
                {
                    CardCode = candidate.CardCode, CardName = candidate.CardName, Success = false,
                    Error = ex.InnerException?.Message ?? ex.Message, Amount = candidate.TotalDue,
                });
            }
        }

        return Ok(new PaymentRunExecuteResponse
        {
            PaymentsCreated = results.Count(r => r.Success),
            TotalAmount     = results.Where(r => r.Success).Sum(r => r.Amount),
            Results         = results,
        });
    }

    // ── AP: open invoices grouped by vendor, matching the due-date window ──
    private List<PaymentRunCandidate> OpenApByVendor(PaymentRunCriteria c)
    {
        var q = _db.Set<APInvoiceEntity>().Where(i => i.Status == "O");
        if (c.DueDateFrom.HasValue) q = q.Where(i => i.DueDate >= c.DueDateFrom.Value);
        if (c.DueDateTo.HasValue)   q = q.Where(i => i.DueDate <= c.DueDateTo.Value);

        var invoices = q.ToList();
        var vendors  = _db.Set<BPEntity>().Where(b => invoices.Select(i => i.VendorId).Contains(b.Id))
            .ToDictionary(b => b.Id, b => (b.Code, b.CardName));
        if (!string.IsNullOrWhiteSpace(c.BpCode))
            invoices = invoices.Where(i => vendors.GetValueOrDefault(i.VendorId).Code == c.BpCode).ToList();

        return invoices.GroupBy(i => i.VendorId).Select(g =>
        {
            var (code, name) = vendors.GetValueOrDefault(g.Key, (g.Key.ToString(), g.Key.ToString()));
            return new PaymentRunCandidate
            {
                CardCode = code, CardName = name,
                TotalDue = g.Sum(i => OpenApBalance(i.Id, i.Total)),
                Invoices = g.Select(i => new PaymentRunCandidateInvoice
                {
                    InvoiceId = i.Id, DocNum = i.DocNum, DueDate = i.DueDate, Balance = OpenApBalance(i.Id, i.Total),
                }).Where(x => x.Balance > 0).ToList(),
            };
        }).Where(c2 => c2.TotalDue > 0).OrderBy(c2 => c2.CardName).ToList();
    }

    // ── AR: open invoices grouped by customer, matching the due-date window ──
    private List<PaymentRunCandidate> OpenArByCustomer(PaymentRunCriteria c)
    {
        var q = _db.Set<ARInvoiceEntity>().Where(i => i.Status == "O");
        if (c.DueDateFrom.HasValue) q = q.Where(i => i.DueDate >= c.DueDateFrom.Value);
        if (c.DueDateTo.HasValue)   q = q.Where(i => i.DueDate <= c.DueDateTo.Value);

        var invoices  = q.ToList();
        var customers = _db.Set<BPEntity>().Where(b => invoices.Select(i => i.CustomerId).Contains(b.Id))
            .ToDictionary(b => b.Id, b => (b.Code, b.CardName));
        if (!string.IsNullOrWhiteSpace(c.BpCode))
            invoices = invoices.Where(i => customers.GetValueOrDefault(i.CustomerId).Code == c.BpCode).ToList();

        return invoices.GroupBy(i => i.CustomerId).Select(g =>
        {
            var (code, name) = customers.GetValueOrDefault(g.Key, (g.Key.ToString(), g.Key.ToString()));
            return new PaymentRunCandidate
            {
                CardCode = code, CardName = name,
                TotalDue = g.Sum(i => OpenArBalance(i.Id, i.Total)),
                Invoices = g.Select(i => new PaymentRunCandidateInvoice
                {
                    InvoiceId = i.Id, DocNum = i.DocNum, DueDate = i.DueDate, Balance = OpenArBalance(i.Id, i.Total),
                }).Where(x => x.Balance > 0).ToList(),
            };
        }).Where(c2 => c2.TotalDue > 0).OrderBy(c2 => c2.CardName).ToList();
    }

    private decimal OpenApBalance(int invoiceId, decimal total)
    {
        var applied = _db.Set<OPayInvoice>()
            .Where(l => l.InvoiceId == invoiceId && l.OutgoingPayment.Status != "V")
            .Sum(l => (decimal?)(l.AppliedAmount + l.DiscountAmount + l.WithholdingTax)) ?? 0m;
        return total - applied;
    }

    private decimal OpenArBalance(int invoiceId, decimal total)
    {
        var applied = _db.Set<IPayInvoice>()
            .Where(l => l.InvoiceId == invoiceId && l.IncomingPayment.Status != "V")
            .Sum(l => (decimal?)l.AppliedAmount) ?? 0m;
        return total - applied;
    }

    // Mirrors OutgoingPaymentController.Create's allocation/JE pattern exactly,
    // simplified to "pay every matching invoice in full" — no partial amounts,
    // discounts or withholding tax in a batch run.
    private (int id, string docNum, int closed) CreateOutgoingPayment(PaymentRunCandidate candidate, PaymentRunExecuteRequest req)
    {
        var vendor = _db.Set<BPEntity>().FirstOrDefault(b => b.Code == candidate.CardCode);

        var entity = new OPay
        {
            DocNum          = NextDocNum("PAY-OUT"),
            DocDate         = req.PaymentDate,
            DueDate         = req.PaymentDate,
            CardCode        = candidate.CardCode,
            CardName        = candidate.CardName,
            DocTotal        = candidate.TotalDue,
            AppliedAmount   = candidate.TotalDue,
            UnappliedAmount = 0,
            Currency        = "USD",
            ExchangeRate    = 1,
            PaymentMethod   = req.PaymentMethod,
            BankAccount     = req.BankAccount,
            CheckStatus     = req.PaymentMethod == "Check" ? "Issued" : null,
            Status          = "O",
            TransType       = "KPYO",
            VersionNum      = 1,
            Reference       = "Payment Run",
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };

        int lineNum = 1;
        foreach (var ci in candidate.Invoices)
        {
            var invoice = _db.Set<APInvoiceEntity>().First(x => x.Id == ci.InvoiceId);
            entity.Invoices.Add(new OPayInvoice
            {
                LineNum = lineNum++, InvoiceId = invoice.Id, InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total, InvoiceBalance = ci.Balance,
                AppliedAmount = ci.Balance, DiscountAmount = 0, WithholdingTax = 0, RemainingBalance = 0,
                InvoiceStatus = "C", Currency = "USD", ExchangeRate = 1,
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
            invoice.Status = "C";
            invoice.UpdatedAt = DateTime.UtcNow;
            _db.Update(invoice);
        }

        using var tx = _db.Database.BeginTransaction();
        try
        {
            _db.Add(entity);
            _db.SaveChanges();
            var je = _outgoingJournal.CreateJournalEntry(entity, vendor!);
            je.BaseDocEntry = entity.Id;
            _db.SaveChanges();
            entity.JournalEntryId = je.Id;
            _db.SaveChanges();
            tx.Commit();
            return (entity.Id, entity.DocNum, entity.Invoices.Count);
        }
        catch { tx.Rollback(); throw; }
    }

    private (int id, string docNum, int closed) CreateIncomingPayment(PaymentRunCandidate candidate, PaymentRunExecuteRequest req)
    {
        var customer = _db.Set<BPEntity>().FirstOrDefault(b => b.Code == candidate.CardCode);

        var entity = new IPay
        {
            DocNum          = NextDocNum("PAY-IN"),
            DocDate         = req.PaymentDate,
            DueDate         = req.PaymentDate,
            CardCode        = candidate.CardCode,
            CardName        = candidate.CardName,
            DocTotal        = candidate.TotalDue,
            AppliedAmount   = candidate.TotalDue,
            UnappliedAmount = 0,
            Currency        = "USD",
            ExchangeRate    = 1,
            PaymentMethod   = req.PaymentMethod,
            BankAccount     = req.BankAccount,
            CheckStatus     = req.PaymentMethod == "Check" ? "Received" : null,
            Status          = "O",
            TransType       = "KPYI",
            VersionNum      = 1,
            Reference       = "Payment Run",
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };

        int lineNum = 1;
        foreach (var ci in candidate.Invoices)
        {
            var invoice = _db.Set<ARInvoiceEntity>().First(x => x.Id == ci.InvoiceId);
            entity.Invoices.Add(new IPayInvoice
            {
                LineNum = lineNum++, InvoiceId = invoice.Id, InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total, InvoiceBalance = ci.Balance,
                AppliedAmount = ci.Balance, RemainingBalance = 0,
                InvoiceStatus = "C", Currency = "USD", ExchangeRate = 1,
                CreatedAt = DateTime.UtcNow, InActive = false,
            });
            invoice.Status = "C";
            invoice.UpdatedAt = DateTime.UtcNow;
            _db.Update(invoice);
        }

        using var tx = _db.Database.BeginTransaction();
        try
        {
            _db.Add(entity);
            _db.SaveChanges();
            var je = _incomingJournal.CreateJournalEntry(entity, customer);
            je.BaseDocEntry = entity.Id;
            _db.SaveChanges();
            entity.JournalEntryId = je.Id;
            _db.SaveChanges();
            tx.Commit();
            return (entity.Id, entity.DocNum, entity.Invoices.Count);
        }
        catch { tx.Rollback(); throw; }
    }

    // Matches IncomingPaymentController/OutgoingPaymentController's own
    // GenerateDocNumber exactly (PAY-IN-YYYY-##### / PAY-OUT-YYYY-#####,
    // 5-digit padding) so a Payment Run document looks identical to one
    // created from the single-payment screen.
    private string NextDocNum(string prefix)
    {
        var year = DateTime.UtcNow.Year;
        var full = $"{prefix}-{year}-";
        IEnumerable<string> existing = prefix == "PAY-IN"
            ? _db.Set<IPay>().Where(p => p.DocNum.StartsWith(full)).Select(p => p.DocNum).ToList()
            : _db.Set<OPay>().Where(p => p.DocNum.StartsWith(full)).Select(p => p.DocNum).ToList();
        var highest = existing.Select(n => int.TryParse(n[full.Length..], out var v) ? v : 0)
            .DefaultIfEmpty(0).Max();
        return $"{full}{highest + 1:D5}";
    }
}
