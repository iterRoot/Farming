using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Banking.IncomingPayment;

public class IncomingPaymentController : MyController
{
    private readonly IMapper _mapper;
    private readonly IIncomingPaymentRepository _repository;
    private readonly MyDbContext _context;

    public IncomingPaymentController(
        IIncomingPaymentRepository repository,
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
        // Get all open AR invoices for this customer
        var invoices = _context.Set<dynamic>() // Replace with actual KARI entity
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
                FROM KARI
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
    // CREATE PAYMENT with Invoice Allocation
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] IncomingPaymentListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validation
        if (string.IsNullOrWhiteSpace(request.CardCode))
            return BadRequest("Customer is required");
        if (request.DocTotal <= 0)
            return BadRequest("Payment amount must be greater than zero");
        if (request.Invoices == null || !request.Invoices.Any())
            return BadRequest("At least one invoice must be selected");

        // Validate total applied amount
        var totalApplied = request.Invoices.Sum(i => i.AppliedAmount);
        if (totalApplied > request.DocTotal)
            return BadRequest($"Applied amount ({totalApplied:N2}) cannot exceed payment amount ({request.DocTotal:N2})");

        // Create payment entity
        var entity = _mapper.Map<IncomingPayment>(request);
        entity.DocNum = GenerateDocNumber();
        entity.AppliedAmount = totalApplied;
        entity.UnappliedAmount = request.DocTotal - totalApplied;
        entity.Status = "O";
        entity.TransType = "KPYI";
        entity.VersionNum = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        // Get customer name
        var customer = _context.Set<dynamic>()
            .FromSqlRaw("SELECT CardName FROM KBPM WHERE CardCode = {0}", request.CardCode)
            .FirstOrDefault();
        entity.CardName = customer?.CardName ?? request.CardCode;

        // Process invoice allocations
        int lineNum = 1;
        foreach (var invoiceRequest in request.Invoices)
        {
            // Get invoice details
            var invoice = _context.Set<dynamic>()
                .FromSqlRaw(@"
                    SELECT Id, DocNum, DocDate, DueDate, DocTotal, 
                           COALESCE(PaidToDate, 0) as PaidToDate, Status
                    FROM KARI 
                    WHERE Id = {0}", invoiceRequest.InvoiceId)
                .FirstOrDefault();

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            decimal invoiceBalance = invoice.DocTotal - invoice.PaidToDate;

            if (invoiceRequest.AppliedAmount > invoiceBalance)
                return BadRequest($"Applied amount on invoice {invoice.DocNum} exceeds balance");

            decimal remainingBalance = invoiceBalance - invoiceRequest.AppliedAmount;
            string newInvoiceStatus = remainingBalance == 0 ? "C" : "O"; // C=Closed, O=Open

            // Create payment invoice line
            var paymentInvoice = new IncomingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.DocDate,
                InvoiceDueDate = invoice.DueDate,
                InvoiceTotal = invoice.DocTotal,
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

            // ⭐ UPDATE INVOICE STATUS AND PAID AMOUNT
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KARI 
                SET PaidToDate = COALESCE(PaidToDate, 0) + {0},
                    Status = {1},
                    UpdatedAt = GETDATE()
                WHERE Id = {2}
            ", invoiceRequest.AppliedAmount, newInvoiceStatus, invoiceRequest.InvoiceId);
        }

        _repository.Add(entity);
        _repository.Commit();

        // TODO: Auto-create Journal Entry
        // DR: Cash/Bank
        // CR: Accounts Receivable

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

        // Reverse old invoice allocations
        foreach (var oldInvoice in payment.Invoices)
        {
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KARI 
                SET PaidToDate = PaidToDate - {0},
                    Status = CASE 
                        WHEN (DocTotal - (PaidToDate - {0})) > 0 THEN 'O' 
                        ELSE 'C' 
                    END,
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", oldInvoice.AppliedAmount, oldInvoice.InvoiceId);
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
            var invoice = _context.Set<dynamic>()
                .FromSqlRaw("SELECT Id, DocNum, DocDate, DueDate, DocTotal, COALESCE(PaidToDate, 0) as PaidToDate FROM KARI WHERE Id = {0}", invoiceRequest.InvoiceId)
                .FirstOrDefault();

            if (invoice == null)
                return BadRequest($"Invoice {invoiceRequest.InvoiceId} not found");

            decimal invoiceBalance = invoice.DocTotal - invoice.PaidToDate;
            decimal remainingBalance = invoiceBalance - invoiceRequest.AppliedAmount;
            string newInvoiceStatus = remainingBalance == 0 ? "C" : "O";

            var paymentInvoice = new IncomingPaymentInvoice
            {
                LineNum = lineNum++,
                InvoiceId = invoiceRequest.InvoiceId,
                InvoiceDocNum = invoice.DocNum,
                InvoiceDocDate = invoice.DocDate,
                InvoiceDueDate = invoice.DueDate,
                InvoiceTotal = invoice.DocTotal,
                InvoiceBalance = invoiceBalance,
                AppliedAmount = invoiceRequest.AppliedAmount,
                RemainingBalance = remainingBalance,
                InvoiceStatus = newInvoiceStatus,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            payment.Invoices.Add(paymentInvoice);

            // Update invoice
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KARI 
                SET PaidToDate = COALESCE(PaidToDate, 0) + {0},
                    Status = {1},
                    UpdatedAt = GETDATE()
                WHERE Id = {2}
            ", invoiceRequest.AppliedAmount, newInvoiceStatus, invoiceRequest.InvoiceId);
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

        // Reverse all invoice allocations
        foreach (var paymentInvoice in payment.Invoices)
        {
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KARI 
                SET PaidToDate = PaidToDate - {0},
                    Status = CASE 
                        WHEN (DocTotal - (PaidToDate - {0})) > 0 THEN 'O' 
                        ELSE 'C' 
                    END,
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", paymentInvoice.AppliedAmount, paymentInvoice.InvoiceId);
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

        // Reverse invoice allocations
        foreach (var paymentInvoice in payment.Invoices)
        {
            _context.Database.ExecuteSqlRaw(@"
                UPDATE KARI 
                SET PaidToDate = PaidToDate - {0},
                    Status = 'O',
                    UpdatedAt = GETDATE()
                WHERE Id = {1}
            ", paymentInvoice.AppliedAmount, paymentInvoice.InvoiceId);
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