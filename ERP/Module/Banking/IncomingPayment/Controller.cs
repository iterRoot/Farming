using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using ARInvoiceEntity = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Banking.IncomingPayment;

public class IncomingPaymentController : MyController
{
    private readonly IMapper _mapper;
    private readonly IIncomingPaymentRepository _repository;
    private readonly MyDbContext _context;
    private readonly IIncomingPaymentJournalService _journalService;

    public IncomingPaymentController(
        IIncomingPaymentRepository repository,
        MyDbContext context,
        IMapper mapper,
        IIncomingPaymentJournalService journalService)
    {
        _mapper = mapper;
        _repository = repository;
        _context = context;
        _journalService = journalService;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll().Include(p => p.Invoices);
        var results = _mapper.ProjectTo<IncomingPaymentListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var payment = _repository.GetAll()
            .Include(p => p.Invoices)
            .FirstOrDefault(e => e.Id == id);

        if (payment == null)
            return BadRequest($"Payment not found {id}");

        var result = _mapper.Map<IncomingPaymentListResponse>(payment);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET OPEN INVOICES FOR CUSTOMER
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("OpenInvoices/{cardCode}")]
    public IActionResult GetOpenInvoices(string cardCode)
    {
        // Resolve the customer by BP Code (the "CardCode").
        var customer = _context.Set<BPEntity>().FirstOrDefault(b => b.Code == cardCode);
        if (customer == null) return Ok(new List<object>());

        // Amount already applied to each invoice by earlier (non-void) payments.
        var appliedByInvoice = _context.Set<IncomingPaymentInvoice>()
            .Where(l => l.IncomingPayment.Status != "V")
            .GroupBy(l => l.InvoiceId)
            .Select(g => new { InvoiceId = g.Key, Applied = g.Sum(x => x.AppliedAmount) })
            .ToDictionary(x => x.InvoiceId, x => x.Applied);

        var today = DateTime.UtcNow.Date;

        var invoices = _context.Set<ARInvoiceEntity>()
            .Where(i => i.CustomerId == customer.Id && i.Status == "O")
            .OrderBy(i => i.DueDate)
            .ToList()
            .Select(i =>
            {
                var paid = appliedByInvoice.TryGetValue(i.Id, out var p) ? p : 0m;
                var balance = i.Total - paid;
                var due = i.DueDate ?? i.PostingDate;
                return new
                {
                    InvoiceId = i.Id,
                    DocNum = i.DocNum,
                    DocDate = i.PostingDate,
                    DueDate = i.DueDate,
                    DocTotal = i.Total,
                    PaidToDate = paid,
                    Balance = balance,
                    DaysOverdue = due.HasValue ? (int)(today - due.Value.Date).TotalDays : 0,
                    Status = i.Status,
                };
            })
            .Where(x => x.Balance > 0)
            .ToList();

        return Ok(invoices);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE PAYMENT with Invoice Allocation
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] IncomingPaymentListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Build the allocation list from either the invoice array (invoice-
        // allocation style) or the grid lines (SAP-style Create screen).
        var allocations = (request.Invoices != null && request.Invoices.Any())
            ? request.Invoices
                .Where(i => i.InvoiceId > 0 && i.AppliedAmount > 0)
                .Select(i => new IncomingPaymentInvoiceRequest
                { InvoiceId = i.InvoiceId, AppliedAmount = i.AppliedAmount })
                .ToList()
            : (request.Lines ?? new())
                .Where(l => (l.DocEntry ?? 0) > 0
                            && (l.PaymentAmt - l.DiscountAmt - l.WhtAmount) > 0)
                .Select(l => new IncomingPaymentInvoiceRequest
                {
                    InvoiceId = l.DocEntry!.Value,
                    AppliedAmount = l.PaymentAmt - l.DiscountAmt - l.WhtAmount,
                })
                .ToList();

        // Payment total: use the value sent, otherwise derive it from the lines.
        decimal docTotal = request.DocTotal > 0
            ? request.DocTotal
            : (request.Lines?.Sum(l => l.PaymentAmt - l.DiscountAmt - l.WhtAmount)
               ?? allocations.Sum(a => a.AppliedAmount));

        // Validation
        if (docTotal <= 0)
            return BadRequest("Payment amount must be greater than zero");
        if (!allocations.Any())
            return BadRequest("Select at least one document to pay");

        var totalApplied = allocations.Sum(i => i.AppliedAmount);
        if (totalApplied > docTotal + 0.01m)
            return BadRequest($"Applied amount ({totalApplied:N2}) cannot exceed payment amount ({docTotal:N2})");

        // Create payment entity
        var entity = _mapper.Map<IncomingPayment>(request);
        entity.DocNum = GenerateDocNumber();
        entity.CardCode = request.CardCode ?? "";
        entity.DocTotal = docTotal;
        entity.AppliedAmount = totalApplied;
        entity.UnappliedAmount = docTotal - totalApplied;
        // Header aliases sent by the SAP-style form.
        entity.Reference   = request.Reference   ?? request.RefNo;
        entity.Memo        = request.Memo        ?? request.Remarks;
        entity.CheckNumber = request.CheckNumber ?? request.ChequeNo;
        entity.CheckDate   = request.CheckDate   ?? request.ChequeDate;
        entity.BankName    = request.BankName;
        entity.Status = "O";
        entity.TransType = "KPYI";
        entity.VersionNum = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        // Get customer name
        var customer = _context.Set<BPEntity>()
            .FirstOrDefault(b => b.Code == request.CardCode);
        entity.CardName = request.CardName ?? customer?.CardName ?? request.CardCode ?? "";

        // Process invoice allocations
        int lineNum = 1;
        foreach (var invoiceRequest in allocations)
        {
            var invoice = _context.Set<ARInvoiceEntity>()
                .FirstOrDefault(x => x.Id == invoiceRequest.InvoiceId);

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            // Balance = invoice total minus what earlier (non-void) payments applied.
            var alreadyApplied = _context.Set<IncomingPaymentInvoice>()
                .Where(l => l.InvoiceId == invoice.Id && l.IncomingPayment.Status != "V")
                .Sum(l => (decimal?)l.AppliedAmount) ?? 0m;

            decimal invoiceBalance = invoice.Total - alreadyApplied;

            if (invoiceRequest.AppliedAmount > invoiceBalance)
                return BadRequest($"Applied amount on invoice {invoice.DocNum} exceeds balance");

            decimal remainingBalance = invoiceBalance - invoiceRequest.AppliedAmount;
            string newInvoiceStatus = remainingBalance <= 0 ? "C" : "O"; // C=Closed, O=Open

            // Create payment invoice line
            var paymentInvoice = new IncomingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoice.Id,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total,
                InvoiceBalance = invoiceBalance,
                AppliedAmount = invoiceRequest.AppliedAmount,
                RemainingBalance = remainingBalance,
                InvoiceStatus = newInvoiceStatus,
                Currency = request.Currency,
                ExchangeRate = request.ExchangeRate,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            entity.Invoices.Add(paymentInvoice);

            // Update the AR invoice status via EF (no raw SQL / SQL-Server syntax).
            invoice.Status = newInvoiceStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Update(invoice);
        }

        // Save the payment (+ invoice status changes) and its Journal Entry in
        // one transaction — DR Cash/Bank, CR Accounts Receivable.
        using var tx = _context.Database.BeginTransaction();
        try
        {
            _context.Add(entity);
            _context.SaveChanges();                 // entity.Id is now populated

            var je = _journalService.CreateJournalEntry(entity, customer);
            je.BaseDocEntry = entity.Id;
            _context.SaveChanges();                 // je.Id now populated

            entity.JournalEntryId = je.Id;
            _context.SaveChanges();

            tx.Commit();

            return Ok(new
            {
                message = "Payment saved successfully",
                id = entity.Id,
                docNum = entity.DocNum,
                journalEntryId = je.Id,
                journalNo = je.JrnlNo,
                appliedAmount = entity.AppliedAmount,
                unappliedAmount = entity.UnappliedAmount,
                invoicesUpdated = entity.Invoices.Count,
                closedInvoices = entity.Invoices.Count(i => i.InvoiceStatus == "C")
            });
        }
        catch (Exception ex)
        {
            tx.Rollback();
            // Surface the real reason (usually missing GL account setup).
            return BadRequest(ex.InnerException?.Message ?? ex.Message);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE PAYMENT
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] IncomingPaymentUpdateRequest request)
    {
        var payment = _repository.GetAll()
            .Include(p => p.Invoices)
            .FirstOrDefault(e => e.Id == id);

        if (payment == null)
            return NotFound($"Payment not found: {id}");

        if (payment.Status == "C")
            return BadRequest("Cannot update closed payment");
        if (payment.Status == "V")
            return BadRequest("Cannot update voided payment");

        // Reverse old invoice allocations — removing this payment's lines frees
        // the balance, so reopen each affected AR invoice.
        foreach (var oldInvoice in payment.Invoices)
        {
            var inv = _context.Set<ARInvoiceEntity>().FirstOrDefault(x => x.Id == oldInvoice.InvoiceId);
            if (inv != null)
            {
                inv.Status = "O";
                inv.UpdatedAt = DateTime.UtcNow;
                _context.Update(inv);
            }
        }

        // Clear old lines
        payment.Invoices.Clear();

        // Validate new allocations
        var totalApplied = request.Invoices.Sum(i => i.AppliedAmount);
        if (totalApplied > request.DocTotal)
            return BadRequest($"Applied amount ({totalApplied:N2}) cannot exceed payment amount ({request.DocTotal:N2})");

        // Update header
        payment.DocDate = request.DocDate;
        payment.DueDate = request.DueDate;
        payment.DocTotal = request.DocTotal;
        payment.AppliedAmount = totalApplied;
        payment.UnappliedAmount = request.DocTotal - totalApplied;
        payment.Currency = request.Currency;
        payment.ExchangeRate = request.ExchangeRate;
        payment.PaymentMethod = request.PaymentMethod;
        payment.CheckNumber = request.CheckNumber;
        payment.CheckDate = request.CheckDate;
        payment.BankAccount = request.BankAccount;
        payment.Reference = request.Reference;
        payment.Memo = request.Memo;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.VersionNum += 1;

        // Apply new allocations
        int lineNum = 1;
        foreach (var invoiceRequest in request.Invoices)
        {
            var invoice = _context.Set<ARInvoiceEntity>()
                .FirstOrDefault(x => x.Id == invoiceRequest.InvoiceId);

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            // Applied by other (non-void) payments, excluding this one being edited.
            var alreadyApplied = _context.Set<IncomingPaymentInvoice>()
                .Where(l => l.InvoiceId == invoice.Id
                            && l.IncomingPaymentId != payment.Id
                            && l.IncomingPayment.Status != "V")
                .Sum(l => (decimal?)l.AppliedAmount) ?? 0m;

            decimal invoiceBalance = invoice.Total - alreadyApplied;
            decimal remainingBalance = invoiceBalance - invoiceRequest.AppliedAmount;
            string newInvoiceStatus = remainingBalance <= 0 ? "C" : "O";

            var paymentInvoice = new IncomingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total,
                InvoiceBalance = invoiceBalance,
                AppliedAmount = invoiceRequest.AppliedAmount,
                RemainingBalance = remainingBalance,
                InvoiceStatus = newInvoiceStatus,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            payment.Invoices.Add(paymentInvoice);

            // Update the AR invoice status via EF.
            invoice.Status = newInvoiceStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Update(invoice);
        }

        _repository.Update(payment);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // VOID PAYMENT (Reverses all invoice allocations)
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id}/Void")]
    public IActionResult Void(int id)
    {
        var payment = _repository.GetAll()
            .Include(p => p.Invoices)
            .FirstOrDefault(e => e.Id == id);

        if (payment == null)
            return NotFound($"Payment not found: {id}");

        if (payment.Status == "V")
            return BadRequest("Payment is already voided");

        // Reverse all invoice allocations — reopen each affected AR invoice.
        foreach (var paymentInvoice in payment.Invoices)
        {
            var inv = _context.Set<ARInvoiceEntity>().FirstOrDefault(x => x.Id == paymentInvoice.InvoiceId);
            if (inv != null)
            {
                inv.Status = "O";
                inv.UpdatedAt = DateTime.UtcNow;
                _context.Update(inv);
            }
        }

        payment.Status = "V";
        payment.UpdatedAt = DateTime.UtcNow;

        _repository.Update(payment);
        _repository.Commit();

        return Ok(new
        {
            message = "Payment voided and invoice allocations reversed",
            reversedInvoices = payment.Invoices.Count
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var payment = _repository.GetAll()
            .Include(p => p.Invoices)
            .FirstOrDefault(e => e.Id == id);

        if (payment == null)
            return BadRequest($"Payment not found {id}");

        if (payment.Status == "C")
            return BadRequest("Cannot delete closed payment. Use Void instead.");

        // Reverse invoice allocations — reopen each affected AR invoice.
        foreach (var paymentInvoice in payment.Invoices)
        {
            var inv = _context.Set<ARInvoiceEntity>().FirstOrDefault(x => x.Id == paymentInvoice.InvoiceId);
            if (inv != null)
            {
                inv.Status = "O";
                inv.UpdatedAt = DateTime.UtcNow;
                _context.Update(inv);
            }
        }

        payment.DeletedAt = DateTime.UtcNow;
        _repository.Remove(payment);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY CUSTOMER
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Customer/{cardCode}")]
    public IActionResult GetByCustomer(string cardCode)
    {
        var payments = _repository.GetAll()
            .Include(p => p.Invoices)
            .Where(p => p.CardCode == cardCode)
            .OrderByDescending(p => p.DocDate)
            .ToList();

        var results = _mapper.Map<List<IncomingPaymentListResponse>>(payments);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER: Generate Document Number
    // ═══════════════════════════════════════════════════════════════
    private string GenerateDocNumber()
    {
        var year = DateTime.Now.Year;
        var lastPayment = _repository.GetAll()
            .Where(p => p.DocNum.StartsWith($"PAY-IN-{year}"))
            .OrderByDescending(p => p.Id)
            .FirstOrDefault();

        if (lastPayment == null)
            return $"PAY-IN-{year}-00001";

        var parts = lastPayment.DocNum.Split('-');
        if (parts.Length >= 4 && int.TryParse(parts[3], out int lastNumber))
        {
            return $"PAY-IN-{year}-{(lastNumber + 1).ToString("D5")}";
        }

        return $"PAY-IN-{year}-00001";
    }
}