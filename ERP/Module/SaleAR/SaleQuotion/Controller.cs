using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using FarmingApi.Modules.Administration.ApprovalRequest;
using FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster;
using FarmingApi.Modules.Inventory.ItemsMaster;
using QuestPDF.Fluent;
using CompanyEntity = FarmingApi.Modules.Company.Company;

namespace FarmingApi.Modules.SaleAR.SaleQuotion;

[ApiController]
[Route("[controller]")]        // /SaleQuotion  (legacy spelling)
[Route("SaleQuotation")]       // /SaleQuotation (alias used by the Sale-AR UI)
public class SaleQuotionController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;
    private readonly IApprovalGateService _approvalGate;
    private readonly IPrintLayoutRenderService _printLayout;

    public SaleQuotionController(MyDbContext db, IMapper mapper, IApprovalGateService approvalGate, IPrintLayoutRenderService printLayout)
    {
        _db     = db;
        _mapper = mapper;
        _approvalGate = approvalGate; _printLayout = printLayout;
    }

    // ── GET /SaleQuotion ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<SaleQuotionListResponse>>(list));
    }

    // ── GET /SaleQuotion/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (order == null) return NotFound();
        return Ok(_mapper.Map<SaleQuotionListResponse>(order));
    }

    // ── POST /SaleQuotion ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleQuotionListRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate customer
        var customer = await _db.Set<BusinessPartnersMaster>()
            .FirstOrDefaultAsync(x => x.Id == dto.CustomerId);
        if (customer == null)
            return BadRequest($"Customer with Id {dto.CustomerId} not found");

        var order = _mapper.Map<SaleQuotion>(dto);

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

        _db.Set<SaleQuotion>().Add(order);
        await _db.SaveChangesAsync();

        var created = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstAsync(x => x.Id == order.Id);

        return CreatedAtAction(nameof(GetById), new { id = order.Id },
            _mapper.Map<SaleQuotionListResponse>(created));
    }

    // ── POST /SaleQuotion/{id}/Post ─── promotes a Draft quotation to Open.
    // A quotation carries no GL/inventory side effect, so posting is just the
    // status transition — but it's the only door out of Draft, same as SAP B1.
    [HttpPost("{id:int}/Post")]
    public async Task<IActionResult> Post(int id)
    {
        var order = await _db.Set<SaleQuotion>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "D")
            return BadRequest($"Only a Draft document can be posted (current status: {order.Status}).");

        var gate = await _approvalGate.EvaluateAsync("SaleQuotion", order.Id, order.DocNum, order.Total, this.CurrentUserId() ?? 0);
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

        return Ok(new { message = "Quotation posted", docNum = order.DocNum });
    }

    // ── POST /SaleQuotion/{id}/Approve ── approves a Pending document and posts it
    [HttpPost("{id:int}/Approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var order = await _db.Set<SaleQuotion>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "P")
            return BadRequest($"Only a document pending approval can be approved (current status: {order.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleQuotion", order.Id, "Approved", userId.Value, null);
        if (!decision.Success) return BadRequest(decision.Error);

        order.Status    = "O";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Quotation approved and posted", docNum = order.DocNum });
    }

    // ── POST /SaleQuotion/{id}/Reject ── rejects a Pending document back to Draft
    [HttpPost("{id:int}/Reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalDecisionRequest? body)
    {
        var order = await _db.Set<SaleQuotion>().FindAsync(id);
        if (order == null) return NotFound();
        if (order.Status != "P")
            return BadRequest($"Only a document pending approval can be rejected (current status: {order.Status}).");

        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var decision = await _approvalGate.DecideAsync("SaleQuotion", order.Id, "Rejected", userId.Value, body?.Remarks);
        if (!decision.Success) return BadRequest(decision.Error);

        order.Status    = "D";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Quotation rejected", docNum = order.DocNum });
    }

    // ── PUT /SaleQuotion/{id} ───────────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleQuotionUpdateRequest dto)
    {
        var order = await _db.Set<SaleQuotion>()
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

    // ── GET /SaleQuotion/{id}/Pdf ───────────────────────────────────────────
    [HttpGet("{id:int}/Pdf")]
    public async Task<IActionResult> GetPdf(int id)
    {
        var quotation = await _db.Set<SaleQuotion>()
            .Include(x => x.Customer)
            .Include(x => x.Items).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (quotation == null) return NotFound();

        var customHtml = await _printLayout.TryRenderAsync("SaleQuotion", quotation, quotation.Items);
        if (customHtml != null) return Content(customHtml, "text/html");

        var company = await _db.Set<CompanyEntity>()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        var pdfBytes = new SaleQuotionPdfDocument(quotation, company).GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{quotation.DocNum}.pdf");
    }

    // ── DELETE /SaleQuotion/{id} ────────────────────────────────────────────
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var order = await _db.Set<SaleQuotion>().FindAsync(id);
        if (order == null) return NotFound();

        _db.Set<SaleQuotion>().Remove(order);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}