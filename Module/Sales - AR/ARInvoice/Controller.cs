using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Master.BusinessPartner;

namespace FarmingApi.Modules.SaleAR.ARInvoice;

public class ARInvoiceController : MyController
{
    private readonly IMapper _mapper;
    private readonly IARInvoiceRepository _repository;
    private readonly IBusinessPartnerRepository _bpRepository;

    public ARInvoiceController(
        IARInvoiceRepository repository,
        IBusinessPartnerRepository bpRepository,
        IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _bpRepository = bpRepository;
    }

    // ─────────────────────────────────────────
    // GET /ar-invoices
    // ─────────────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var query = _repository.GetAll()
            .Include(x => x.BusinessPartner);

        var results = _mapper.ProjectTo<ARInvoiceListResponse>(query).ToList();
        return Ok(results);
    }

    // ─────────────────────────────────────────
    // GET /ar-invoices/{id}
    // ─────────────────────────────────────────
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetAll()
            .Include(x => x.BusinessPartner)
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = $"AR Invoice not found: {id}" });

        var result = _mapper.Map<ARInvoiceDetailResponse>(item);
        return Ok(result);
    }

    // ─────────────────────────────────────────
    // POST /ar-invoices
    // ─────────────────────────────────────────
    [HttpPost]
    public IActionResult Create([FromBody] ARInvoiceCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Validate Business Partner exists
        var bp = _bpRepository.GetSingle(x => x.Id == request.BusinessPartnerId);
        if (bp == null)
            return BadRequest(new { message = $"Business Partner not found: {request.BusinessPartnerId}" });

        // 2. Validate at least one line
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest(new { message = "Invoice must have at least one line item." });

        // 3. Map request → entity
        var entity = _mapper.Map<ARInvoice>(request);

        // 4. Snapshot BP name + set defaults
        entity.BPName = $"{bp.FirstName} {bp.LastName}".Trim();
        entity.DocNumber = GenerateDocNumber();
        entity.Status = InvoiceStatus.Open;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        // 5. Calculate line totals
        foreach (var line in entity.Lines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100);
            line.TaxAmount = line.LineTotal * line.VatRate;
        }

        // 6. Calculate header totals
        CalculateTotals(entity);

        _repository.Add(entity);
        _repository.Commit();

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, new
        {
            message = "AR Invoice created successfully.",
            id = entity.Id,
            docNumber = entity.DocNumber
        });
    }

    // ─────────────────────────────────────────
    // PUT /ar-invoices/{id}
    // ─────────────────────────────────────────
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] ARInvoiceUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = _repository.GetAll()
            .Include(x => x.Lines)
            .FirstOrDefault(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = $"AR Invoice not found: {id}" });

        // Only Open invoices can be edited
        if (item.Status != InvoiceStatus.Open)
            return BadRequest(new { message = $"Cannot edit invoice with status: {item.Status}" });

        // Validate lines
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest(new { message = "Invoice must have at least one line item." });

        // Map updated fields (BP, DocNumber, Status are ignored in mapper)
        _mapper.Map(request, item);

        // Replace lines
        item.Lines.Clear();
        var newLines = _mapper.Map<List<ARInvoiceLine>>(request.Lines);
        foreach (var line in newLines)
        {
            line.LineTotal = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100);
            line.TaxAmount = line.LineTotal * line.VatRate;
            item.Lines.Add(line);
        }

        // Recalculate totals
        CalculateTotals(item);

        item.UpdatedAt = DateTime.UtcNow;

        _repository.Update(item);
        _repository.Commit();

        return NoContent();
    }

    // ─────────────────────────────────────────
    // PATCH /ar-invoices/{id}/cancel
    // ─────────────────────────────────────────
    [HttpPatch("{id:int}/cancel")]
    public IActionResult Cancel(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = $"AR Invoice not found: {id}" });

        if (item.Status == InvoiceStatus.Cancelled)
            return BadRequest(new { message = "Invoice is already cancelled." });

        if (item.Status == InvoiceStatus.Closed)
            return BadRequest(new { message = "Cannot cancel a closed invoice." });

        item.Status = InvoiceStatus.Cancelled;
        item.UpdatedAt = DateTime.UtcNow;

        _repository.Update(item);
        _repository.Commit();

        return Ok(new { message = $"Invoice {item.DocNumber} cancelled." });
    }

    // ─────────────────────────────────────────
    // DELETE /ar-invoices/{id}
    // ─────────────────────────────────────────
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(x => x.Id == id);

        if (item == null)
            return NotFound(new { message = $"AR Invoice not found: {id}" });

        if (item.Status == InvoiceStatus.Closed)
            return BadRequest(new { message = "Cannot delete a closed invoice. Cancel it first." });

        item.DeletedAt = DateTime.UtcNow;
        _repository.Remove(item);
        _repository.Commit();

        return NoContent();
    }

    // ─────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────
    private static void CalculateTotals(ARInvoice invoice)
    {
        invoice.TotalBeforeDiscount = invoice.Lines.Sum(l => l.LineTotal);
        invoice.DiscountAmount      = invoice.TotalBeforeDiscount * (invoice.DiscountPct / 100);
        invoice.TaxTotal            = invoice.Lines.Sum(l => l.TaxAmount);
        invoice.GrandTotal          = invoice.TotalBeforeDiscount
                                      - invoice.DiscountAmount
                                      + invoice.TaxTotal
                                      + invoice.Rounding;
        invoice.BalanceDue          = invoice.GrandTotal - invoice.AppliedAmount;
    }

    private string GenerateDocNumber()
    {
        var last = _repository.GetAll()
            .OrderByDescending(x => x.Id)
            .Select(x => x.DocNumber)
            .FirstOrDefault();

        if (last == null) return "1100000001";

        return int.TryParse(last, out var num)
            ? (num + 1).ToString()
            : "1100000001";
    }
}