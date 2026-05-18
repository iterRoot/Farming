using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
namespace FarmingApi.Modules.Company.Staff;

public class StaffController : MyController
{
    private readonly IMapper _mapper;
    private readonly IStaffRepository _repository;

    public StaffController(IStaffRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;

    }
    [AllowAnonymous]
    [HttpGet]

    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll();
        var result = _mapper.ProjectTo<StaffListResponse>(iQueryable).ToList();
        return Ok(result);

    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if (staff == null)
        {
            return BadRequest($"Item Not Found {id}");

        } 
        var result = _mapper.Map<StaffListResponse>(staff);
        return Ok(result);

    }

    [HttpPost]
    public async Task<IActionResult> Insert([FromForm] StaffInsertRequest request)
    {
        var staff = _mapper.Map<Staff>(request);
        staff.CreatedAt = DateTime.UtcNow;
        _repository.Add(staff);
        _repository.Commit();
        return NoContent();
    }
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromForm] StaffUpdateRequest request)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if(staff == null)
        {
            return BadRequest($"Item not found {id}");

        }
        _mapper.Map(request, staff);
        staff.UpdatedAt = DateTime.UtcNow;
        _repository.Update(staff);
        _repository.Commit();
        return NoContent();

    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if (staff == null)
        {
            return BadRequest($"Item not Found {id}");

        }
        staff.DeletedAt = DateTime.UtcNow;
        _repository.Remove(staff);
        _repository.Commit();
        return NoContent();
    }
    
}
