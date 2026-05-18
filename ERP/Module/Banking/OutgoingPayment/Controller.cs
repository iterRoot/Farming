using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.OutgoingPayment;

public class OutgoingPaymentController : MyController
{
    private readonly IMapper _mapper;
    private readonly IOutgoingPaymentRepository _repository;
    private readonly MyDbContext _context;

    public OutgoingPaymentController(
        IOutgoingPaymentRepository repository,
        MyDbContext context,
        IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _context = context;
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
    [HttpGet("OpenInvoices/{cardCode}")]
    public IActionResult GetOpenInvoices(string cardCode)
    {
        // Get all open AP invoices for this supplier
        var invoices = _context.Set<dynamic>()
            .FromSqlRaw(@"
                SELECT 
                    Id as InvoiceId,
                    DocNum,
                    DocDate,
                    DueDate,
                    DocTotal,
                    COALESCE(PaidToDate, 0) as PaidToDate,
                    (DocTotal - COALESCE(PaidToDate, 0)) as Balance,
                    DATEDIFF(day, DueDate, GETDATE()) as DaysOverdue,
                    Status
                FROM KPAI
                WHERE CardCode = {0}
                  AND Status = 'O'
                  AND (DocTotal - COALESCE(PaidToDate, 0)) > 0
                  AND DeletedAt IS NULL
                ORDER BY DueDate ASC
            ", cardCode)
            .ToList();

        return Ok(invoices);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE PAYMENT with AP Invoice Allocation
    // ═══════════════════════════════════════════════════════════════
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
        if (totalApplied > request.DocTotal)
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
        var supplier = _context.Set<dynamic>()
            .FromSqlRaw("SELECT CardName FROM KBPM WHERE CardCode = {0}", request.CardCode)
            .FirstOrDefault();
        entity.CardName = supplier?.CardName ?? request.CardCode;

        // Process AP invoice allocations
        int lineNum = 1;
        foreach (var invoiceRequest in request.Invoices)
        {
            // Get invoice details
            var invoice = _context.Set<dynamic>()
                .FromSqlRaw(@"
                    SELECT Id, DocNum, DocDate, DueDate, DocTotal, 
                           COALESCE(PaidToDate, 0) as PaidToDate, Status
                    FROM KPAI 
                    WHERE Id = {0}", invoiceRequest.InvoiceId)
                .FirstOrDefault();

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            decimal invoiceBalance = invoice.DocTotal - invoice.PaidToDate;
            decimal totalPaymentOnInvoice = invoiceRequest.AppliedAmount + invoiceRequest.DiscountAmount + invoiceRequest.WithholdingTax;

            if (totalPaymentOnInvoice > invoiceBalance)
                return BadRequest($"Total payment on invoice {invoice.DocNum} exceeds balance");

            decimal remainingBalance = invoiceBalance - totalPaymentOnInvoice;
            string newInvoiceStatus = remainingBalance == 0 ? "C" : "O"; // C=Closed, O=Open

            // Create payment invoice line
            var paymentInvoice = new OutgoingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.DocDate,
                InvoiceDueDate = invoice.DueDate,
                InvoiceTotal = invoice.DocTotal,
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

            // ⭐ UPDATE AP INVOICE STATUS AND PAID AMOUNT
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KPAI 
                SET PaidToDate = COALESCE(PaidToDate, 0) + {0},
                    Status = {1},
                    UpdatedAt = GETDATE()
                WHERE Id = {2}
            ", totalPaymentOnInvoice, newInvoiceStatus, invoiceRequest.InvoiceId);
        }

        _repository.Add(entity);
        _repository.Commit();

        // TODO: Auto-create Journal Entry
        // DR: Accounts Payable
        // CR: Cash/Bank

        return Ok(new
        {
            message = "Payment saved successfully",
            id = entity.Id,
            docNum = entity.DocNum,
            appliedAmount = entity.AppliedAmount,
            unappliedAmount = entity.UnappliedAmount,
            invoicesUpdated = entity.Invoices.Count,
            closedInvoices = entity.Invoices.Count(i => i.InvoiceStatus == "C")
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE PAYMENT
    // ═══════════════════════════════════════════════════════════════
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

        // Reverse old invoice allocations
        foreach (var oldInvoice in payment.Invoices)
        {
            decimal totalPaid = oldInvoice.AppliedAmount + oldInvoice.DiscountAmount + oldInvoice.WithholdingTax;
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KPAI 
                SET PaidToDate = PaidToDate - {0},
                    Status = CASE 
                        WHEN (DocTotal - (PaidToDate - {0})) > 0 THEN 'O' 
                        ELSE 'C' 
                    END,
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", totalPaid, oldInvoice.InvoiceId);
        }

        // Clear old lines
        payment.Invoices.Clear();

        // Validate new allocations
        var totalApplied = request.Invoices.Sum(i => i.AppliedAmount + i.DiscountAmount + i.WithholdingTax);
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
            var invoice = _context.Set<dynamic>()
                .FromSqlRaw("SELECT Id, DocNum, DocDate, DueDate, DocTotal, COALESCE(PaidToDate, 0) as PaidToDate FROM KPAI WHERE Id = {0}", invoiceRequest.InvoiceId)
                .FirstOrDefault();

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            decimal invoiceBalance = invoice.DocTotal - invoice.PaidToDate;
            decimal totalPayment = invoiceRequest.AppliedAmount + invoiceRequest.DiscountAmount + invoiceRequest.WithholdingTax;
            decimal remainingBalance = invoiceBalance - totalPayment;
            string newInvoiceStatus = remainingBalance == 0 ? "C" : "O";

            var paymentInvoice = new OutgoingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.DocDate,
                InvoiceDueDate = invoice.DueDate,
                InvoiceTotal = invoice.DocTotal,
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

            // Update invoice
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KPAI 
                SET PaidToDate = COALESCE(PaidToDate, 0) + {0},
                    Status = {1},
                    UpdatedAt = GETDATE()
                WHERE Id = {2}
            ", totalPayment, newInvoiceStatus, invoiceRequest.InvoiceId);
        }

        _repository.Update(payment);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // VOID PAYMENT (Reverses all AP invoice allocations)
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

        // Reverse all AP invoice allocations
        foreach (var paymentInvoice in payment.Invoices)
        {
            decimal totalPaid = paymentInvoice.AppliedAmount + paymentInvoice.DiscountAmount + paymentInvoice.WithholdingTax;
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KPAI 
                SET PaidToDate = PaidToDate - {0},
                    Status = CASE 
                        WHEN (DocTotal - (PaidToDate - {0})) > 0 THEN 'O' 
                        ELSE 'C' 
                    END,
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", totalPaid, paymentInvoice.InvoiceId);
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

        // Reverse invoice allocations
        foreach (var paymentInvoice in payment.Invoices)
        {
            decimal totalPaid = paymentInvoice.AppliedAmount + paymentInvoice.DiscountAmount + paymentInvoice.WithholdingTax;
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KPAI 
                SET PaidToDate = PaidToDate - {0},
                    Status = 'O',
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", totalPaid, paymentInvoice.InvoiceId);
        }

        payment.DeletedAt = DateTime.UtcNow;
        _repository.Remove(payment);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY SUPPLIER
    // ═══════════════════════════════════════════════════════════════
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