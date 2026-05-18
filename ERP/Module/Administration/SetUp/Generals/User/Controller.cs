using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FarmingApi.Core;
using System.Security.Cryptography;
using System.Text;

namespace FarmingApi.Modules.Administration.SetUp.User;

public class UserController : MyController
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;

    public UserController(IUserRepository repository, IMapper mapper)
    {
        _mapper = mapper;
        _repository = repository;
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }

    private bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(c => char.IsUpper(c));
        bool hasLower = password.Any(c => char.IsLower(c));
        bool hasDigit = password.Any(c => char.IsDigit(c));
        bool hasSpecial = password.Any(c => "@$!%*?&".Contains(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Gets()
    {
        var iQueryable = _repository.GetAll();
        var results = _mapper.ProjectTo<UserListResponse>(iQueryable).ToList();
        return Ok(results);
    }

    [HttpGet("{id:int}")]
    public IActionResult Get(int id)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
        {
            return BadRequest($"Item not found {id}");
        }
        var result = _mapper.Map<UserListResponse>(user);
        return Ok(result);
    }

    [HttpPost]
    public IActionResult Create([FromBody] UserListRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.UserId))
            return BadRequest("User ID is required");
        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("User Name is required");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            return BadRequest("Valid email is required");
        if (string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Password is required");

        if (!ValidatePasswordStrength(request.Password))
            return BadRequest("Password must be at least 8 characters with uppercase, lowercase, number, and special character (@$!%*?&)");

        var existingUser = _repository.GetSingle(u => u.UserId == request.UserId);
        if (existingUser != null)
            return BadRequest("User ID already exists");

        var existingEmail = _repository.GetSingle(u => u.Email == request.Email);
        if (existingEmail != null)
            return BadRequest("Email already exists");

        var entity = _mapper.Map<User>(request);
        entity.PasswordHash = HashPassword(request.Password);
        entity.CreatedAt = DateTime.UtcNow;
        entity.InActive = false;

        _repository.Add(entity);
        _repository.Commit();

        return Ok(new
        {
            message = "User saved successfully",
            id = entity.Id
        });
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromForm] UserUpdateRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
            return NotFound($"Item not found: {id}");

        if (string.IsNullOrWhiteSpace(request.UserName))
            return BadRequest("User Name is required");
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains("@"))
            return BadRequest("Valid email is required");

        if (user.Email != request.Email)
        {
            var existingEmail = _repository.GetSingle(u => u.Email == request.Email && u.Id != id);
            if (existingEmail != null)
                return BadRequest("Email already exists");
        }

        _mapper.Map(request, user);

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (!ValidatePasswordStrength(request.Password))
                return BadRequest("Password does not meet complexity requirements");
            user.PasswordHash = HashPassword(request.Password);
        }

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
        _repository.Remove(user);
        _repository.Commit();
        return NoContent();
    }

    [HttpPost("{id}/ChangePassword")]
    public IActionResult ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        var user = _repository.GetSingle(e => e.Id == id);
        if (user == null)
            return NotFound($"User not found: {id}");

        if (!VerifyPassword(request.OldPassword, user.PasswordHash))
            return BadRequest("Current password is incorrect");

        if (request.NewPassword != request.ConfirmPassword)
            return BadRequest("New passwords do not match");

        if (!ValidatePasswordStrength(request.NewPassword))
            return BadRequest("New password does not meet complexity requirements");

        user.PasswordHash = HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        _repository.Update(user);
        _repository.Commit();

        return NoContent();
    }
}