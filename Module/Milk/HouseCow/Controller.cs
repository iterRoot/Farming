
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.HouseCow;
public class HouseCowController : MyController

{
    private readonly IMapper _mapper;
	private readonly IHouseCowRepository _repository;
    public HouseCowController(
		IHouseCowRepository repository,
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
		var results = _mapper.ProjectTo<HouseCowListResponse>(iQueryable).ToList();

		return Ok(results);
	}
	[HttpPost]
	public IActionResult CreatHouseCow([FromBody] HouseCowListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var HouseCow = _mapper.Map<HouseCow>(request);
		HouseCow.CreatedAt = DateTime.UtcNow;
		HouseCow.InActive = false;
		_repository.Add(HouseCow);
		_repository.Commit();
		var response = _mapper.Map<HouseCowListResponse>(HouseCow);
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}

	// [HttpPost]
	// public IActionResult CreatHouseCow()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<HouseHouseCow>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

}