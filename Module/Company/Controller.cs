using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
// using FarmingApi.Services;

namespace FarmingApi.Modules.Company;

public class CompanyController : MyController
{
	private readonly IMapper _mapper;
	private readonly ICompanyRepository _repository;
	// private readonly ICloudStorageSingletonService _service;

	public CompanyController(
		ICompanyRepository repository,
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
		var results = _mapper.ProjectTo<CompanyListResponse>(iQueryable).ToList();

		return Ok(results);
	}

	[HttpGet("{id:int}")]
	public IActionResult Get(int id)
	{
		var item = _repository.GetSingle(e => e.Id == id);
		if (item == null)
		{
			return BadRequest($"Item not found {id}");
		}
		var result = _mapper.Map<CompanyDetailResponse>(item);
		return Ok(result);
	}

	[HttpPost]
	public async Task<IActionResult> Insert([FromForm] CompanyInsertRequest request)
	{
		var item = _mapper.Map<Company>(request);
		// if (request.Logo is not null)
		// {
		// 	var url = await _service.UploadFile(request.Logo.OpenReadStream());
		// 	item.Logo = url;
		// }
		item.CreatedAt = DateTime.UtcNow;
		// item.CreatedBy = GetClaim()!.Id;
		_repository.Add(item);
		_repository.Commit();
		return NoContent();
	}

	[HttpPut("{id:int}")]
	public async Task<IActionResult> Update(int id, [FromForm] CompanyUpdateRequest request)
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