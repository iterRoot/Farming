
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Branch;
public class BranchController : MyController

{
    private readonly IMapper _mapper;
	private readonly IBranchRepository _repository;
    public BranchController(
		IBranchRepository repository,
		IMapper mapper
		// ICloudStorageSingletonService service
	)
	{
		_mapper = mapper;
		_repository = repository;
		// _service = service;
	}

	[AllowAnonymous]
		[HttpGet]
	public IActionResult Gets()
	{
		var iQueryable = _repository.GetAll();
		var results = _mapper.ProjectTo<BranchListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	// [HttpGet("Branch{id:int}")]
	[HttpGet("{id:int}")]

	public IActionResult Get(int id)
	{
		var branch = _repository.GetSingle(e => e.Id == id);
		if (branch == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<BranchListResponse>(branch);
		return Ok(result);
	}

    [HttpPost]
    public IActionResult Create([FromBody] BranchListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<Branch>(request);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        _repository.Add(entity);
        _repository.Commit();

		return Ok(new
		{
			message = "Planting saved successfully",
			id = entity.Id
		});
    }

	[HttpPut("{id}")]
	public IActionResult Update(int id, [FromForm] BranchUpdateRequest request)
	{
		var branch = _repository.GetSingle(e => e.Id == id);

		if (branch == null)
			return NotFound($"Item not found: {id}");

		_mapper.Map(request, branch);

		branch.UpdatedAt = DateTime.UtcNow;

		_repository.Update(branch);
		_repository.Commit();

		return NoContent();
	}

	[HttpDelete]
	public IActionResult Delete(int id)
	{
		var branch = _repository.GetSingle(e => e.Id == id);
		if (branch == null)
		{
			return BadRequest($"Item not found {id}");
		}
		branch.DeletedAt = DateTime.UtcNow;
		// branch.DeletedBy = GetClaim()!.Id;
		_repository.Remove(branch);
		_repository.Commit();
		return NoContent();
	}

}