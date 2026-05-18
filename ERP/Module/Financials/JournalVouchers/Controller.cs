using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Financials.JournalEntry;

namespace FarmingApi.Modules.Financials.JournalVoucher;

public class JournalVoucherController : MyController
{
    private readonly IMapper _mapper;
    private readonly IJournalVoucherRepository _repository;
    private readonly IJournalEntryRepository _journalEntryRepository;

    public JournalVoucherController(
        IJournalVoucherRepository repository,
        IJournalEntryRepository journalEntryRepository,
        IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
        _journalEntryRepository = journalEntryRepository;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll().Include(j => j.Lines);
        var results = _mapper.ProjectTo<JournalVoucherListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var voucher = _repository.GetAll()
            .Include(j => j.Lines)
            .FirstOrDefault(e => e.Id == id);

        if (voucher == null)
            return BadRequest($"Journal Voucher not found {id}");

        var result = _mapper.Map<JournalVoucherListResponse>(voucher);
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE (Draft)
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] JournalVoucherListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.Lines == null || !request.Lines.Any())
            return BadRequest("Journal Voucher must have at least one line");

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
            return BadRequest($"Debit ({totalDebit:N2}) must equal Credit ({totalCredit:N2})");

        if (totalDebit == 0)
            return BadRequest("Total amount cannot be zero");

        var entity = _mapper.Map<JournalVoucher>(request);
        entity.VoucherNo = GenerateVoucherNumber();
        entity.TotalDebit = totalDebit;
        entity.TotalCredit = totalCredit;
        entity.Status = "D"; // Draft
        entity.ApprovalStatus = "N"; // No approval yet
        entity.VersionNum = 1;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        int lineNum = 1;
        foreach (var lineRequest in request.Lines)
        {
            var line = _mapper.Map<JournalVoucherLine>(lineRequest);
            line.LineNum = lineNum++;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "Journal Voucher saved as draft",
            id = entity.Id,
            voucherNo = entity.VoucherNo,
            status = entity.Status
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE (Only drafts can be updated)
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] JournalVoucherUpdateRequest request)
    {
        var voucher = _repository.GetAll()
            .Include(j => j.Lines)
            .FirstOrDefault(e => e.Id == id);

        if (voucher == null)
            return NotFound($"Journal Voucher not found: {id}");

        // Only drafts can be updated
        if (voucher.Status != "D")
            return BadRequest($"Cannot update voucher in status: {voucher.Status}. Only Draft vouchers can be updated.");

        // Cannot update if approved or rejected
        if (voucher.ApprovalStatus == "A")
            return BadRequest("Cannot update approved voucher");
        if (voucher.ApprovalStatus == "P")
            return BadRequest("Cannot update voucher pending approval");

        var totalDebit = request.Lines.Sum(l => l.Debit);
        var totalCredit = request.Lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
            return BadRequest($"Debit ({totalDebit:N2}) must equal Credit ({totalCredit:N2})");

        // Update header
        voucher.VoucherDate = request.VoucherDate;
        voucher.DueDate = request.DueDate;
        voucher.RefNo = request.RefNo;
        voucher.Description = request.Description;
        voucher.Memo = request.Memo;
        voucher.Currency = request.Currency;
        voucher.ExchangeRate = request.ExchangeRate;
        voucher.ProjectCode = request.ProjectCode;
        voucher.CostCenter = request.CostCenter;
        voucher.TotalDebit = totalDebit;
        voucher.TotalCredit = totalCredit;
        voucher.UpdatedAt = DateTime.UtcNow;
        voucher.VersionNum += 1;

        // Replace lines
        voucher.Lines.Clear();
        int lineNum = 1;
        foreach (var lineRequest in request.Lines)
        {
            var line = _mapper.Map<JournalVoucherLine>(lineRequest);
            line.LineNum = lineNum++;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive = false;
            voucher.Lines.Add(line);
        }

        _repository.Update(voucher);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // SUBMIT FOR APPROVAL
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id}/Submit")]
    public IActionResult Submit(int id)
    {
        var voucher = _repository.GetSingle(e => e.Id == id);
        if (voucher == null)
            return NotFound($"Voucher not found: {id}");

        if (voucher.Status != "D")
            return BadRequest("Only draft vouchers can be submitted");

        voucher.ApprovalStatus = "P"; // Pending approval
        voucher.UpdatedAt = DateTime.UtcNow;

        _repository.Update(voucher);
        _repository.Commit();

        return Ok(new { message = "Voucher submitted for approval", approvalStatus = "P" });
    }

    // ═══════════════════════════════════════════════════════════════
    // APPROVE / REJECT
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id}/Approval")]
    public IActionResult Approval(int id, [FromBody] ApprovalRequest request)
    {
        var voucher = _repository.GetSingle(e => e.Id == id);
        if (voucher == null)
            return NotFound($"Voucher not found: {id}");

        if (voucher.ApprovalStatus != "P")
            return BadRequest("Voucher is not pending approval");

        if (request.Action == "Approve")
        {
            voucher.ApprovalStatus = "A";
        }
        else if (request.Action == "Reject")
        {
            voucher.ApprovalStatus = "R";
        }
        else
        {
            return BadRequest("Invalid action. Use 'Approve' or 'Reject'");
        }

        voucher.ApprovedDate = DateTime.UtcNow;
        voucher.ApprovalNote = request.Note;
        voucher.UpdatedAt = DateTime.UtcNow;
        // voucher.ApprovedBy = GetClaim()!.Id;

        _repository.Update(voucher);
        _repository.Commit();

        return Ok(new
        {
            message = $"Voucher {request.Action.ToLower()}d successfully",
            approvalStatus = voucher.ApprovalStatus
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // POST TO JOURNAL ENTRY
    // (Convert approved voucher to actual Journal Entry)
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id}/Post")]
    public IActionResult PostToJournal(int id)
    {
        var voucher = _repository.GetAll()
            .Include(j => j.Lines)
            .FirstOrDefault(e => e.Id == id);

        if (voucher == null)
            return NotFound($"Voucher not found: {id}");

        if (voucher.Status != "D")
            return BadRequest("Voucher already posted or voided");

        if (voucher.ApprovalStatus != "A")
            return BadRequest("Voucher must be approved before posting");

        // Create Journal Entry from Voucher
        var journalEntry = new JournalEntry.JournalEntry
        {
            JrnlNo = GenerateJournalEntryNumber(),
            TransDate = voucher.VoucherDate,
            DueDate = voucher.DueDate,
            DocDate = voucher.VoucherDate,
            RefNo = voucher.VoucherNo,
            TransType = "JV", // From Journal Voucher
            BaseDocEntry = voucher.Id,
            BaseDocType = "KJVH",
            TotalDebit = voucher.TotalDebit,
            TotalCredit = voucher.TotalCredit,
            TotalLC = voucher.TotalDebit,
            TotalFC = voucher.TotalDebit * voucher.ExchangeRate,
            Currency = voucher.Currency,
            ExchangeRate = voucher.ExchangeRate,
            Memo = voucher.Memo ?? voucher.Description,
            Status = "C", // Closed/Posted immediately
            IsAutoGenerated = true,
            ProjectCode = voucher.ProjectCode,
            CostCenter = voucher.CostCenter,
            VersionNum = 1,
            CreatedAt = DateTime.UtcNow,
            InActive = false
        };

        // Copy lines
        int lineNum = 1;
        foreach (var voucherLine in voucher.Lines)
        {
            var journalLine = new JournalEntry.JournalEntryLine
            {
                LineNum = lineNum++,
                AccountCode = voucherLine.AccountCode,
                AccountName = voucherLine.AccountName,
                CardCode = voucherLine.CardCode,
                CardName = voucherLine.CardName,
                Debit = voucherLine.Debit,
                Credit = voucherLine.Credit,
                DebitFC = voucherLine.DebitFC,
                CreditFC = voucherLine.CreditFC,
                DebitLC = voucherLine.Debit,
                CreditLC = voucherLine.Credit,
                LineMemo = voucherLine.LineMemo,
                CostCenter = voucherLine.CostCenter,
                Project = voucherLine.Project,
                TaxCode = voucherLine.TaxCode,
                TaxAmount = voucherLine.TaxAmount,
                Reference = voucherLine.Reference,
                DueDate = voucherLine.DueDate,
                CreatedAt = DateTime.UtcNow,
                InActive = false
            };

            journalEntry.Lines.Add(journalLine);
        }

        _journalEntryRepository.Add(journalEntry);
        _journalEntryRepository.Commit();

        // Update voucher
        voucher.Status = "P"; // Posted
        voucher.PostedToJournalEntryId = journalEntry.Id;
        voucher.PostedDate = DateTime.UtcNow;
        voucher.UpdatedAt = DateTime.UtcNow;

        _repository.Update(voucher);
        _repository.Commit();

        return Ok(new
        {
            message = "Voucher posted to Journal Entry successfully",
            voucherId = voucher.Id,
            voucherNo = voucher.VoucherNo,
            journalEntryId = journalEntry.Id,
            jrnlNo = journalEntry.JrnlNo
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // VOID
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id}/Void")]
    public IActionResult Void(int id)
    {
        var voucher = _repository.GetSingle(e => e.Id == id);
        if (voucher == null)
            return NotFound($"Voucher not found: {id}");

        if (voucher.Status == "V")
            return BadRequest("Voucher is already voided");

        if (voucher.Status == "P")
            return BadRequest("Cannot void posted voucher. Void the Journal Entry instead.");

        voucher.Status = "V";
        voucher.UpdatedAt = DateTime.UtcNow;

        _repository.Update(voucher);
        _repository.Commit();

        return Ok(new { message = "Voucher voided successfully" });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE (Only drafts)
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var voucher = _repository.GetSingle(e => e.Id == id);
        if (voucher == null)
            return BadRequest($"Voucher not found {id}");

        if (voucher.Status != "D")
            return BadRequest("Only draft vouchers can be deleted");

        if (voucher.ApprovalStatus == "P")
            return BadRequest("Cannot delete voucher pending approval");

        voucher.DeletedAt = DateTime.UtcNow;
        _repository.Remove(voucher);
        _repository.Commit();

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY STATUS
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("Status/{status}")]
    public IActionResult GetByStatus(string status)
    {
        var vouchers = _repository.GetAll()
            .Include(j => j.Lines)
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.VoucherDate)
            .ToList();

        var results = _mapper.Map<List<JournalVoucherListResponse>>(vouchers);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET PENDING APPROVAL
    // ═══════════════════════════════════════════════════════════════
    [HttpGet("PendingApproval")]
    public IActionResult GetPendingApproval()
    {
        var vouchers = _repository.GetAll()
            .Include(j => j.Lines)
            .Where(j => j.ApprovalStatus == "P")
            .OrderBy(j => j.VoucherDate)
            .ToList();

        var results = _mapper.Map<List<JournalVoucherListResponse>>(vouchers);
        return Ok(results);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private string GenerateVoucherNumber()
    {
        var year = DateTime.Now.Year;
        var lastVoucher = _repository.GetAll()
            .Where(j => j.VoucherNo.StartsWith($"JV-{year}"))
            .OrderByDescending(j => j.Id)
            .FirstOrDefault();

        if (lastVoucher == null)
            return $"JV-{year}-00001";

        var parts = lastVoucher.VoucherNo.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
        {
            return $"JV-{year}-{(lastNumber + 1).ToString("D5")}";
        }

        return $"JV-{year}-00001";
    }

    private string GenerateJournalEntryNumber()
    {
        var year = DateTime.Now.Year;
        var lastJournal = _journalEntryRepository.GetAll()
            .Where(j => j.JrnlNo.StartsWith($"JE-{year}"))
            .OrderByDescending(j => j.Id)
            .FirstOrDefault();

        if (lastJournal == null)
            return $"JE-{year}-00001";

        var parts = lastJournal.JrnlNo.Split('-');
        if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
        {
            return $"JE-{year}-{(lastNumber + 1).ToString("D5")}";
        }

        return $"JE-{year}-00001";
    }
}