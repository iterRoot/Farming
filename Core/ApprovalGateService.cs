using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Administration.ApprovalProcess;
using ApprovalRequestEntity = FarmingApi.Modules.Administration.ApprovalRequest.ApprovalRequest;

namespace FarmingApi.Core;

public record ApprovalGateResult(bool RequiresApproval, int? ApprovalRequestId);
public record ApprovalDecisionResult(bool Success, string? Error);

// ══════════════════════════════════════════════════════════════════════════════
// Central hook every one of the 23 Sale-AR / Purchase-AP / Purchase Request
// document controllers calls from its own Post/Approve/Reject actions. Keeps
// the condition-matching and approver-eligibility rules in one place while the
// actual GL/inventory posting logic stays where it already lives, per document
// type (called back by each controller itself once EvaluateAsync/DecideAsync
// says it's clear to proceed).
// ══════════════════════════════════════════════════════════════════════════════
public interface IApprovalGateService
{
    // Called at the top of a document's Post action, right after the
    // Status == "D" guard. True → the caller must stop, set Status = "P" and
    // save; the caller's normal posting logic must NOT run yet.
    Task<ApprovalGateResult> EvaluateAsync(string documentType, int documentId, string docNum, decimal total, int requestedByUserId);

    // Called from a document's Approve/Reject actions. Validates the decider
    // is a listed approver on the matching template and that a Pending request
    // exists; does not touch the document itself — the caller applies the
    // Status transition and (for Approve) its own posting logic afterward.
    Task<ApprovalDecisionResult> DecideAsync(string documentType, int documentId, string decision, int deciderUserId, string? remarks);
}

public class ApprovalGateService : IApprovalGateService
{
    private readonly MyDbContext   _db;
    private readonly IAlertService _alerts;
    public ApprovalGateService(MyDbContext db, IAlertService alerts) { _db = db; _alerts = alerts; }

    public async Task<ApprovalGateResult> EvaluateAsync(string documentType, int documentId, string docNum, decimal total, int requestedByUserId)
    {
        var templates = await _db.Set<ApprovalTemplate>()
            .Include(t => t.Approvers)
            .Where(t => t.Active && t.DocumentType == documentType)
            .OrderBy(t => t.Id)
            .ToListAsync();

        var match = templates.FirstOrDefault(t => Matches(t, total));
        if (match == null) return new ApprovalGateResult(false, null);

        var existing = await _db.Set<ApprovalRequestEntity>()
            .FirstOrDefaultAsync(r => r.DocumentType == documentType && r.DocumentId == documentId && r.Status == "Pending");
        if (existing != null) return new ApprovalGateResult(true, existing.Id);

        var request = new ApprovalRequestEntity
        {
            DocumentType       = documentType,
            DocumentId         = documentId,
            DocNum             = docNum,
            Total              = total,
            ApprovalTemplateId = match.Id,
            Status             = "Pending",
            RequestedBy        = requestedByUserId,
            RequestedAt        = DateTime.UtcNow,
        };

        _db.Set<ApprovalRequestEntity>().Add(request);
        await _db.SaveChangesAsync();

        // Tell every approver — this is the alert system's main real source
        // of content, not a synthetic demo. RaiseAsync is per-recipient
        // idempotent, so this is safe even if EvaluateAsync is ever called
        // twice for the same request (it isn't today, but costs nothing).
        foreach (var approver in match.Approvers)
        {
            await _alerts.RaiseAsync(
                approver.UserId, "ApprovalPending",
                $"Approval needed: {docNum}",
                $"{docNum} (${total:0.##}) is waiting on your decision under \"{match.Name}\".",
                link: "/Administration/ApprovalRequest/List",
                referenceKey: $"ApprovalPending:{request.Id}");
        }

        return new ApprovalGateResult(true, request.Id);
    }

    public async Task<ApprovalDecisionResult> DecideAsync(string documentType, int documentId, string decision, int deciderUserId, string? remarks)
    {
        var request = await _db.Set<ApprovalRequestEntity>()
            .FirstOrDefaultAsync(r => r.DocumentType == documentType && r.DocumentId == documentId && r.Status == "Pending");
        if (request == null)
            return new ApprovalDecisionResult(false, "No pending approval request found for this document.");

        var template = await _db.Set<ApprovalTemplate>()
            .Include(t => t.Approvers)
            .FirstOrDefaultAsync(t => t.Id == request.ApprovalTemplateId);

        if (template == null || !template.Approvers.Any(a => a.UserId == deciderUserId))
            return new ApprovalDecisionResult(false, "You are not an approver for this document.");

        request.Status     = decision;
        request.DecidedBy  = deciderUserId;
        request.DecidedAt  = DateTime.UtcNow;
        request.Remarks    = remarks;
        await _db.SaveChangesAsync();

        return new ApprovalDecisionResult(true, null);
    }

    private static bool Matches(ApprovalTemplate template, decimal total)
    {
        if (template.Operator == null || template.Amount == null) return true;
        return template.Operator switch
        {
            ">"  => total >  template.Amount,
            ">=" => total >= template.Amount,
            "<"  => total <  template.Amount,
            "<=" => total <= template.Amount,
            "==" => total == template.Amount,
            _    => true,
        };
    }
}
