using Microsoft.EntityFrameworkCore;
using FarmingApi.Modules.Administration.AlertManagement;

namespace FarmingApi.Core;

// ══════════════════════════════════════════════════════════════════════════════
// Central hook any module can call to drop a notification into a user's
// inbox — a person-to-person message, or a system-raised alert (low stock,
// a document number range running low, an approval waiting on someone).
// System checks pass a referenceKey so re-running them is idempotent: a
// recipient who already has an unread alert for the same condition doesn't
// get a duplicate.
// ══════════════════════════════════════════════════════════════════════════════
public interface IAlertService
{
    // Returns false (no row written) when referenceKey matches an existing
    // unread alert for this recipient — the caller can use that to report
    // "N new alerts" without double-counting on repeated runs.
    Task<bool> RaiseAsync(int recipientUserId, string type, string title, string body,
        string? link = null, string? referenceKey = null, int? senderUserId = null);
}

public class AlertService : IAlertService
{
    private readonly MyDbContext _db;
    public AlertService(MyDbContext db) => _db = db;

    public async Task<bool> RaiseAsync(int recipientUserId, string type, string title, string body,
        string? link = null, string? referenceKey = null, int? senderUserId = null)
    {
        if (referenceKey != null)
        {
            var alreadyUnread = await _db.Set<Alert>().AnyAsync(a =>
                a.RecipientUserId == recipientUserId && a.ReferenceKey == referenceKey && !a.IsRead);
            if (alreadyUnread) return false;
        }

        _db.Set<Alert>().Add(new Alert
        {
            RecipientUserId = recipientUserId,
            SenderUserId    = senderUserId,
            Type            = type,
            Title           = title,
            Body            = body,
            Link            = link,
            ReferenceKey    = referenceKey,
            IsRead          = false,
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
