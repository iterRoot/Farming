using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using RunEntity     = FarmingApi.Modules.Financials.FixedAssets.DepreciationRun.DepreciationRun;
using RunLineEntity = FarmingApi.Modules.Financials.FixedAssets.DepreciationRun.DepreciationRunLine;

namespace FarmingApi.Modules.Financials.FixedAssets;

// ═══════════════════════════════════════════════════════════════
// ASSET MASTER DATA  (route: /AssetsMaster)
// GET    /AssetsMaster        → all assets
// GET    /AssetsMaster/{id}   → one
// POST   /AssetsMaster        → create
// PUT    /AssetsMaster/{id}   → update
// DELETE /AssetsMaster/{id}   → delete
//
// Once a posted depreciation run includes an asset, its cost, salvage,
// life, method, capitalization date, accumulated depreciation and
// depreciation accounts are locked — the run history and the ledger
// depend on them. Reversing the runs unlocks them again.
// ═══════════════════════════════════════════════════════════════
[Route("AssetsMaster")]
public class AssetsMasterController : MyController
{
    private readonly IAssetMasterRepository _repo;
    private readonly IMapper _mapper;
    private readonly MyDbContext _db;

    public AssetsMasterController(IAssetMasterRepository repo, IMapper mapper, MyDbContext db)
    {
        _repo = repo;
        _mapper = mapper;
        _db = db;
    }

    private static readonly string[] ManageBy   = { "None", "Serial Numbers", "Batches" };
    private static readonly string[] Methods     = { "Straight Line", "Declining Balance", "Immediate Write-off", "No Depreciation" };
    private static readonly string[] Statuses    = { "New", "Active", "Inactive", "Deactivated" };

    [HttpGet]
    public IActionResult GetAll()
    {
        var list = _repo.GetAll().OrderBy(x => x.ItemNo).ToList();
        var locked = AssetsWithPostedDepreciation();
        var dtos = _mapper.Map<List<AssetMasterDto>>(list);
        foreach (var d in dtos) d.HasPostedDepreciation = locked.Contains(d.Id);
        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetOne(int id)
    {
        var x = _repo.GetSingle(e => e.Id == id);
        if (x == null) return NotFound($"Asset {id} not found.");
        var dto = _mapper.Map<AssetMasterDto>(x);
        dto.HasPostedDepreciation = AssetsWithPostedDepreciation().Contains(id);
        return Ok(dto);
    }

    [HttpPost]
    public IActionResult Create([FromBody] AssetMasterSaveRequest req)
    {
        var err = Validate(req, null);
        if (err != null) return BadRequest(err);

        var e = _mapper.Map<AssetMaster>(req);
        // A new asset with nothing depreciated yet has its whole life ahead of it.
        if (e.RemainingLifeMonths == 0 && e.AccumulatedDepreciation == 0)
            e.RemainingLifeMonths = e.UsefulLifeMonths;
        e.CreatedAt = DateTime.UtcNow;
        e.InActive = !req.Active;
        _repo.Add(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Asset created." });
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] AssetMasterSaveRequest req)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Asset {id} not found.");
        var err = Validate(req, id);
        if (err != null) return BadRequest(err);

        if (AssetsWithPostedDepreciation().Contains(id))
        {
            var changed = LockedFieldChanges(e, req);
            if (changed.Count > 0)
                return BadRequest($"This asset has posted depreciation, so these can't change: {string.Join(", ", changed)}. Reverse its depreciation runs first.");
        }

        _mapper.Map(req, e);
        e.UpdatedAt = DateTime.UtcNow;
        e.InActive = !req.Active;
        _repo.Update(e);
        _repo.Commit();
        return Ok(new { id = e.Id, message = "Asset updated." });
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var e = _repo.GetSingle(x => x.Id == id);
        if (e == null) return NotFound($"Asset {id} not found.");
        if (AssetsWithPostedDepreciation().Contains(id))
            return BadRequest("This asset has posted depreciation and can't be deleted. Set it to Inactive or Deactivated instead.");
        _repo.Remove(e);
        _repo.Commit();
        return NoContent();
    }

    private HashSet<int> AssetsWithPostedDepreciation() =>
        _db.Set<RunLineEntity>()
            .Join(_db.Set<RunEntity>().Where(r => r.Status == "Posted"),
                  l => l.DepreciationRunId, r => r.Id, (l, r) => l.AssetId)
            .Distinct().ToHashSet();

    private static List<string> LockedFieldChanges(AssetMaster e, AssetMasterSaveRequest r)
    {
        var changed = new List<string>();
        if (e.AcquisitionCost != r.AcquisitionCost)                 changed.Add("Acquisition Cost");
        if (e.SalvageValue != r.SalvageValue)                       changed.Add("Salvage Value");
        if (e.UsefulLifeMonths != r.UsefulLifeMonths)               changed.Add("Useful Life");
        if (e.RemainingLifeMonths != r.RemainingLifeMonths)         changed.Add("Remaining Life");
        if (e.DepreciationMethod != r.DepreciationMethod)           changed.Add("Depreciation Method");
        if (e.CapitalizationDate != r.CapitalizationDate)           changed.Add("Capitalization Date");
        if (e.AccumulatedDepreciation != r.AccumulatedDepreciation) changed.Add("Accumulated Depreciation");
        if ((e.DepreciationAccount ?? "") != (r.DepreciationAccount ?? ""))                       changed.Add("Depreciation Account");
        if ((e.AccumulatedDepreciationAccount ?? "") != (r.AccumulatedDepreciationAccount ?? "")) changed.Add("Accumulated Depreciation Account");
        return changed;
    }

    private string? Validate(AssetMasterSaveRequest r, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(r.ItemNo))      return "Item No. is required.";
        if (string.IsNullOrWhiteSpace(r.Description)) return "Description is required.";
        if (!ManageBy.Contains(r.ManageItemBy))       return "Invalid 'Manage Item by' value.";
        if (!Methods.Contains(r.DepreciationMethod))  return "Invalid depreciation method.";
        if (!Statuses.Contains(r.AssetStatus))        return "Invalid asset status.";
        if (r.UsefulLifeMonths < 0)                   return "Useful life cannot be negative.";
        if (r.RemainingLifeMonths < 0)                return "Remaining life cannot be negative.";
        if (r.RemainingLifeMonths > r.UsefulLifeMonths) return "Remaining life can't be longer than useful life.";
        if (r.AcquisitionCost < 0)                    return "Acquisition cost cannot be negative.";
        if (r.SalvageValue < 0)                       return "Salvage value cannot be negative.";
        if (r.SalvageValue > r.AcquisitionCost)       return "Salvage value can't exceed acquisition cost.";
        if (r.AccumulatedDepreciation < 0)            return "Accumulated depreciation cannot be negative.";
        if (r.AccumulatedDepreciation > r.AcquisitionCost - r.SalvageValue)
            return "Accumulated depreciation can't exceed acquisition cost minus salvage value.";

        var code = r.ItemNo.Trim().ToLowerInvariant();
        var clash = _repo.GetAll().AsEnumerable()
            .Any(x => x.ItemNo.ToLowerInvariant() == code && x.Id != ignoreId);
        if (clash) return $"Item No. \"{r.ItemNo}\" already exists.";
        return null;
    }
}
