using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

namespace FarmingApi.Modules.Financials.FixedAssets;

// ═══════════════════════════════════════════════════════════════
// ASSET MASTER DATA  (route: /AssetsMaster)
// GET    /AssetsMaster        → all assets
// GET    /AssetsMaster/{id}   → one
// POST   /AssetsMaster        → create
// PUT    /AssetsMaster/{id}   → update
// DELETE /AssetsMaster/{id}   → delete
// ═══════════════════════════════════════════════════════════════
[Route("AssetsMaster")]
public class AssetsMasterController : MyController
{
    private readonly IAssetMasterRepository _repo;
    private readonly IMapper _mapper;

    public AssetsMasterController(IAssetMasterRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    private static readonly string[] ManageBy   = { "None", "Serial Numbers", "Batches" };
    private static readonly string[] Methods     = { "Straight Line", "Declining Balance", "Immediate Write-off", "No Depreciation" };
    private static readonly string[] Statuses    = { "New", "Active", "Inactive", "Deactivated" };

    [AllowAnonymous]
    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll().OrderBy(x => x.ItemNo).ToList();
        return Ok(_mapper.Map<List<AssetMasterDto>>(list));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Asset {id} not found.");
        return Ok(_mapper.Map<AssetMasterDto>(x));
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Create([FromBody] AssetMasterSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = _mapper.Map<AssetMaster>(req);
        e.CreatedAt = DateTime.UtcNow;
        e.InActive = !req.Active;
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Asset created." });
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AssetMasterSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Asset {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        _mapper.Map(req, e);
        e.UpdatedAt = DateTime.UtcNow;
        e.InActive = !req.Active;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Asset updated." });
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Asset {id} not found.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private string? Validate(AssetMasterSaveRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.ItemNo))      return "Item No. is required.";
        if (string.IsNullOrWhiteSpace(r.Description)) return "Description is required.";
        if (!ManageBy.Contains(r.ManageItemBy))       return "Invalid 'Manage Item by' value.";
        if (!Methods.Contains(r.DepreciationMethod))  return "Invalid depreciation method.";
        if (!Statuses.Contains(r.AssetStatus))        return "Invalid asset status.";
        if (r.UsefulLifeMonths < 0)                   return "Useful life cannot be negative.";

        var code = r.ItemNo.Trim().ToLowerInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.ItemNo.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Item No. \"{r.ItemNo}\" already exists.";
        return null;
    }
}
