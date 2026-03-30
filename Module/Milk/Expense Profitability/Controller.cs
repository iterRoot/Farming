
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Expense;
public class ExpenseController : MyController

{
    private readonly IMapper _mapper;
	private readonly IExpenseRepository _repository;
    public ExpenseController(
		IExpenseRepository repository,
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
		var results = _mapper.ProjectTo<ExpenseListResponse>(iQueryable).ToList();

		return Ok(results);
	}
	[HttpPost]
	public IActionResult CreatExpense ([FromBody] ExpenseListRequest request)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		var Expense  = _mapper.Map<Expense>(request);
		Expense .CreatedAt = DateTime.UtcNow;
		Expense .InActive = false;
		_repository.Add(Expense );
		_repository.Commit();
		var response = _mapper.Map<ExpenseListResponse>(Expense );
		return CreatedAtAction(nameof(Gets), new { id = response.Id}, response);
	}

	// [HttpPost]
	// public IActionResult CreatExpense ()
	// {

	// 	var existed = repository.Existed(e=> e.Id == request.Id);
	// 	if(existed) return Existed(request.id);
	// 	var item = mapper.Map<HouseExpense >(request);
	// 	item.CreatedAt = DateTime.UtcNow;
    //     item.InActive = false;
    //     // item.CreatedBy = GetClaim()!.Id;
    //     repository.Add(item);
    //     repository.Commit();
    //     return Ok();

	// }

}