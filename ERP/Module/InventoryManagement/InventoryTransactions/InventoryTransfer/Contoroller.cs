using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;
using FarmingApi.Modules.Inventory.StockTransferRequest;

namespace FarmingApi.Modules.Inventory.StockTransfer;

public class StockTransferController : MyController
{
    private readonly IMapper                          _mapper;
    private readonly IStockTransferRepository         _repository;
    private readonly IStockTransferRequestRepository? _strRepository;

    public StockTransferController(
        IStockTransferRepository          repository,
        IMapper                           mapper,
        IStockTransferRequestRepository?  strRepository = null)
    {
        _mapper        = mapper;
        _repository    = repository;
        _strRepository = strRepository;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets(
        [FromQuery] string?   fromWarehouse = null,
        [FromQuery] string?   toWarehouse   = null,
        [FromQuery] string?   cardCode      = null,
        [FromQuery] DateTime? from          = null,
        [FromQuery] DateTime? to            = null)
    {
        var query = _repository.GetAll().Include(s => s.Lines).AsQueryable();

        if (!string.IsNullOrEmpty(fromWarehouse)) query = query.Where(s => s.FromWarehouse == fromWarehouse);
        if (!string.IsNullOrEmpty(toWarehouse))   query = query.Where(s => s.ToWarehouse   == toWarehouse);
        if (!string.IsNullOrEmpty(cardCode))      query = query.Where(s => s.CardCode      == cardCode);
        if (from.HasValue) query = query.Where(s => s.PostingDate >= from.Value);
        if (to.HasValue)   query = query.Where(s => s.PostingDate <= to.Value);

        var result = query.OrderByDescending(s => s.PostingDate)
                          .ThenByDescending(s => s.Id)
                          .ToList();
        return Ok(_mapper.Map<List<StockTransferResponse>>(result));
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var doc = _repository.GetAll()
            .Include(s => s.Lines)
            .FirstOrDefault(s => s.Id == id);

        if (doc == null) return NotFound($"Stock Transfer {id} not found");
        return Ok(_mapper.Map<StockTransferResponse>(doc));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] StockTransferCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.Lines == null || !request.Lines.Any())
            return BadRequest("At least one item line is required");

        if (request.FromWarehouse == request.ToWarehouse)
            return BadRequest("From Warehouse and To Warehouse must be different");

        var missingItems = request.Lines.Where(l => string.IsNullOrWhiteSpace(l.ItemNo)).ToList();
        if (missingItems.Any())
            return BadRequest("All lines must have an Item No.");

        var entity = _mapper.Map<StockTransfer>(request);
        entity.DocNo         = GenerateDocNo();
        entity.Number        = GenerateNumber();
        entity.TotalQuantity = request.Lines.Sum(l => l.Quantity);
        entity.VersionNum    = 1;
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;

        if (string.IsNullOrEmpty(entity.JournalRemarks))
            entity.JournalRemarks = $"Stock Transfers - {entity.DocNo}";

        int lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<StockTransferLine>(lr);
            line.LineNum         = lineNum++;
            line.ItemNo          = lr.ItemNo.Trim();
            line.ItemDescription = lr.ItemDescription ?? lr.ItemNo;
            line.FromWarehouse   = string.IsNullOrEmpty(lr.FromWarehouse) ? request.FromWarehouse : lr.FromWarehouse;
            line.ToWarehouse     = string.IsNullOrEmpty(lr.ToWarehouse)   ? request.ToWarehouse   : lr.ToWarehouse;
            line.ToBinLocation   = string.IsNullOrEmpty(lr.ToBinLocation) ? request.ToBinLocation : lr.ToBinLocation;
            line.LineTotal       = line.Quantity * line.UnitPrice;
            line.CreatedAt       = DateTime.UtcNow;
            line.InActive        = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "Stock Transfer created successfully",
            id      = entity.Id,
            docNo   = entity.DocNo,
            number  = entity.Number,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StockTransferUpdateRequest request)
    {
        var doc = _repository.GetAll()
            .Include(s => s.Lines)
            .FirstOrDefault(s => s.Id == id);

        if (doc == null) return NotFound($"Stock Transfer {id} not found");

        if (request.FromWarehouse == request.ToWarehouse)
            return BadRequest("From Warehouse and To Warehouse must be different");

        _mapper.Map(request, doc);
        doc.TotalQuantity  = request.Lines.Sum(l => l.Quantity);
        doc.UpdatedAt      = DateTime.UtcNow;
        doc.VersionNum    += 1;

        doc.Lines.Clear();
        int lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<StockTransferLine>(lr);
            line.LineNum         = lineNum++;
            line.ItemNo          = lr.ItemNo.Trim();
            line.ItemDescription = lr.ItemDescription ?? lr.ItemNo;
            line.FromWarehouse   = string.IsNullOrEmpty(lr.FromWarehouse) ? request.FromWarehouse : lr.FromWarehouse;
            line.ToWarehouse     = string.IsNullOrEmpty(lr.ToWarehouse)   ? request.ToWarehouse   : lr.ToWarehouse;
            line.ToBinLocation   = string.IsNullOrEmpty(lr.ToBinLocation) ? request.ToBinLocation : lr.ToBinLocation;
            line.LineTotal       = line.Quantity * line.UnitPrice;
            line.CreatedAt       = DateTime.UtcNow;
            line.InActive        = false;
            doc.Lines.Add(line);
        }

        _repository.Update(doc);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var doc = _repository.GetSingle(s => s.Id == id);
        if (doc == null) return NotFound($"Stock Transfer {id} not found");

        doc.DeletedAt = DateTime.UtcNow;
        _repository.Remove(doc);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // COPY FROM — pull lines from a Stock Transfer Request
    // Returns a pre-filled StockTransfer payload (not saved yet)
    // so the frontend can show the form pre-populated
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpPost("CopyFrom")]
    public IActionResult CopyFrom([FromBody] CopyFromRequest request)
    {
        if (_strRepository == null)
            return BadRequest("Stock Transfer Request repository not available");

        if (request.SourceDocType != "KSTR")
            return BadRequest($"Unsupported source document type '{request.SourceDocType}'");

        var source = _strRepository.GetAll()
            .Include(s => s.Lines)
            .FirstOrDefault(s => s.Id == request.SourceDocEntry);

        if (source == null)
            return NotFound($"Stock Transfer Request {request.SourceDocEntry} not found");

        if (source.Status == "X")
            return BadRequest("Cannot copy from a Cancelled Stock Transfer Request");

        var today = DateTime.UtcNow.Date;

        // Return a pre-filled request for the frontend to show
        var result = new
        {
            // Header
            series          = source.Series,
            postingDate     = today,
            documentDate    = today,
            cardCode        = source.CardCode,
            cardName        = source.CardName,
            contactPerson   = source.ContactPerson,
            shipTo          = source.ShipTo,
            fromWarehouse   = source.FromWarehouse,
            toWarehouse     = source.ToWarehouse,
            priceList       = source.PriceList,
            referencedDoc   = source.DocNo,
            baseDocEntry    = source.Id,
            baseDocType     = "KSTR",
            salesEmployee   = source.SalesEmployee,
            journalRemarks  = $"Stock Transfers - (From {source.DocNo})",
            remarks         = source.Remarks,
            // Lines
            lines = source.Lines.Select((l, i) => new
            {
                lineNum         = i + 1,
                itemNo          = l.ItemNo,
                itemDescription = l.ItemDescription,
                fromWarehouse   = l.FromWarehouse,
                fromBinLocation = (string?)null,
                toWarehouse     = l.ToWarehouse,
                toBinLocation   = (string?)null,
                quantity        = l.Quantity,
                uoMCode        = l.UoMCode,
                uoMName        = l.UoMName,
                unitPrice      = l.UnitPrice,
                firstPrice     = l.UnitPrice,
                remarks        = l.Remarks,
            }).ToList(),
            // Meta
            sourceDocNo  = source.DocNo,
            sourceStatus = source.Status,
        };

        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    private string GenerateDocNo()
    {
        var year   = DateTime.Now.Year;
        var prefix = $"STO-{year}-";
        var last   = _repository.GetAll()
            .Where(s => s.DocNo.StartsWith(prefix))
            .OrderByDescending(s => s.Id)
            .FirstOrDefault();

        if (last == null) return $"{prefix}00001";
        var parts = last.DocNo.Split('-');
        return parts.Length >= 3 && int.TryParse(parts[2], out int n)
            ? $"{prefix}{(n + 1):D5}"
            : $"{prefix}00001";
    }

    private int GenerateNumber()
    {
        var last = _repository.GetAll()
            .OrderByDescending(s => s.Number)
            .FirstOrDefault();
        return (last?.Number ?? 0) + 1;
    }
}