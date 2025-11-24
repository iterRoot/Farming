using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FarmingApi.Core;
[ApiController]
[Route("[controller]")]
public class MyController : ControllerBase
{
    // [NonAction]
    // protected Auth? GetClaim()
    // {
    //     if (User.Identity is ClaimsIdentity claimsIdentity && claimsIdentity.Claims.Any())
    //         return new Auth(claimsIdentity.Claims);

    //     return null;
    // }

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
        return Error("Item is not found!");
    }

    [NonAction]
    protected IActionResult Error(string message)
    {
        return Ok(new { Status = "E", Message = message });
    }
}