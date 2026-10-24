using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi;
using FarmingApi.Core;
using ItemEntity = FarmingApi.Modules.Inventory.ItemsMaster.ItemsMaster;
using InventoryBalanceEntity = FarmingApi.Modules.Sale.Inventory.Inventory;
using DocNumberRangeEntity = FarmingApi.Modules.Administration.DocumentNumberRange.DocumentNumberRange;
using UserEntity = FarmingApi.Modules.Administration.SetUp.User.User;

namespace FarmingApi.Modules.Administration.AlertManagement;

[ApiController]
[Route("[controller]")]
public class AlertManagementController : ControllerBase
{
    private readonly MyDbContext    _db;
    private readonly IMapper        _mapper;
    private readonly IAlertService  _alerts;

    public AlertManagementController(MyDbContext db, IMapper mapper, IAlertService alerts)
    {
        _db     = db;
        _mapper = mapper;
        _alerts = alerts;
    }

    // ── GET /AlertManagement ── the caller's own inbox, newest first ────────
    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var list = await _db.Set<Alert>()
            .Include(x => x.SenderUser)
            .Where(x => x.RecipientUserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(_mapper.Map<List<AlertResponse>>(list));
    }

    // ── GET /AlertManagement/UnreadCount ── for a bell-icon badge ────────────
    [HttpGet("UnreadCount")]
    public async Task<IActionResult> UnreadCount()
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var count = await _db.Set<Alert>().CountAsync(x => x.RecipientUserId == userId && !x.IsRead);
        return Ok(new { count });
    }

    // ── POST /AlertManagement/{id}/Read ──────────────────────────────────────
    [HttpPost("{id:int}/Read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var alert = await _db.Set<Alert>().FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserId == userId);
        if (alert == null) return NotFound();

        if (!alert.IsRead)
        {
            alert.IsRead = true;
            alert.ReadAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        return NoContent();
    }

    // ── POST /AlertManagement/ReadAll ────────────────────────────────────────
    [HttpPost("ReadAll")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var unread = await _db.Set<Alert>()
            .Where(x => x.RecipientUserId == userId && !x.IsRead)
            .ToListAsync();

        foreach (var a in unread) { a.IsRead = true; a.ReadAt = DateTime.UtcNow; }
        await _db.SaveChangesAsync();

        return Ok(new { updated = unread.Count });
    }

    // ── DELETE /AlertManagement/{id} ── dismiss one of my own alerts ────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        var alert = await _db.Set<Alert>().FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserId == userId);
        if (alert == null) return NotFound();

        _db.Set<Alert>().Remove(alert);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ── POST /AlertManagement/Send ── person-to-person message ──────────────
    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest dto)
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        if (string.IsNullOrWhiteSpace(dto.Title)) return BadRequest("Title is required.");
        if (string.IsNullOrWhiteSpace(dto.Body))  return BadRequest("Message is required.");

        var recipientExists = await _db.Set<UserEntity>().AnyAsync(u => u.Id == dto.RecipientUserId);
        if (!recipientExists) return BadRequest($"Recipient user {dto.RecipientUserId} not found.");

        await _alerts.RaiseAsync(dto.RecipientUserId, "Message", dto.Title, dto.Body, senderUserId: userId.Value);
        return Ok(new { message = "Sent" });
    }

    // ── POST /AlertManagement/RunChecks ── scans built-in system conditions
    // and raises alerts to the CALLER (there's no "who owns inventory"
    // concept in this app yet, so a check notifies whoever asked for it,
    // same as clicking "refresh" on a dashboard tile). Idempotent per
    // recipient via ReferenceKey — re-running only raises what's new.
    [HttpPost("RunChecks")]
    public async Task<IActionResult> RunChecks()
    {
        var userId = this.CurrentUserId();
        if (userId == null) return Forbid();

        int raised = 0;

        // ── Low stock: on-hand (Σ Inventory.Balance per item) below ItemsMaster.MinStock ──
        var balances = await _db.Set<InventoryBalanceEntity>()
            .GroupBy(x => x.ItemsCode)
            .Select(g => new { ItemCode = g.Key, OnHand = g.Sum(x => x.Balance) })
            .ToListAsync();

        var trackedItems = await _db.Set<ItemEntity>().Where(i => i.MinStock > 0).ToListAsync();
        foreach (var item in trackedItems)
        {
            var onHand = balances.FirstOrDefault(b => b.ItemCode == item.ItemCode)?.OnHand ?? 0;
            if (onHand >= item.MinStock) continue;

            var created = await _alerts.RaiseAsync(
                userId.Value, "LowStock",
                $"Low stock: {item.ItemName}",
                $"{item.ItemCode} is at {onHand:0.##} on hand — below the minimum of {item.MinStock:0.##}.",
                link: "/Inventory/ItemsMaster/List",
                referenceKey: $"LowStock:{item.ItemCode}");
            if (created) raised++;
        }

        // ── Document number ranges within 10 numbers of their configured limit ──
        var lowRanges = await _db.Set<DocNumberRangeEntity>()
            .Where(r => r.LastNum != null && (r.LastNum.Value - r.NextNum) <= 10)
            .ToListAsync();
        foreach (var r in lowRanges)
        {
            var remaining = r.LastNum!.Value - r.NextNum;
            var created = await _alerts.RaiseAsync(
                userId.Value, "DocNumberRangeLow",
                $"Number range running out: {r.DocumentType}",
                $"Series '{r.SeriesName}' has {remaining} number{(remaining == 1 ? "" : "s")} left before it hits its configured limit.",
                link: "/Administration/Documentnumberrange/List",
                referenceKey: $"DocNumberRangeLow:{r.Id}");
            if (created) raised++;
        }

        // ── Daily exchange-rate update overdue ──
        var fx = await _db.Set<FarmingApi.Modules.Financials.ExchangeRate.ExchangeRateSettings>().AsNoTracking().OrderBy(s => s.Id).FirstOrDefaultAsync();
        var utcNow = DateTime.UtcNow;
        if (fx is { AutoFetchEnabled: true } && utcNow.Hour > fx.RunAtHourUtc
            && (fx.LastSuccessAt == null || fx.LastSuccessAt.Value.ToUniversalTime().Date < utcNow.Date))
        {
            var created = await _alerts.RaiseAsync(
                userId.Value, "ExchangeRateStale",
                "Exchange rates were not updated today",
                $"The daily update did not complete{(string.IsNullOrWhiteSpace(fx.LastMessage) ? "." : ": " + fx.LastMessage)} Last successful update: {(fx.LastSuccessAt?.ToUniversalTime().ToString("yyyy-MM-dd HH:mm") ?? "never")} UTC.",
                link: "/Administration/ExchangeRateAndIndex/List",
                referenceKey: $"ExchangeRateStale:{utcNow:yyyy-MM-dd}");
            if (created) raised++;
        }

        return Ok(new RunChecksResponse
        {
            Message = raised == 0 ? "Checked — nothing new." : $"Checked — {raised} new alert(s) raised.",
            Raised  = raised,
        });
    }
}
