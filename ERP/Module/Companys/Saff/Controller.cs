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

    [HttpGet]
    public IActionResult Gets()
    {
        var result = _mapper.ProjectTo<StaffListResponse>(_repository.GetAll().OrderBy(s => s.StaffName)).ToList();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if (staff == null) return NotFound($"Staff {id} not found");
        return Ok(_mapper.Map<StaffListResponse>(staff));
    }

    [HttpPost]
    public IActionResult Insert([FromBody] StaffInsertRequest request)
    {
        var error = Validate(request.StaffName, request.StaffCode, null);
        if (error != null) return BadRequest(error);

        var staff = _mapper.Map<Staff>(request);
        staff.StaffName = staff.StaffName.Trim();
        staff.CreatedAt = DateTime.UtcNow;
        staff.InActive = request.InActive ?? false;
        _repository.Add(staff);
        _repository.Commit();
        return Ok(_mapper.Map<StaffListResponse>(staff));
    }

    [HttpPut("{id:int}")]
    public IActionResult Update(int id, [FromBody] StaffUpdateRequest request)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if (staff == null) return NotFound($"Staff {id} not found");

        var error = Validate(request.StaffName, request.StaffCode, id);
        if (error != null) return BadRequest(error);

        _mapper.Map(request, staff);
        staff.StaffName = staff.StaffName.Trim();
        staff.InActive = request.InActive ?? false;
        staff.UpdatedAt = DateTime.UtcNow;
        _repository.Update(staff);
        _repository.Commit();
        return Ok(_mapper.Map<StaffListResponse>(staff));
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var staff = _repository.GetSingle(e => e.Id == id);
        if (staff == null) return NotFound($"Staff {id} not found");
        staff.DeletedAt = DateTime.UtcNow;
        _repository.Remove(staff);
        _repository.Commit();
        return NoContent();
    }

    private string? Validate(string? name, string? code, int? ignoreId)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Name is required.";
        if (!string.IsNullOrWhiteSpace(code))
        {
            var c = code.Trim().ToLower();
            var clash = _repository.GetAll().Any(x => x.StaffCode != null && x.StaffCode.ToLower() == c && x.Id != ignoreId);
            if (clash) return $"Staff code \"{code}\" already exists.";
        }
        return null;
    }
}
