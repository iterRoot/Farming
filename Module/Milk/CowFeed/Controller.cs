
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

}