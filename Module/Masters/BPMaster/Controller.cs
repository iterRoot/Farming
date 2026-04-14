
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;

using FarmingApi;
namespace FarmingApi.Modules.Master.UserMaster;
public class UserMasterController : MyController
{
    private readonly IMapper _mapper;
    private readonly IUserMasterRepository _repository;
    public UserMasterController(IUserMasterRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets(){
        var iQueryable = _repository.GetAll();
        var results = _mapper.ProjectTo<UserMasterResponse>(iQueryable).ToList();
        return Ok(results);

    }
    [HttpGet("id:int")]
    public IActionResult Get(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
        {
            return BadRequest($"User Not Found {id}");
        }
        var result = _mapper.Map<UserMasterResponse>(user);
        return Ok(result);


    }
    [HttpPost]
    public IActionResult Create([FromBody] UserMasterInsertrequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = _mapper.Map<UserMaster>(request);
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
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UserMasterUpdaterequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null) return NotFound ($"Item not found: {id}");
        _mapper.Map(request, user);
        user.UpdatedAt = DateTime.UtcNow;
        _repository.Update(user);
        _repository.Commit();
        return NoContent(); 
    }
    [HttpDelete]
	public IActionResult Delete(int id)
	{
		var user = _repository.GetSingle(e => e.Id == id);
		if (user == null)
		{
			return BadRequest($"Item not found {id}");
		}
		user.DeletedAt = DateTime.UtcNow;
		// item.DeletedBy = GetClaim()!.Id;
		_repository.Remove(user);
		_repository.Commit();
		return NoContent();
	}

}