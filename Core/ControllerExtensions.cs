using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace KamnttraApi.Core;

public static class ControllerExtensions
{
    public static int? CurrentUserId(this ControllerBase controller)
    {
        var claim = controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
