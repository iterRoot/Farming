
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Administration.SetUp.UserGroup;
public class UserGroupController : MyController

{
    private readonly IMapper _mapper;
	private readonly IUserGroupRepository _repository;
    public UserGroupController(
		IUserGroupRepository repository,
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
		var results = _mapper.ProjectTo<UserGroupListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	// [HttpGet("UserGroup{id:int}")]
	[HttpGet("{id:int}")]

	public IActionResult Get(int id)
	{
		var UserGroup = _repository.GetSingle(e => e.Id == id);
		if (UserGroup == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<UserGroupListResponse>(UserGroup);
		return Ok(result);
	}

    [HttpPost]
    public IActionResult Create([FromBody] UserGroupListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<UserGroup>(request);
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
	public IActionResult Update(int id, [FromForm] UserGroupUpdateRequest request)
	{
		var UserGroup = _repository.GetSingle(e => e.Id == id);

		if (UserGroup == null)
			return NotFound($"Item not found: {id}");

		_mapper.Map(request, UserGroup);

		UserGroup.UpdatedAt = DateTime.UtcNow;

		_repository.Update(UserGroup);
		_repository.Commit();

		return NoContent();
	}

	[HttpDelete]
	public IActionResult Delete(int id)
	{
		var UserGroup = _repository.GetSingle(e => e.Id == id);
		if (UserGroup == null)
		{
			return BadRequest($"Item not found {id}");
		}
		UserGroup.DeletedAt = DateTime.UtcNow;
		// UserGroup.DeletedBy = GetClaim()!.Id;
		_repository.Remove(UserGroup);
		_repository.Commit();
		return NoContent();
	}

}