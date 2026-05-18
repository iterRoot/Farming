using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;

namespace FarmingApi.Modules.Inventory.GoodsReceipt;

public class GoodsReceiptController : MyController
{
    private readonly IMapper _mapper;
    private readonly IGoodsReceiptRepository _repository;

    public GoodsReceiptController(
        IGoodsReceiptRepository repository,
        IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    [AllowAnonymous]
    [HttpGet]
	public IActionResult Gets()
	{
		var iQueryable = _repository.GetAll();
		var results = _mapper.ProjectTo<GoodsReceiptListResponse>(iQueryable).ToList();

		return Ok(results);
	}
        [HttpPost]
    public IActionResult Create([FromBody] GoodsReceiptInsertListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<GoodsReceipt>(request);
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
}