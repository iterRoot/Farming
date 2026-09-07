using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FarmingApi.Core;

namespace FarmingApi.Modules.Inventory.BarCode;

public class BarCodeController : MyController
{
    private readonly IMapper             _mapper;
    private readonly IBarCodeRepository  _repository;

    public BarCodeController(IBarCodeRepository repository, IMapper mapper)
    {
        _mapper     = mapper;
        _repository = repository;
    }

    // ═══════════════════════════════════════════════════════════════
    // GET ALL
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets([FromQuery] string? itemNo = null, [FromQuery] string? uomGroup = null, [FromQuery] string? barCodeType = null)
    {
        var query = _repository.GetAll().Include(b => b.Lines).AsQueryable();

        if (!string.IsNullOrEmpty(itemNo))      query = query.Where(b => b.ItemNo.Contains(itemNo));
        if (!string.IsNullOrEmpty(uomGroup))    query = query.Where(b => b.UoMGroup == uomGroup);
        if (!string.IsNullOrEmpty(barCodeType)) query = query.Where(b => b.BarCodeType == barCodeType);

        var list   = query.OrderBy(b => b.ItemNo).ToList();
        var result = list.Select(b => MapWithComputed(b)).ToList();
        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ID
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var barcode = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (barcode == null) return NotFound($"BarCode {id} not found");
        return Ok(MapWithComputed(barcode));
    }

    // ═══════════════════════════════════════════════════════════════
    // GET BY ITEM NO
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("ByItem/{itemNo}")]
    public IActionResult GetByItem(string itemNo)
    {
        var barcode = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.ItemNo == itemNo);

        if (barcode == null) return NotFound($"No barcode found for item '{itemNo}'");
        return Ok(MapWithComputed(barcode));
    }

    // ═══════════════════════════════════════════════════════════════
    // FIND BY BARCODE VALUE — scan barcode to find item
    // ═══════════════════════════════════════════════════════════════
    [AllowAnonymous]
    [HttpGet("Find/{code}")]
    public IActionResult FindByCode(string code)
    {
        var line = _repository.GetAll()
            .Include(b => b.Lines)
            .SelectMany(b => b.Lines.Select(l => new { b, l }))
            .FirstOrDefault(x => x.l.Code == code);

        if (line == null)
            return NotFound($"No item found for barcode '{code}'");

        return Ok(new BarCodeSearchResponse
        {
            ItemNo          = line.b.ItemNo,
            ItemDescription = line.b.ItemDescription,
            UoMGroup        = line.b.UoMGroup,
            BarCodeType     = line.b.BarCodeType,
            Code            = line.l.Code,
            UoM             = line.l.UoM,
            FreeText        = line.l.FreeText,
            IsDefault       = line.l.IsDefault,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPost]
    public IActionResult Create([FromBody] BarCodeCreateRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (string.IsNullOrWhiteSpace(request.ItemNo))
            return BadRequest("Item No. is required");

        var barCodeType = NormalizeType(request.BarCodeType, out var typeError);
        if (typeError != null) return BadRequest(typeError);

        // Check duplicate item PER type
        var exists = _repository.GetAll().Any(b => b.ItemNo == request.ItemNo && b.BarCodeType == barCodeType);
        if (exists) return BadRequest($"{barCodeType} barcodes for item '{request.ItemNo}' already exist. Use Update instead.");

        // Validate no duplicate codes
        var codes = request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Code)).Select(l => l.Code).ToList();
        if (codes.Count != codes.Distinct().Count())
            return BadRequest("Duplicate barcode values in lines");

        var entity = _mapper.Map<BarCode>(request);
        entity.BarCodeType = barCodeType;
        entity.VersionNum = 1;
        entity.CreatedAt  = DateTime.UtcNow;
        entity.InActive   = false;

        int lineNum = 1;
        bool hasDefault = request.Lines.Any(l => l.IsDefault);
        foreach (var lr in request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Code)))
        {
            var line = _mapper.Map<BarCodeLine>(lr);
            line.LineNum   = lineNum++;
            // Auto-set first line as default if none specified
            if (!hasDefault && lineNum == 2) line.IsDefault = true;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            entity.Lines.Add(line);
        }

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message     = "BarCode created successfully",
            id          = entity.Id,
            itemNo      = entity.ItemNo,
            barCodeType = entity.BarCodeType,
            total       = entity.Lines.Count,
        });
    }

    // ═══════════════════════════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════════════════════════
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] BarCodeUpdateRequest request)
    {
        var barcode = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (barcode == null) return NotFound($"BarCode {id} not found");

        // Normalize + guard type change against another record of the same item/type
        if (request.BarCodeType != null)
        {
            var newType = NormalizeType(request.BarCodeType, out var typeError);
            if (typeError != null) return BadRequest(typeError);

            if (newType != barcode.BarCodeType &&
                _repository.GetAll().Any(b => b.Id != id && b.ItemNo == barcode.ItemNo && b.BarCodeType == newType))
                return BadRequest($"{newType} barcodes for item '{barcode.ItemNo}' already exist.");

            barcode.BarCodeType = newType;
        }

        _mapper.Map(request, barcode);
        barcode.UpdatedAt   = DateTime.UtcNow;
        barcode.VersionNum += 1;

        barcode.Lines.Clear();
        int lineNum = 1;
        foreach (var lr in request.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Code)))
        {
            var line = _mapper.Map<BarCodeLine>(lr);
            line.LineNum   = lineNum++;
            line.CreatedAt = DateTime.UtcNow;
            line.InActive  = false;
            barcode.Lines.Add(line);
        }

        // Ensure exactly one default
        EnsureSingleDefault(barcode);

        _repository.Update(barcode);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // SET DEFAULT — mark one line as the default barcode
    // ═══════════════════════════════════════════════════════════════
    [HttpPost("{id:int}/SetDefault/{lineId:int}")]
    public IActionResult SetDefault(int id, int lineId)
    {
        var barcode = _repository.GetAll()
            .Include(b => b.Lines)
            .FirstOrDefault(b => b.Id == id);

        if (barcode == null) return NotFound($"BarCode {id} not found");

        var targetLine = barcode.Lines.FirstOrDefault(l => l.Id == lineId);
        if (targetLine == null) return NotFound($"Line {lineId} not found");

        foreach (var line in barcode.Lines)
            line.IsDefault = (line.Id == lineId);

        barcode.UpdatedAt = DateTime.UtcNow;
        _repository.Update(barcode);
        _repository.Commit();

        return Ok(new { message = $"'{targetLine.Code}' set as default barcode", code = targetLine.Code });
    }

    // ═══════════════════════════════════════════════════════════════
    // DELETE
    // ═══════════════════════════════════════════════════════════════
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var barcode = _repository.GetSingle(b => b.Id == id);
        if (barcode == null) return NotFound($"BarCode {id} not found");

        barcode.DeletedAt = DateTime.UtcNow;
        _repository.Remove(barcode);
        _repository.Commit();
        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════
    // Allowed barcode types
    private static readonly string[] AllowedTypes = { "Sale", "Purchase", "Inventory" };

    // Normalize/validate the type. Null or empty defaults to "Sale".
    private static string NormalizeType(string? raw, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(raw)) return "Sale";

        var match = AllowedTypes.FirstOrDefault(t => t.Equals(raw.Trim(), StringComparison.OrdinalIgnoreCase));
        if (match == null)
        {
            error = $"Invalid BarCodeType '{raw}'. Allowed: {string.Join(", ", AllowedTypes)}";
            return "Sale";
        }
        return match;
    }

    private BarCodeResponse MapWithComputed(BarCode b)
    {
        var response = _mapper.Map<BarCodeResponse>(b);
        response.TotalCodes  = b.Lines.Count;
        response.DefaultCode = b.Lines.FirstOrDefault(l => l.IsDefault)?.Code
                            ?? b.Lines.FirstOrDefault()?.Code;
        return response;
    }

    private static void EnsureSingleDefault(BarCode barcode)
    {
        var defaults = barcode.Lines.Where(l => l.IsDefault).ToList();
        if (defaults.Count == 0 && barcode.Lines.Any())
            barcode.Lines.First().IsDefault = true;
        else if (defaults.Count > 1)
        {
            foreach (var l in defaults.Skip(1))
                l.IsDefault = false;
        }
    }
}