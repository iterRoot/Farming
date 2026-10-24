
// using AutoMapper;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using FarmingApi.Core;

// using FarmingApi;

// namespace FarmingApi.Modules.Salad;
// public class SaladController : MyController

// {
//     private readonly IMapper _mapper;
// 	private readonly ISaladRepository _repository;
//     public SaladController(
// 		ISaladRepository repository,
// 		IMapper mapper
// 		// ICloudStorageSingletonService service
// 	)
// 	{
// 		_mapper = mapper;
// 		_repository = repository;
// 		// _service = service;
// 	}

// 	[AllowAnonymous]
// 		[HttpGet ("Salad")]
// 	public IActionResult Gets()
// 	{
// 		var iQueryable = _repository.GetAll();
// 		var results = _mapper.ProjectTo<SaladListResponse>(iQueryable).ToList();

// 		return Ok(results);
// 	}

// 	[HttpGet("Salad{id:int}")]
// 	public IActionResult Get(int id)
// 	{
// 		var item = _repository.GetSingle(e => e.Id == id);
// 		if (item == null)
// 		{
// 			return BadRequest($"Item not found {id}");
// 		}
// 		var result = _mapper.Map<SaladListResponse>(item);
// 		return Ok(result);
// 	}




// 	[HttpPost]
// 	public IActionResult CreatSalad([FromForm] SaladListRequest request)
// 	{
// 		if (!ModelState.IsValid)
// 			return BadRequest(ModelState);
// 		var cow = _mapper.Map<Salad>(request);
// 		cow.CreatedAt = DateTime.UtcNow;
// 		cow.InActive = false;
// 		_repository.Add(cow);
// 		_repository.Commit();
// 		var response = _mapper.Map<SaladListResponse>(cow);
// 		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
// 	}

// 	[HttpPut("{id:int}")]
// 	public async Task<IActionResult> Update(int id, [FromForm] SaladUpdateRequest request)
// 	{
// 		var item = _repository.GetSingle(e => e.Id == id);
// 		if (item == null)
// 		{
// 			return BadRequest($"Item not found {id}");
// 		}
// 		_mapper.Map(request, item);
// 		// if (request.Logo != null)
// 		// {
// 		// 	var stream = request.Logo.OpenReadStream();
// 		// 	// var url = await _service.ReplaceFile(item.Logo, stream);
// 		// 	item.Logo = url;
// 		// }
// 		item.UpdatedAt = DateTime.UtcNow;
// 		// item.UpdatedBy = GetClaim()!.Id;
// 		_repository.Update(item);
// 		_repository.Commit();
// 		return NoContent();
// 	}

// 	[HttpDelete]
// 	public IActionResult Delete(int id)
// 	{
// 		var item = _repository.GetSingle(e => e.Id == id);
// 		if (item == null)
// 		{
// 			return BadRequest($"Item not found {id}");
// 		}
// 		item.DeletedAt = DateTime.UtcNow;
// 		// item.DeletedBy = GetClaim()!.Id;
// 		_repository.Remove(item);
// 		_repository.Commit();
// 		return NoContent();
// 	}

// }