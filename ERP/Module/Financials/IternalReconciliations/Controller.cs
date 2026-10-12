using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using ARInvoiceEntity     = FarmingApi.Modules.SaleAR.ARInvoice.ARInvoice;
using ARCreditNoteEntity  = FarmingApi.Modules.SaleAR.ARCreditNote.ARCreditNote;
using APInvoiceEntity     = FarmingApi.Modules.PurchaseAP.APInvoice.APInvoice;
using APCreditNoteEntity  = FarmingApi.Modules.PurchaseAP.APCreditNote.APCreditNote;
using IncomingPaymentEntity = FarmingApi.Modules.Banking.IncomingPayment.IncomingPayment;
using OutgoingPaymentEntity = FarmingApi.Modules.Banking.OutgoingPayment.OutgoingPayment;
using BPEntity            = FarmingApi.Modules.BusinessPartners.BusinessPartnersMaster.BusinessPartnersMaster;

namespace FarmingApi.Modules.Financials.IternalReconciliations;

// ══════════════════════════════════════════════════════════════════════════════
// Same mechanics as BankReconciliationController — this reconciles a business
// partner's own AR/AP subledger against itself (pairing an invoice against the
// credit note or payment that settles it) rather than the book against an
// external bank statement, so there is no separate statement-balance side and
// Close never requires the difference to hit zero (a BP realistically carries
// a genuine open balance; Close here just means "reviewed for this period").
// ══════════════════════════════════════════════════════════════════════════════
[ApiController]
[Route("[controller]")]
public class BPReconciliationController : ControllerBase
{
    private readonly MyDbContext _db;
    private readonly IMapper     _mapper;

    public BPReconciliationController(MyDbContext db, IMapper mapper)
    {
        _db     = db;
        _mapper = mapper;
    }

    // ── GET /BPReconciliation ────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? partnerId = null, [FromQuery] string? status = null)
    {
        var query = _db.Set<BPReconciliation>().Include(b => b.Lines).AsQueryable();
        if (partnerId.HasValue) query = query.Where(b => b.PartnerId == partnerId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(b => b.Status == status);

        var list = await query.OrderByDescending(b => b.CreatedAt).ToListAsync();
        return Ok(_mapper.Map<List<BPReconciliationResponse>>(list));
    }

    // ── GET /BPReconciliation/{id} ───────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var br = await Load(id);
        if (br == null) return NotFound();
        return Ok(_mapper.Map<BPReconciliationResponse>(br));
    }

    // ── POST /BPReconciliation ───────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BPReconciliationRequest dto)
    {
        if (dto.StartDate >= dto.EndDate) return BadRequest("End Date must be after Start Date.");

        var partner = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == dto.PartnerId);
        if (partner == null) return BadRequest($"Business partner {dto.PartnerId} not found.");

        var entity = new BPReconciliation
        {
            ReconcNo       = await GenerateReconcNo(),
            PartnerId      = dto.PartnerId,
            PartnerCode    = partner.Code,
            PartnerName    = partner.CardName,
            Currency       = dto.Currency,
            StartDate      = dto.StartDate,
            EndDate        = dto.EndDate,
            Status         = "O",
            OpeningBalance = dto.OpeningBalance,
            Remarks        = dto.Remarks,
            VersionNum     = 1,
            CreatedAt      = DateTime.UtcNow,
            InActive       = false,
        };

        int lineNum = 1;
        decimal runningBal = dto.OpeningBalance;
        foreach (var lr in dto.Lines.OrderBy(l => l.TransDate))
        {
            var line = _mapper.Map<BPReconciliationLine>(lr);
            line.LineNum   = lineNum++;
            runningBal    += line.Debit - line.Credit;
            line.Balance   = runningBal;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            entity.Lines.Add(line);
        }

        RecalculateTotals(entity);

        _db.Set<BPReconciliation>().Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new { message = "BP Reconciliation created", id = entity.Id, reconcNo = entity.ReconcNo, closingBalance = entity.ClosingBalance });
    }

    // ── PUT /BPReconciliation/{id} ───────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] BPReconciliationUpdateRequest dto)
    {
        var br = await Load(id);
        if (br == null) return NotFound();
        if (br.Status == "C") return BadRequest("Cannot edit a Closed reconciliation.");

        br.StartDate      = dto.StartDate;
        br.EndDate        = dto.EndDate;
        br.OpeningBalance = dto.OpeningBalance;
        br.Remarks        = dto.Remarks;
        br.UpdatedAt      = DateTime.UtcNow;
        br.VersionNum    += 1;

        _db.RemoveRange(br.Lines);
        br.Lines.Clear();

        int lineNum = 1;
        decimal runningBal = dto.OpeningBalance;
        foreach (var lr in dto.Lines.OrderBy(l => l.TransDate))
        {
            var line = _mapper.Map<BPReconciliationLine>(lr);
            line.LineNum   = lineNum++;
            runningBal    += line.Debit - line.Credit;
            line.Balance   = runningBal;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            br.Lines.Add(line);
        }

        RecalculateTotals(br);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /BPReconciliation/{id}/Reconcile ────────────────────
    [HttpPost("{id:int}/Reconcile")]
    public async Task<IActionResult> ReconcileLines(int id, [FromBody] ReconcileBPLineRequest dto)
    {
        var br = await Load(id);
        if (br == null) return NotFound();
        if (br.Status == "C") return BadRequest("Cannot modify a Closed reconciliation.");

        int count = 0;
        foreach (var line in br.Lines.Where(l => dto.LineIds.Contains(l.Id)))
        {
            line.IsReconciled   = true;
            line.ReconciledDate = DateTime.UtcNow;
            line.ReconciledRef  = dto.ReconciledRef;
            count++;
        }

        RecalculateTotals(br);
        br.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"{count} line(s) marked as reconciled", reconciledAmt = br.ReconciledAmt, unreconciledAmt = br.UnreconciledAmt });
    }

    // ── POST /BPReconciliation/{id}/Unreconcile ──────────────────
    [HttpPost("{id:int}/Unreconcile")]
    public async Task<IActionResult> UnreconcileLines(int id, [FromBody] ReconcileBPLineRequest dto)
    {
        var br = await Load(id);
        if (br == null) return NotFound();
        if (br.Status == "C") return BadRequest("Cannot modify a Closed reconciliation.");

        foreach (var line in br.Lines.Where(l => dto.LineIds.Contains(l.Id)))
        {
            line.IsReconciled   = false;
            line.ReconciledDate = null;
            line.ReconciledRef  = null;
        }

        RecalculateTotals(br);
        br.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Lines unreconciled" });
    }

    // ── POST /BPReconciliation/{id}/Close ────────────────────────
    [HttpPost("{id:int}/Close")]
    public async Task<IActionResult> Close(int id)
    {
        var br = await Load(id);
        if (br == null) return NotFound();
        if (br.Status == "C") return BadRequest("Already closed.");

        br.Status    = "C";
        br.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "BP Reconciliation closed", reconcNo = br.ReconcNo });
    }

    // ── POST /BPReconciliation/{id}/Reopen ───────────────────────
    [HttpPost("{id:int}/Reopen")]
    public async Task<IActionResult> Reopen(int id)
    {
        var br = await _db.Set<BPReconciliation>().FirstOrDefaultAsync(x => x.Id == id);
        if (br == null) return NotFound();
        if (br.Status != "C") return BadRequest("Only Closed reconciliations can be reopened.");

        br.Status    = "O";
        br.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new { message = "BP Reconciliation reopened" });
    }

    // ── DELETE /BPReconciliation/{id} ────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var br = await _db.Set<BPReconciliation>().FirstOrDefaultAsync(x => x.Id == id);
        if (br == null) return NotFound();
        if (br.Status == "C") return BadRequest("Cannot delete a Closed reconciliation.");

        _db.Set<BPReconciliation>().Remove(br);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /BPReconciliation/LoadTransactions ──────────────────
    // Pulls real AR/AP invoices, credit notes and payments for this partner —
    // whichever side (customer or vendor) actually has activity.
    [HttpPost("LoadTransactions")]
    public async Task<IActionResult> LoadTransactions([FromBody] LoadBPTransactionsRequest request)
    {
        var partner = await _db.Set<BPEntity>().FirstOrDefaultAsync(x => x.Id == request.PartnerId);
        if (partner == null) return NotFound("Business partner not found.");

        var lines = new List<BPReconciliationLineResponse>();
        int lineNum = 1;

        var arInvoices = await _db.Set<ARInvoiceEntity>()
            .Where(x => x.CustomerId == request.PartnerId && x.PostingDate >= request.FromDate && x.PostingDate <= request.ToDate && x.Status != "D")
            .ToListAsync();
        foreach (var d in arInvoices)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.PostingDate ?? request.FromDate, RefNo = d.DocNum, Description = $"AR Invoice {d.DocNum}", DocType = "KARI", DocEntry = d.Id, DocNo = d.DocNum, Debit = d.Total, Credit = 0, IsReconciled = false });

        var arCredits = await _db.Set<ARCreditNoteEntity>()
            .Where(x => x.CustomerId == request.PartnerId && x.PostingDate >= request.FromDate && x.PostingDate <= request.ToDate && x.Status != "D")
            .ToListAsync();
        foreach (var d in arCredits)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.PostingDate ?? request.FromDate, RefNo = d.DocNum, Description = $"AR Credit Note {d.DocNum}", DocType = "KACN", DocEntry = d.Id, DocNo = d.DocNum, Debit = 0, Credit = d.Total, IsReconciled = false });

        var apInvoices = await _db.Set<APInvoiceEntity>()
            .Where(x => x.VendorId == request.PartnerId && x.PostingDate >= request.FromDate && x.PostingDate <= request.ToDate && x.Status != "D")
            .ToListAsync();
        foreach (var d in apInvoices)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.PostingDate ?? request.FromDate, RefNo = d.DocNum, Description = $"AP Invoice {d.DocNum}", DocType = "KAPI", DocEntry = d.Id, DocNo = d.DocNum, Debit = 0, Credit = d.Total, IsReconciled = false });

        var apCredits = await _db.Set<APCreditNoteEntity>()
            .Where(x => x.VendorId == request.PartnerId && x.PostingDate >= request.FromDate && x.PostingDate <= request.ToDate && x.Status != "D")
            .ToListAsync();
        foreach (var d in apCredits)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.PostingDate ?? request.FromDate, RefNo = d.DocNum, Description = $"AP Credit Note {d.DocNum}", DocType = "KAPC", DocEntry = d.Id, DocNo = d.DocNum, Debit = d.Total, Credit = 0, IsReconciled = false });

        var incoming = await _db.Set<IncomingPaymentEntity>()
            .Where(x => x.CardCode == partner.Code && x.DocDate >= request.FromDate && x.DocDate <= request.ToDate && x.Status != "V")
            .ToListAsync();
        foreach (var d in incoming)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.DocDate, RefNo = d.DocNum, Description = $"Incoming Payment {d.DocNum}", DocType = "KPYI", DocEntry = d.Id, DocNo = d.DocNum, Debit = 0, Credit = d.DocTotal, IsReconciled = false });

        var outgoing = await _db.Set<OutgoingPaymentEntity>()
            .Where(x => x.CardCode == partner.Code && x.DocDate >= request.FromDate && x.DocDate <= request.ToDate && x.Status != "V")
            .ToListAsync();
        foreach (var d in outgoing)
            lines.Add(new BPReconciliationLineResponse { LineNum = lineNum++, TransDate = d.DocDate, RefNo = d.DocNum, Description = $"Outgoing Payment {d.DocNum}", DocType = "KPYO", DocEntry = d.Id, DocNo = d.DocNum, Debit = d.DocTotal, Credit = 0, IsReconciled = false });

        return Ok(lines.OrderBy(l => l.TransDate).ToList());
    }

    // ── HELPERS ───────────────────────────────────────────────────
    private async Task<BPReconciliation?> Load(int id) =>
        await _db.Set<BPReconciliation>()
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);

    private static void RecalculateTotals(BPReconciliation br)
    {
        br.ReconciledAmt   = br.Lines.Where(l => l.IsReconciled).Sum(l => l.Debit - l.Credit);
        br.UnreconciledAmt = br.Lines.Where(l => !l.IsReconciled).Sum(l => l.Debit - l.Credit);
        br.ClosingBalance  = br.OpeningBalance + br.Lines.Sum(l => l.Debit - l.Credit);
    }

    private async Task<string> GenerateReconcNo()
    {
        var year   = DateTime.Now.Year;
        var prefix = $"BPR-{year}-";
        var last = await _db.Set<BPReconciliation>()
            .Where(b => b.ReconcNo.StartsWith(prefix))
            .OrderByDescending(b => b.Id)
            .FirstOrDefaultAsync();

        if (last == null) return $"{prefix}00001";
        var parts = last.ReconcNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n)
            ? $"{prefix}{(n + 1):D5}"
            : $"{prefix}00001";
    }
}
