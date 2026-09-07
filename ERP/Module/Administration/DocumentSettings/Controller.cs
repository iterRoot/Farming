using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FarmingApi.Core;

namespace FarmingApi.Modules.Administration.DocumentSettings;

// ═══════════════════════════════════════════════════════════════
// DOCUMENT SETTINGS  (route: /DocumentSetting)
// GET    /DocumentSetting              → { general, documents:[...] }
// PUT    /DocumentSetting/General      → save the General tab (singleton)
// GET    /DocumentSetting/{docType}    → one document type's settings
// PUT    /DocumentSetting/{docType}    → save one document type (upsert)
// DELETE /DocumentSetting/{docType}    → reset a document type
// ═══════════════════════════════════════════════════════════════
public class DocumentSettingController : MyController
{
    private readonly IDocumentGeneralSettingRepository _general;
    private readonly IDocumentTypeSettingRepository    _perDoc;

    public DocumentSettingController(
        IDocumentGeneralSettingRepository general,
        IDocumentTypeSettingRepository perDoc)
    {
        _general = general;
        _perDoc  = perDoc;
    }

    // Known document types — the Per-Document grid always shows all of these.
    private static readonly string[] DocTypes =
    {
        "SaleQuotation", "SaleOrder", "Delivery", "ARInvoice", "ARReserveInvoice",
        "ARCreditNote", "ARDownPayment",
        "PurchaseQuotation", "PurchaseOrder", "GoodsReceiptPO", "GoodsReturn",
        "APInvoice", "APCreditNote", "APDownPayment",
    };

    private static Dictionary<string, JsonElement> Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new(); }
        catch { return new(); }
    }

    // ── GET /DocumentSetting ───────────────────────────────────
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Get()
    {
        var gen  = _general.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        var rows = _perDoc.GetAll().ToDictionary(x => x.DocumentType, x => x.Settings);

        var docs = DocTypes.Select(dt => new DocumentTypeSettingDto
        {
            DocumentType = dt,
            Configured   = rows.ContainsKey(dt),
            Settings     = rows.TryGetValue(dt, out var s) ? Parse(s) : new(),
        }).ToList();

        return Ok(new DocumentSettingsResponse
        {
            General   = Parse(gen?.Settings),
            Documents = docs,
        });
    }

    // ── PUT /DocumentSetting/General ───────────────────────────
    [AllowAnonymous]
    [HttpPut("General")]
    public IActionResult SaveGeneral([FromBody] DocumentTypeSettingSaveRequest request)
    {
        var json = JsonSerializer.Serialize(request?.Settings ?? new());

        var g = _general.GetAll().OrderBy(x => x.Id).FirstOrDefault();
        var isNew = g == null;
        g ??= new DocumentGeneralSetting { CreatedAt = DateTime.UtcNow, InActive = false };
        g.Settings = json;

        if (isNew) _general.Add(g);
        else { g.UpdatedAt = DateTime.UtcNow; _general.Update(g); }

        _general.Commit();
        return Content(json, "application/json");
    }

    // ── GET /DocumentSetting/{docType} ─────────────────────────
    [AllowAnonymous]
    [HttpGet("{docType}")]
    public IActionResult GetOne(string docType)
    {
        if (!DocTypes.Contains(docType)) return NotFound($"Unknown document type: {docType}");
        var row = _perDoc.GetSingle(x => x.DocumentType == docType);
        return Ok(new DocumentTypeSettingDto
        {
            DocumentType = docType,
            Configured   = row != null,
            Settings     = Parse(row?.Settings),
        });
    }

    // ── PUT /DocumentSetting/{docType} ─────────────────────────
    [AllowAnonymous]
    [HttpPut("{docType}")]
    public IActionResult SaveOne(string docType, [FromBody] DocumentTypeSettingSaveRequest request)
    {
        if (!DocTypes.Contains(docType)) return BadRequest($"Unknown document type: {docType}");

        var json = JsonSerializer.Serialize(request?.Settings ?? new());

        var row = _perDoc.GetSingle(x => x.DocumentType == docType);
        var isNew = row == null;
        row ??= new DocumentTypeSetting { DocumentType = docType, CreatedAt = DateTime.UtcNow, InActive = false };
        row.Settings = json;

        if (isNew) _perDoc.Add(row);
        else { row.UpdatedAt = DateTime.UtcNow; _perDoc.Update(row); }

        _perDoc.Commit();
        return Ok(new { message = $"{docType} settings saved", documentType = docType });
    }

    // ── DELETE /DocumentSetting/{docType} ──────────────────────
    [AllowAnonymous]
    [HttpDelete("{docType}")]
    public IActionResult Delete(string docType)
    {
        var row = _perDoc.GetSingle(x => x.DocumentType == docType);
        if (row == null) return NotFound($"No settings for {docType}");
        _perDoc.Remove(row);
        _perDoc.Commit();
        return NoContent();
    }
}
