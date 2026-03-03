using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Huit.API.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? User.FindFirst("sub");
        if (userIdClaim == null)
            throw new UnauthorizedAccessException("User is not authenticated");

        return int.Parse(userIdClaim.Value);
    }

    protected string? GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
        return roleClaim?.Value;
    }

    protected bool IsAdmin()
    {
        var role = GetCurrentUserRole();
        return role == "ADMIN";
    }

    protected bool IsStaff()
    {
        var role = GetCurrentUserRole();
        return role == "ADMIN" || role == "STAFF";
    }
}
