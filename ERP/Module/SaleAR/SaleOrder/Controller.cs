using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;
using FarmingApi.Services;
using QuestPDF.Fluent;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.SaleOrder;

[ApiController]
[Route("[controller]")]
public class SaleOrderController : ControllerBase
{
    private readonly MyDbContext            _db;
    private readonly IMapper                _mapper;
    private readonly IDocumentNumberService _docNumber;
    private readonly IApprovalGateService   _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public SaleOrderController(MyDbContext db, IMapper mapper, IDocumentNumberService docNumber, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db        = db;
        _mapper    = mapper;
        _docNumber = docNumber;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /SaleOrder ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleOrderListResponse>>(list));
    }

    // ── GET /SaleOrder/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return Ok(_mapper.Map<SaleOrderListResponse>(order));
    }

    // ── POST /SaleOrder ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleOrderListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var order = _mapper.Map<SaleOrder>(dto);

        // Auto-assign the document number (server-side, race-safe)
        try { order.DocNum = _docNumber.Next("SaleOrder"); }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }

        // Auto-fill ItemCode / ItemName from ItemsMaster
        foreach (var line in order.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        _db.Set<SaleOrder>().Add(order);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            _mapper.Map<SaleOrderListResponse>(created));
    }

    // ── POST /SaleOrder/{id}/Post ─── promotes a Draft order to Open. No
    // GL/inventory side effect on a Sale Order — posting is the status
    // transition the entity's Status comment already names but nothing set.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var order = await _db.Set<SaleOrder>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {order.Status}).");

        var gate = await _approvalGate.EvaluateAsync("SaleOrder", order.Id, order.DocNum, order.Total, this.CurrentUserId() ?? 0);
        if (gate.RequiresApproval)
        {
            order.Status    = "P";
            order.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Submitted for approval", docNum = order.DocNum, approvalRequestId = gate.ApprovalRequestId });
        }

        order.Status    = "O";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Sale Order posted", docNum = order.DocNum });
    }

    // ── POST /SaleOrder/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var order = await _db.Set<SaleOrder>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {order.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleOrder", order.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        order.Status    = "O";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Sale Order approved and posted", docNum = order.DocNum });
    }

    // ── POST /SaleOrder/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var order = await _db.Set<SaleOrder>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {order.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleOrder", order.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        order.Status    = "D";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Sale Order rejected", docNum = order.DocNum });
    }

    // ── PUT /SaleOrder/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleOrderUpdateRequest dto)
    {
        var order = await _db.Set<SaleOrder>()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();

        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        _db.RemoveRange(order.Items);
        _mapper.Map(dto, order);

        foreach (var line in order.Items)
        {
            var item = await _db.Set<ItemsMaster>()
                .FirstOrDefaultAsync(x => x.Id == line.ItemId);
            if (item == null)
                return BadRequest($"Item with Id {line.ItemId} not found");

            line.ItemCode = item.ItemCode;
            line.ItemName = item.ItemName;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── GET /SaleOrder/{id}/Pdf ────────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var order = await _db.Set<SaleOrder>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("SaleOrder", order, order.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new SaleOrderPdfDocument(order, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{order.DocNum}.pdf");
    }

    // ── DELETE /SaleOrder/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Set<SaleOrder>().FindAsync(id);
        if (order == null) return NotFound();

        _db.Set<SaleOrder>().Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}