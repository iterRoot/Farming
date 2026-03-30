
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Sheep;
public class SheepController : MyController

{
    private readonly IMapper _mapper;
	private readonly ISheepRepository _repository;
    public SheepController(
		ISheepRepository repository,
		IMapper mapper
		// ICloudStorageSingletonService service
	)
	{
		_mapper = mapper;
		_repository = repository;
		// _service = service;
	}

	[AllowAnonymous]
		[HttpGet ("Sheep")]
	public IActionResult Gets()
	{
		var iQueryable = _repository.GetAll();
		var results = _mapper.ProjectTo<SheepListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	[HttpGet("Sheep{id:int}")]
	public IActionResult Get(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<SheepListResponse>(item);
		return Ok(result);
	}




	[HttpPost]
	public IActionResult CreatSheep([FromForm] SheepListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var cow = _mapper.Map<Sheep>(request);
		cow.CreatedAt = DateTime.UtcNow;
		cow.InActive = false;
		_repository.Add(cow);
		_repository.Commit();
		var response = _mapper.Map<SheepListResponse>(cow);
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}
// 	[HttpPost]
// public IActionResult CreatSheep([FromBody] SheepListRequest request)
// {
//     if (!ModelState.IsValid) return BadRequest(ModelState);

//     if (request.HouseId.HasValue)
//     {
//         var exists = _houseRepository.GetAll().Any(h => h.Id == request.HouseId.Value);
//         if (!exists) return BadRequest(new { message = "HouseId not found." });
//     }

//     var entity = _mapper.Map<Sheep>(request);
//     entity.CreatedAt = DateTime.UtcNow;
//     _repository.Add(entity);
//     _repository.Commit();
//     return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
// }


	// [HttpPost]
	// public IActionResult CreatSheep()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<Sheep>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromForm] SheepUpdateRequest request)
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