using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Administration.PredefinedText;

[ApiController]
[Route("[controller]")]
public class PredefinedTextController : ControllerBase
{
    private readonly IPredefinedTextRepository _repo;
    private readonly IMapper                   _mapper;

    public PredefinedTextController(IPredefinedTextRepository repo, IMapper mapper)
    {
        _repo   = repo;
        _mapper = mapper;
    }

    // ── GET /PredefinedText ────────────────────────────────────
    [AllowAnonymous][HttpGet]
    public IActionResult GetAll(
        [FromQuery] string? category = null,
        [FromQuery] string? module   = null,
        [FromQuery] string? language = null)
    {
        var query = _repo.GetAll()
            .Where(x => category == null || x.Category == category)
            .Where(x => module   == null || x.Module   == module || x.Module == "All")
            .Where(x => language == null || x.Language == language)
            .OrderBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Category)
            .ThenBy(x => x.Name)
            .ToList();
        return Ok(_mapper.Map<List<PredefinedTextResponse>>(query));
    }

    // ── GET /PredefinedText/{id} ───────────────────────────────
    [AllowAnonymous][HttpGet("{id:int}")]
    public IActionResult GetById(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound();
        return Ok(_mapper.Map<PredefinedTextResponse>(e));
    }

    // ── GET /PredefinedText/Search?q=payment ──────────────────
    [AllowAnonymous][HttpGet("Search")]
    public IActionResult Search([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Query parameter 'q' is required");

        var term  = q.ToLower();
        var results = _repo.GetAll()
            .Where(x => x.IsActive && (
                x.Name.ToLower().Contains(term)         ||
                x.TextContent.ToLower().Contains(term)  ||
                (x.Tags != null && x.Tags.ToLower().Contains(term)) ||
                (x.Category != null && x.Category.ToLower().Contains(term))
            ))
            .OrderBy(x => x.SortOrder == 0 ? int.MaxValue : x.SortOrder)
            .ThenBy(x => x.Name)
            .Take(20)
            .ToList();
        return Ok(_mapper.Map<List<PredefinedTextResponse>>(results));
    }

    // ── GET /PredefinedText/Categories ────────────────────────
    [AllowAnonymous][HttpGet("Categories")]
    public IActionResult GetCategories()
    {
        var cats = _repo.GetAll()
            .Select(x => x.Category)
            .Where(x => x != null)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return Ok(cats);
    }

    // ── GET /PredefinedText/Languages ─────────────────────────
    [AllowAnonymous][HttpGet("Languages")]
    public IActionResult GetLanguages()
    {
        var langs = _repo.GetAll()
            .Select(x => x.Language)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
        return Ok(langs);
    }

    // ── POST /PredefinedText ───────────────────────────────────
    [AllowAnonymous][HttpPost]
    public IActionResult Create([FromBody] PredefinedTextRequest dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Predefined text code '{code}' already exists");

        var entity       = _mapper.Map<PredefinedText>(dto);
        entity.Code      = code;
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive  = false;
        _repo.Add(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PredefinedTextResponse>(entity));
    }

    // ── PUT /PredefinedText/{id} ───────────────────────────────
    [AllowAnonymous][HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] PredefinedTextRequest dto)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code && x.Id != id))
            return BadRequest($"Code '{code}' already exists");

        _mapper.Map(dto, entity);
        entity.Code      = code;
        entity.UpdatedAt = DateTime.UtcNow;
        _repo.Update(entity);
        _repo.Commit();
        return Ok(_mapper.Map<PredefinedTextResponse>(entity));
    }

    // ── POST /PredefinedText/{id}/Copy ─────────────────────────
    [AllowAnonymous][HttpPost("{id:int}/Copy")]
    public IActionResult Copy(int id, [FromBody] CopyTextRequest dto)
    {
        var source = _repo.GetSingle(x => x.Id == id);
        if (source == null) return NotFound();
        var code = dto.Code.Trim().ToUpper();
        if (_repo.GetAll().Any(x => x.Code == code))
            return BadRequest($"Code '{code}' already exists");

        var copy = new PredefinedText
        {
            Code        = code,
            Name        = dto.Name,
            Category    = source.Category,
            Module      = source.Module,
            Language    = source.Language,
            TextContent = source.TextContent,
            Tags        = source.Tags,
            SortOrder   = source.SortOrder,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
            InActive    = false,
        };
        _repo.Add(copy);
        _repo.Commit();
        return Ok(_mapper.Map<PredefinedTextResponse>(copy));
    }

    // ── DELETE /PredefinedText/{id} ────────────────────────────
    [AllowAnonymous][HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var entity = _repo.GetSingle(x => x.Id == id);
        if (entity == null) return NotFound();
        _repo.Remove(entity);
        _repo.Commit();
        return NoContent();
    }

    // ── POST /PredefinedText/Seed ──────────────────────────────
    [AllowAnonymous][HttpPost("Seed")]
    public IActionResult Seed()
    {
        if (_repo.GetAll().Any())
            return BadRequest("Predefined texts already exist.");

        var seeds = GetSeedData();
        foreach (var s in seeds) { s.CreatedAt = DateTime.UtcNow; s.InActive = false; _repo.Add(s); }
        _repo.Commit();
        return Ok(new { message = $"{seeds.Count} predefined texts created" });
    }

    // ─── Seed data ────────────────────────────────────────────
    private static List<PredefinedText> GetSeedData() => new()
    {
        // Sales
        new(){ Code="TXT-THANKS",     Name="Thank You Message",         Category="Sales",     Module="All",          Language="en", SortOrder=1,  TextContent="Thank you for your business. We look forward to serving you again." },
        new(){ Code="TXT-QUO-VALID",  Name="Quotation Validity",        Category="Sales",     Module="Quotation",    Language="en", SortOrder=2,  TextContent="This quotation is valid for 7 days from the date of issue. Prices are subject to change after the validity period.", Tags="quotation,valid,expiry" },
        new(){ Code="TXT-DELIVER",    Name="Delivery Note",             Category="Sales",     Module="SalesOrder",   Language="en", SortOrder=3,  TextContent="Delivery will be made within 3-5 working days upon confirmation of payment. Please ensure someone is available to receive the goods.", Tags="delivery,shipping" },
        new(){ Code="TXT-NO-RETURN",  Name="No Return Policy",          Category="Sales",     Module="ARInvoice",    Language="en", SortOrder=4,  TextContent="Goods once sold cannot be returned or exchanged unless defective. Please inspect all items upon delivery.", Tags="return,policy,exchange" },
        new(){ Code="TXT-DISC-NOTE",  Name="Discount Note",             Category="Sales",     Module="ARInvoice",    Language="en", SortOrder=5,  TextContent="This discount is a one-time special offer and does not apply to future orders.", Tags="discount,special" },

        // Payment Terms
        new(){ Code="TXT-PAY-30",     Name="Payment Due 30 Days",       Category="Payment",   Module="ARInvoice",    Language="en", SortOrder=10, TextContent="Payment is due within 30 days of invoice date. Late payments may incur a 2% monthly interest charge.", Tags="payment,net30,terms" },
        new(){ Code="TXT-PAY-15",     Name="Payment Due 15 Days",       Category="Payment",   Module="ARInvoice",    Language="en", SortOrder=11, TextContent="Payment is due within 15 days of invoice date.", Tags="payment,net15,terms" },
        new(){ Code="TXT-PAY-COD",    Name="Cash on Delivery",          Category="Payment",   Module="SalesOrder",   Language="en", SortOrder=12, TextContent="Full payment is required upon delivery of goods. Cash or bank transfer accepted.", Tags="cod,cash,delivery" },
        new(){ Code="TXT-ADVANCE",    Name="Advance Payment Required",  Category="Payment",   Module="SalesOrder",   Language="en", SortOrder=13, TextContent="A 50% advance payment is required before processing this order. The remaining balance is due upon delivery.", Tags="advance,deposit,50%" },

        // Purchasing
        new(){ Code="TXT-PO-NOTE",    Name="Purchase Order Note",       Category="Purchasing",Module="PurchaseOrder",Language="en", SortOrder=20, TextContent="Please confirm receipt of this purchase order within 24 hours. Delivery must match the specifications listed above.", Tags="purchase,order,confirm" },
        new(){ Code="TXT-VENDOR-QTY", Name="Vendor Quantity Note",      Category="Purchasing",Module="PurchaseOrder",Language="en", SortOrder=21, TextContent="Partial deliveries are not accepted unless prior written approval is obtained. Full order quantity must be delivered together.", Tags="quantity,partial,delivery" },
        new(){ Code="TXT-INSP",       Name="Goods Inspection Note",     Category="Purchasing",Module="GoodsReceipt", Language="en", SortOrder=22, TextContent="All goods are subject to quality inspection upon receipt. Items not meeting specifications will be returned at vendor's expense.", Tags="inspection,quality,return" },

        // Legal / Compliance
        new(){ Code="TXT-CONF",       Name="Confidentiality Notice",    Category="Legal",     Module="All",          Language="en", SortOrder=30, TextContent="This document contains confidential information intended solely for the named recipient. Unauthorised disclosure is strictly prohibited.", Tags="confidential,legal,private" },
        new(){ Code="TXT-FORCE",      Name="Force Majeure Clause",      Category="Legal",     Module="All",          Language="en", SortOrder=31, TextContent="Neither party shall be liable for failure to perform obligations due to circumstances beyond reasonable control, including natural disasters, government actions, or other force majeure events.", Tags="force majeure,legal,clause" },
        new(){ Code="TXT-DISPUTE",    Name="Dispute Resolution",        Category="Legal",     Module="All",          Language="en", SortOrder=32, TextContent="Any disputes arising from this agreement shall be resolved through mutual negotiation. If unresolved, the matter shall be referred to the competent courts of the Kingdom of Cambodia.", Tags="dispute,legal,Cambodia" },

        // Farming-specific
        new(){ Code="TXT-HARVEST",    Name="Harvest Season Note",       Category="General",   Module="All",          Language="en", SortOrder=40, TextContent="Please note that prices may fluctuate during the main harvest season (November–February). Orders placed during peak season are subject to availability.", Tags="harvest,season,price" },
        new(){ Code="TXT-ORGANIC",    Name="Organic Certification",     Category="General",   Module="ARInvoice",    Language="en", SortOrder=41, TextContent="All products marked as organic have been certified under applicable standards. Certificates are available upon request.", Tags="organic,certificate,certified" },
        new(){ Code="TXT-STORAGE",    Name="Storage Instructions",      Category="General",   Module="GoodsReceipt", Language="en", SortOrder=42, TextContent="Store in a cool, dry place away from direct sunlight. Maintain temperature between 15°C and 25°C. Keep away from moisture.", Tags="storage,temperature,conditions" },

        // Khmer language samples
        new(){ Code="TXT-THANKS-KM",  Name="ថ្លែងអំណរគុណ",               Category="Sales",     Module="All",          Language="km", SortOrder=50, TextContent="សូមអរគុណចំពោះការទំនុកចិត្ត និងការជ្រើសរើសប្រើប្រាស់សេវាកម្មរបស់យើង។ យើងសង្ឃឹមថានឹងបន្តធ្វើការជាមួយគ្នា។", Tags="khmer,thanks" },
        new(){ Code="TXT-PAY-KM",     Name="លក្ខខណ្ឌការទូទាត់",          Category="Payment",   Module="ARInvoice",    Language="km", SortOrder=51, TextContent="ការទូទាត់ត្រូវបញ្ចប់ក្នុងរយៈពេល ៣០ ថ្ងៃ គិតពីថ្ងៃចេញវិក្កយបត្រ។ ការទូទាត់យឺតអាចនឹងត្រូវបង់ការប្រាក់ ២% ក្នុងមួយខែ។", Tags="khmer,payment,terms" },
    };
}