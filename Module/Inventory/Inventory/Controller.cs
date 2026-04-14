
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Sale.Inventory;
public class InventoryController : MyController

{
    private readonly IMapper _mapper;
	private readonly IInventoryRepository _repository;
    public InventoryController(
		IInventoryRepository repository,
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
		var results = _mapper.ProjectTo<InventoryListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	// [HttpGet("Inventory{id:int}")]
	[HttpGet("{id:int}")]

	public IActionResult Get(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<InventoryListResponse>(item);
		return Ok(result);
	}

    [HttpPost]
    public IActionResult Create([FromBody] InventoryListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<Inventory>(request);
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
// tubecoffee@kpt



	// [HttpPost]
	// public IActionResult CreatInventory([FromForm] InventoryListRequest request)
	// {
	// 	if (!ModelState.IsValid)
	// 		return BadRequest(ModelState);
	// 	var cow = _mapper.Map<Inventory>(request);
	// 	cow.CreatedAt = DateTime.UtcNow;
	// 	cow.InActive = false;
	// 	_repository.Add(cow);
	// 	_repository.Commit();
	// 	var response = _mapper.Map<InventoryListResponse>(cow);
	// 	return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	// }
// 	[HttpPost]
// public IActionResult CreatInventory([FromBody] InventoryListRequest request)
// {
//     if (!ModelState.IsValid) return BadRequest(ModelState);

//     if (request.HouseId.HasValue)
//     {
//         var exists = _houseRepository.GetAll().Any(h => h.Id == request.HouseId.Value);
//         if (!exists) return BadRequest(new { message = "HouseId not found." });
//     }

//     var entity = _mapper.Map<Inventory>(request);
//     entity.CreatedAt = DateTime.UtcNow;
//     _repository.Add(entity);
//     _repository.Commit();
//     return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
// }


	// [HttpPost]
	// public IActionResult CreatInventory()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<Inventory>(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

	// [HttpPut("{id:int}")]
	[HttpPut("{id}")]
	public IActionResult Update(int id, [FromForm] InventoryUpdateRequest request)
	{
		var item = _repository.GetSingle(e => e.Id == id);

		if (item == null)
			return NotFound($"Item not found: {id}");

		_mapper.Map(request, item);

		item.UpdatedAt = DateTime.UtcNow;

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