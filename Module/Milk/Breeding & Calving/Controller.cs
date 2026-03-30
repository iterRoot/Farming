
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Breeding;
public class BreedingController : MyController

{
    private readonly IMapper _mapper;
	private readonly IBreedingRepository _repository;
    public BreedingController(
		IBreedingRepository repository,
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
		var results = _mapper.ProjectTo<BreedingListResponse>(iQueryable).ToList();

		return Ok(results);
	}
	[HttpPost]
	public IActionResult CreatBreeding([FromBody] BreedingListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var Breeding = _mapper.Map<Breeding>(request);
		Breeding.CreatedAt = DateTime.UtcNow;
		Breeding.InActive = false;
		_repository.Add(Breeding);
		_repository.Commit();
		var response = _mapper.Map<BreedingListResponse>(Breeding);
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}

	// [HttpPost]
	// public IActionResult CreatBreeding()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<HouseBreeding>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

}