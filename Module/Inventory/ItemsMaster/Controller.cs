using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Modules.Master.ItemsMaster;

[ApiController]
[Route("[controller]")]
public class ItemsMasterController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IItemsMasterRepository _repository;

    public ItemsMasterController(IItemsMasterRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    // GET ALL
    [HttpGet]
    public IActionResult Gets()
    {
        var data = _repository.GetAll();
        var results = _mapper.ProjectTo<ItemsMasterResponse>(data).ToList();
        return Ok(results);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound();

        return Ok(_mapper.Map<ItemsMasterResponse>(item));
    }

    // CREATE
    [HttpPost]
    public IActionResult Create([FromBody] ItemsMasterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ItemCode) ||
            string.IsNullOrWhiteSpace(request.ItemName))
        {
            return BadRequest("ItemCode and ItemName are required");
        }

        var exists = _repository.GetAll()
            .Any(x => x.ItemCode == request.ItemCode);

        if (exists)
        {
            return BadRequest("Item Code already exists");
        }

        var entity = _mapper.Map<ItemsMaster>(request);
        entity.CreatedAt = DateTime.UtcNow;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(entity);
    }

    // UPDATE
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] ItemsMasterUpdateRequest request)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound();

        _mapper.Map(request, item);
        item.UpdatedAt = DateTime.UtcNow;

        _repository.Update(item);
        _repository.Commit();

        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var item = _repository.GetSingle(e => e.Id == id);
        if (item == null) return NotFound();

        _repository.Remove(item);
        _repository.Commit();

        return NoContent();
    }
}