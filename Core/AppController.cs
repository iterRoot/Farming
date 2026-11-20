using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Core;
[ApiController]
[Route("[controller]")]
public class MyController : ControllerBase
{
    [NonAction]
    protected IActionResult Success(dynamic? data = null, PaginationResponse? pagination = null)
    {
        if (pagination != null) return Ok(new { Status = "S", data, pagination });
        return data != null ? Ok(new { Status = "S", data }) : Ok(new { Status = "S" });
    }

    [NonAction]
    protected IActionResult Existed(string name)
    {
        return Error($"{name} is already exited!");
    }

    [NonAction]
    protected IActionResult Denied()
    {
        return Error("Access Denied!");
    }

    [NonAction]
    protected IActionResult Required(string name)
    {
        return Error($"{name} is required!");
    }

    [NonAction]
    protected IActionResult ItemNotFound()
    {
        // A missing record is a 404 (same body as Error(), so callers reading Message still work);
        // answering 200 made delete/get of a missing id look like a success.
        return NotFound(new { Status = "E", Message = "Item is not found!" });
    }

    [NonAction]
    protected IActionResult Error(string message)
    {
        return Ok(new { Status = "E", Message = message });
    }
}