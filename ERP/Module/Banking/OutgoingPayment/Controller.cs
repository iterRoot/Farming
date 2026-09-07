using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using APInvoiceEntity = FarmingApi.Modules.PurchaseAP.APInvoice.APInvoice;
using BPEntity = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

public class OutgoingPaymentController : MyController
{
    private readonly IMapper _mapper;
    private readonly IOutgoingPaymentRepository _repository;
    private readonly MyDbContext _context;
    private readonly IOutgoingPaymentJournalService _journalService;

    public OutgoingPaymentController(
        IOutgoingPaymentRepository repository,
        MyDbContext context,
        IMapper mapper,
        IOutgoingPaymentJournalService journalService)
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
        var results = _mapper.ProjectTo<OutgoingPaymentListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var payment = _repository.GetAll()
            .Include(p => p.Invoices)
            .FirstOrDefault(e => e.Id == id);

        if (payment == null)
            return BadRequest($"Payment not found {id}");

        var result = _mapper.Map<OutgoingPaymentListResponse>(payment);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET OPEN AP INVOICES FOR SUPPLIER
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("OpenInvoices/{cardCode}")]
    public IActionResult GetOpenInvoices(string cardCode)
    {
        var vendor = _context.Set<BPEntity>().FirstOrDefault(b => b.Code == cardCode);
        if (vendor == null) return Ok(new List<object>());

        // Amount already applied to each invoice by earlier (non-void) payments.
        var appliedByInvoice = _context.Set<OutgoingPaymentInvoice>()
            .Where(l => l.OutgoingPayment.Status != "V")
            .GroupBy(l => l.InvoiceId)
            .Select(g => new { InvoiceId = g.Key, Applied = g.Sum(x => x.AppliedAmount + x.DiscountAmount + x.WithholdingTax) })
            .ToDictionary(x => x.InvoiceId, x => x.Applied);

        var today = DateTime.UtcNow.Date;

        var invoices = _context.Set<APInvoiceEntity>()
            .Where(i => i.VendorId == vendor.Id && i.Status == "O")
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
    // CREATE PAYMENT with AP Invoice Allocation
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] OutgoingPaymentListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validation
        if (string.IsNullOrWhiteSpace(request.CardCode))
            return BadRequest("Supplier is required");
        if (request.DocTotal <= 0)
            return BadRequest("Payment amount must be greater than zero");
        if (request.Invoices == null || !request.Invoices.Any())
            return BadRequest("At least one invoice must be selected");

        // Validate total applied amount (including discounts and withholding)
        var totalApplied = request.Invoices.Sum(i => i.AppliedAmount + i.DiscountAmount + i.WithholdingTax);
        if (totalApplied > request.DocTotal + 0.01m)
            return BadRequest($"Applied amount ({totalApplied:N2}) cannot exceed payment amount ({request.DocTotal:N2})");

        // Create payment entity
        var entity = _mapper.Map<OutgoingPayment>(request);
        entity.DocNum = GenerateDocNumber();
        entity.AppliedAmount = totalApplied;
        entity.UnappliedAmount = request.DocTotal - totalApplied;
        entity.Status = "O";
        entity.TransType = "KPYO";
        entity.VersionNum = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        // Get supplier name
        var vendor = _context.Set<BPEntity>().FirstOrDefault(b => b.Code == request.CardCode);
        entity.CardName = vendor?.CardName ?? request.CardCode;

        // Process AP invoice allocations
        int lineNum = 1;
        foreach (var invoiceRequest in request.Invoices)
        {
            var invoice = _context.Set<APInvoiceEntity>()
                .FirstOrDefault(x => x.Id == invoiceRequest.InvoiceId);

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            // Balance = invoice total minus what earlier (non-void) payments applied.
            var alreadyApplied = _context.Set<OutgoingPaymentInvoice>()
                .Where(l => l.InvoiceId == invoice.Id && l.OutgoingPayment.Status != "V")
                .Sum(l => (decimal?)(l.AppliedAmount + l.DiscountAmount + l.WithholdingTax)) ?? 0m;

            decimal invoiceBalance = invoice.Total - alreadyApplied;
            decimal totalPaymentOnInvoice = invoiceRequest.AppliedAmount + invoiceRequest.DiscountAmount + invoiceRequest.WithholdingTax;

            if (totalPaymentOnInvoice > invoiceBalance + 0.01m)
                return BadRequest($"Total payment on invoice {invoice.DocNum} exceeds balance");

            decimal remainingBalance = invoiceBalance - totalPaymentOnInvoice;
            string newInvoiceStatus = remainingBalance <= 0 ? "C" : "O"; // C=Closed, O=Open

            var paymentInvoice = new OutgoingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoice.Id,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total,
                InvoiceBalance = invoiceBalance,
                AppliedAmount = invoiceRequest.AppliedAmount,
                DiscountAmount = invoiceRequest.DiscountAmount,
                WithholdingTax = invoiceRequest.WithholdingTax,
                RemainingBalance = remainingBalance,
                InvoiceStatus = newInvoiceStatus,
                Currency = request.Currency,
                ExchangeRate = request.ExchangeRate,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            entity.Invoices.Add(paymentInvoice);

            // Update the AP invoice status via EF (no raw SQL / SQL-Server syntax).
            invoice.Status = newInvoiceStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Update(invoice);
        }

        // Save the payment (+ invoice status changes) and its Journal Entry in
        // one transaction — DR Accounts Payable (+ Vendor Deposit) / CR Cash-Bank
        // (+ Purchase Discount / Withholding Tax Payable).
        using var tx = _context.Database.BeginTransaction();
        try
        {
            _context.Add(entity);
            _context.SaveChanges();                 // entity.Id is now populated

            var je = _journalService.CreateJournalEntry(entity, vendor);
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
    [AllowAnonymous]
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] OutgoingPaymentUpdateRequest request)
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
        // the balance, so reopen each affected AP invoice.
        foreach (var oldInvoice in payment.Invoices)
        {
            var inv = _context.Set<APInvoiceEntity>().FirstOrDefault(x => x.Id == oldInvoice.InvoiceId);
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
        var totalApplied = request.Invoices.Sum(i => i.AppliedAmount + i.DiscountAmount + i.WithholdingTax);
        if (totalApplied > request.DocTotal + 0.01m)
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
            var invoice = _context.Set<APInvoiceEntity>()
                .FirstOrDefault(x => x.Id == invoiceRequest.InvoiceId);

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            // Applied by other (non-void) payments, excluding this one being edited.
            var alreadyApplied = _context.Set<OutgoingPaymentInvoice>()
                .Where(l => l.InvoiceId == invoice.Id
                            && l.OutgoingPaymentId != payment.Id
                            && l.OutgoingPayment.Status != "V")
                .Sum(l => (decimal?)(l.AppliedAmount + l.DiscountAmount + l.WithholdingTax)) ?? 0m;

            decimal invoiceBalance = invoice.Total - alreadyApplied;
            decimal totalPayment = invoiceRequest.AppliedAmount + invoiceRequest.DiscountAmount + invoiceRequest.WithholdingTax;
            decimal remainingBalance = invoiceBalance - totalPayment;
            string newInvoiceStatus = remainingBalance <= 0 ? "C" : "O";

            var paymentInvoice = new OutgoingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceDueDate = invoice.DueDate ?? invoice.PostingDate ?? DateTime.UtcNow,
                InvoiceTotal = invoice.Total,
                InvoiceBalance = invoiceBalance,
                AppliedAmount = invoiceRequest.AppliedAmount,
                DiscountAmount = invoiceRequest.DiscountAmount,
                WithholdingTax = invoiceRequest.WithholdingTax,
                RemainingBalance = remainingBalance,
                InvoiceStatus = newInvoiceStatus,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            payment.Invoices.Add(paymentInvoice);

            // Update the AP invoice status via EF.
            invoice.Status = newInvoiceStatus;
            invoice.UpdatedAt = DateTime.UtcNow;
            _context.Update(invoice);
        }

        _repository.Update(payment);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // VOID PAYMENT (Reverses all AP invoice allocations)
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
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

        // Reverse all AP invoice allocations — reopen each affected invoice.
        foreach (var paymentInvoice in payment.Invoices)
        {
            var inv = _context.Set<APInvoiceEntity>().FirstOrDefault(x => x.Id == paymentInvoice.InvoiceId);
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
            message = "Payment voided and AP invoice allocations reversed",
            reversedInvoices = payment.Invoices.Count
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
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

        // Reverse invoice allocations — reopen each affected AP invoice.
        foreach (var paymentInvoice in payment.Invoices)
        {
            var inv = _context.Set<APInvoiceEntity>().FirstOrDefault(x => x.Id == paymentInvoice.InvoiceId);
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
    // GET BY SUPPLIER
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("Supplier/{cardCode}")]
    public IActionResult GetBySupplier(string cardCode)
    {
        var payments = _repository.GetAll()
            .Include(p => p.Invoices)
            .Where(p => p.CardCode == cardCode)
            .OrderByDescending(p => p.DocDate)
            .ToList();

        var results = _mapper.Map<List<OutgoingPaymentListResponse>>(payments);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY DATE RANGE
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("DateRange")]
    public IActionResult GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate)
    {
        var payments = _repository.GetAll()
            .Include(p => p.Invoices)
            .Where(p => p.DocDate >= fromDate && p.DocDate <= toDate)
            .OrderByDescending(p => p.DocDate)
            .ToList();

        var results = _mapper.Map<List<OutgoingPaymentListResponse>>(payments);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET PAYMENT SUMMARY BY SUPPLIER
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("Summary/{cardCode}")]
    public IActionResult GetPaymentSummary(string cardCode)
    {
        var summary = _repository.GetAll()
            .Where(p => p.CardCode == cardCode && p.Status != "V")
            .GroupBy(p => p.CardCode)
            .Select(g => new
            {
                CardCode = g.Key,
                TotalPaid = g.Sum(p => p.DocTotal),
                TotalApplied = g.Sum(p => p.AppliedAmount),
                TotalAdvance = g.Sum(p => p.UnappliedAmount),
                PaymentCount = g.Count()
            })
            .FirstOrDefault();

        if (summary == null)
            return Ok(new
            {
                CardCode = cardCode,
                TotalPaid = 0,
                TotalApplied = 0,
                TotalAdvance = 0,
                PaymentCount = 0
            });

        return Ok(summary);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER: Generate Document Number
    // ═══════════════════════════════════════════════════════════════
    private string GenerateDocNumber()
    {
        var year = DateTime.Now.Year;
        var lastPayment = _repository.GetAll()
            .Where(p => p.DocNum.StartsWith($"PAY-OUT-{year}"))
            .OrderByDescending(p => p.Id)
            .FirstOrDefault();

        if (lastPayment == null)
            return $"PAY-OUT-{year}-00001";

        var parts = lastPayment.DocNum.Split('-');
        if (parts.Length >= 4 && int.TryParse(parts[3], out int lastNumber))
        {
            return $"PAY-OUT-{year}-{(lastNumber + 1).ToString("D5")}";
        }

        return $"PAY-OUT-{year}-00001";
    }
}
