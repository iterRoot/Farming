
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.MilkCow;
public class CowController : MyController

{
    private readonly IMapper _mapper;
	private readonly ICowRepository _repository;
    public CowController(
		ICowRepository repository,
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
		var results = _mapper.ProjectTo<CowListResponse>(iQueryable).ToList();

		return Ok(results);
	}
	[HttpPost]
	public IActionResult CreatCow([FromBody] CowListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var cow = _mapper.Map<Cow>(request);
		cow.CreatedAt = DateTime.UtcNow;
		cow.InActive = false;
		_repository.Add(cow);
		_repository.Commit();
		var response = _mapper.Map<CowListResponse>(cow);
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}

	// [HttpPost]
	// public IActionResult CreatCow()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<Cow>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

}