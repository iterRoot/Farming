using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.StockTransferRequest;

public class StockTransferRequestController : MyController
{
    private readonly IMapper                          _mapper;
    private readonly IStockTransferRequestRepository  _repository;

    public StockTransferRequestController(
        IStockTransferRequestRepository repository,
        IMapper mapper)
    {
        _mapper     = mapper;
        _repository = repository;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets(
        [FromQuery] string?   status        = null,
        [FromQuery] string?   fromWarehouse = null,
        [FromQuery] string?   toWarehouse   = null,
        [FromQuery] string?   cardCode      = null,
        [FromQuery] DateTime? from          = null,
        [FromQuery] DateTime? to            = null)
    {
        var query = _repository.GetAll().Include(s => s.Lines).AsQueryable();

        if (!string.IsNullOrEmpty(status))        query = query.Where(s => s.Status        == status);
        if (!string.IsNullOrEmpty(fromWarehouse)) query = query.Where(s => s.FromWarehouse == fromWarehouse);
        if (!string.IsNullOrEmpty(toWarehouse))   query = query.Where(s => s.ToWarehouse   == toWarehouse);
        if (!string.IsNullOrEmpty(cardCode))      query = query.Where(s => s.CardCode      == cardCode);
        if (from.HasValue) query = query.Where(s => s.PostingDate >= from.Value);
        if (to.HasValue)   query = query.Where(s => s.PostingDate <= to.Value);

        var result = query.OrderByDescending(s => s.PostingDate)
                          .ThenByDescending(s => s.Id)
                          .ToList();
        return Ok(_mapper.Map<List<StockTransferRequestResponse>>(result));
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

        if (doc == null) return NotFound($"Stock Transfer Request {id} not found");
        return Ok(_mapper.Map<StockTransferRequestResponse>(doc));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] StockTransferRequestCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.Lines == null || !request.Lines.Any())
            return BadRequest("At least one item line is required");

        var missingItems = request.Lines.Where(l => string.IsNullOrWhiteSpace(l.ItemNo)).ToList();
        if (missingItems.Any())
            return BadRequest("All lines must have an Item No.");

        if (request.FromWarehouse == request.ToWarehouse)
            return BadRequest("From Warehouse and To Warehouse must be different");

        var entity = _mapper.Map<StockTransferRequest>(request);
        entity.DocNo         = GenerateDocNo();
        entity.Status        = "O";
        entity.TotalQuantity = request.Lines.Sum(l => l.Quantity);
        entity.VersionNum    = 1;
        entity.CreatedAt     = DateTime.UtcNow;
        entity.InActive      = false;

        // Default journal remarks like SAP
        if (string.IsNullOrEmpty(entity.JournalRemarks))
            entity.JournalRemarks = $"Stock Transfer Request - {entity.DocNo}";

        int lineNum = 1;
        foreach (var lr in request.Lines)
        {
            var line = _mapper.Map<StockTransferRequestLine>(lr);
            line.LineNum      = lineNum++;
            line.ItemNo       = lr.ItemNo.Trim();
            line.ItemDescription = lr.ItemDescription ?? lr.ItemNo;
            // Inherit header warehouse if line doesn't override
            line.FromWarehouse = string.IsNullOrEmpty(lr.FromWarehouse) ? request.FromWarehouse : lr.FromWarehouse;
            line.ToWarehouse   = string.IsNullOrEmpty(lr.ToWarehouse)   ? request.ToWarehouse   : lr.ToWarehouse;
            line.LineTotal    = line.Quantity * line.UnitPrice;
            line.LineStatus   = "O";
            line.CreatedAt    = DateTime.UtcNow;
            line.InActive     = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "Stock Transfer Request created successfully",
            id      = entity.Id,
            docNo   = entity.DocNo,
            status  = entity.Status,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StockTransferRequestUpdateRequest request)
    {
        var doc = _repository.GetAll()
            .Include(s => s.Lines)
            .FirstOrDefault(s => s.Id == id);

        if (doc == null) return NotFound($"Stock Transfer Request {id} not found");
        if (doc.Status == "C") return BadRequest("Cannot edit a Closed document");
        if (doc.Status == "X") return BadRequest("Cannot edit a Cancelled document");

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
            var line = _mapper.Map<StockTransferRequestLine>(lr);
            line.LineNum       = lineNum++;
            line.ItemNo        = lr.ItemNo.Trim();
            line.ItemDescription = lr.ItemDescription ?? lr.ItemNo;
            line.FromWarehouse = string.IsNullOrEmpty(lr.FromWarehouse) ? request.FromWarehouse : lr.FromWarehouse;
            line.ToWarehouse   = string.IsNullOrEmpty(lr.ToWarehouse)   ? request.ToWarehouse   : lr.ToWarehouse;
            line.LineTotal     = line.Quantity * line.UnitPrice;
            line.LineStatus    = "O";
            line.CreatedAt     = DateTime.UtcNow;
            line.InActive      = false;
            doc.Lines.Add(line);
        }

        _repository.Update(doc);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // APPROVE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Approve")]
    public IActionResult Approve(int id)
    {
        var doc = _repository.GetSingle(s => s.Id == id);
        if (doc == null) return NotFound();
        if (doc.Status != "O") return BadRequest($"Only Open requests can be approved (status: {doc.Status})");

        doc.Status    = "A";
        doc.UpdatedAt = DateTime.UtcNow;
        _repository.Update(doc);
        _repository.Commit();
        return Ok(new { message = "Stock Transfer Request approved", docNo = doc.DocNo });
    }

    // ═══════════════════════════════════════════════════════════════
    // CLOSE — mark as delivered/completed
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Close")]
    public IActionResult Close(int id)
    {
        var doc = _repository.GetSingle(s => s.Id == id);
        if (doc == null) return NotFound();
        if (doc.Status == "C") return BadRequest("Already closed");
        if (doc.Status == "X") return BadRequest("Document is cancelled");

        doc.Status    = "C";
        doc.UpdatedAt = DateTime.UtcNow;
        _repository.Update(doc);
        _repository.Commit();
        return Ok(new { message = "Stock Transfer Request closed", docNo = doc.DocNo });
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/Cancel")]
    public IActionResult Cancel(int id)
    {
        var doc = _repository.GetSingle(s => s.Id == id);
        if (doc == null) return NotFound();
        if (doc.Status == "C") return BadRequest("Cannot cancel a closed document");
        if (doc.Status == "X") return BadRequest("Already cancelled");

        doc.Status    = "X";
        doc.UpdatedAt = DateTime.UtcNow;
        _repository.Update(doc);
        _repository.Commit();
        return Ok(new { message = "Stock Transfer Request cancelled" });
    }

    // ═══════════════════════════════════════════════════════════════
    // COPY TO — duplicate this request into a new one
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/CopyTo")]
    public IActionResult CopyTo(int id)
    {
        var source = _repository.GetAll()
            .Include(s => s.Lines)
            .FirstOrDefault(s => s.Id == id);

        if (source == null) return NotFound();

        var today = DateTime.UtcNow.Date;
        var copy  = new StockTransferRequest
        {
            DocNo           = GenerateDocNo(),
            Series          = source.Series,
            Status          = "O",
            PostingDate     = today,
            DueDate         = today,
            DocumentDate    = today,
            CardCode        = source.CardCode,
            CardName        = source.CardName,
            ContactPerson   = source.ContactPerson,
            ShipTo          = source.ShipTo,
            FromWarehouse   = source.FromWarehouse,
            ToWarehouse     = source.ToWarehouse,
            PriceList       = source.PriceList,
            SalesEmployee   = source.SalesEmployee,
            JournalRemarks  = $"Stock Transfer Request - (Copy of {source.DocNo})",
            PickPackRemarks = source.PickPackRemarks,
            Remarks         = source.Remarks,
            TotalQuantity   = source.TotalQuantity,
            VersionNum      = 1,
            CreatedAt       = DateTime.UtcNow,
            InActive        = false,
        };

        int lineNum = 1;
        foreach (var sl in source.Lines)
        {
            copy.Lines.Add(new StockTransferRequestLine
            {
                LineNum         = lineNum++,
                ItemNo          = sl.ItemNo,
                ItemDescription = sl.ItemDescription,
                FromWarehouse   = sl.FromWarehouse,
                ToWarehouse     = sl.ToWarehouse,
                Quantity        = sl.Quantity,
                UoMCode        = sl.UoMCode,
                UoMName        = sl.UoMName,
                UnitPrice      = sl.UnitPrice,
                LineTotal      = sl.LineTotal,
                LineStatus     = "O",
                Remarks        = sl.Remarks,
                CreatedAt      = DateTime.UtcNow,
                InActive       = false,
            });
        }

        _repository.Add(copy);
        _repository.Commit();

        return Ok(new
        {
            message    = $"Copied from {source.DocNo}",
            id         = copy.Id,
            docNo      = copy.DocNo,
            sourceDocNo= source.DocNo,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var doc = _repository.GetSingle(s => s.Id == id);
        if (doc == null) return NotFound();
        if (doc.Status == "C") return BadRequest("Cannot delete a Closed document");

        doc.DeletedAt = DateTime.UtcNow;
        _repository.Remove(doc);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPER — Generate STR-2026-00001
    // ═══════════════════════════════════════════════════════════════
    private string GenerateDocNo()
    {
        var year   = DateTime.Now.Year;
        var prefix = $"STR-{year}-";
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
}