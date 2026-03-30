
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.CowFeed;
public class CowFeedController : MyController

{
    private readonly IMapper _mapper;
	private readonly ICowFeedRepository _repository;
    public CowFeedController(
		ICowFeedRepository repository,
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
		var results = _mapper.ProjectTo<CowFeedListResponse>(iQueryable).ToList();

		return Ok(results);
	}
	[HttpPost]
	public IActionResult CreatCowFeed([FromBody] CowFeedListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var cow = _mapper.Map<CowFeed>(request);
		cow.CreatedAt = DateTime.UtcNow;
		cow.InActive = false;
		_repository.Add(cow);
		_repository.Commit();
		var response = _mapper.Map<CowFeedListResponse>(cow);
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}

	// [HttpPost]
	// public IActionResult CreatCowFeed()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<CowFeed>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }


	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromForm] CowFeedUpdateRequest request)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		_mapper.Map(request, item);
		// if (request.Logo != null)
		// {
		// 	var stream = request.Logo.OpenReadStream();
		// 	// var url = await _service.ReplaceFile(item.Logo, stream);
		// 	item.Logo = url;
		// }
		item.UpdatedAt = DateTime.UtcNow;
		// item.UpdatedBy = GetClaim()!.Id;
		_repository.Update(item);
		_repository.Commit();
		return NoContent();
	}

	[HttpDelete]
	public IActionResult Delete(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		item.DeletedAt = DateTime.UtcNow;
		// item.DeletedBy = GetClaim()!.Id;
		_repository.Remove(item);
		_repository.Commit();
		return NoContent();
	}

}