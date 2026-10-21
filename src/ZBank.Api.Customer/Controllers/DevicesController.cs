using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Auth;

namespace ZBank.Api.Customer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    [Authorize]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice(
        [FromServices] RegisterDeviceUseCase useCase,
        [FromBody] RegisterDeviceClientRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { Message = "User identity not found in token." });
        }

        var response = await useCase.ExecuteAsync(new RegisterDeviceRequest
        {
            UserId = userId,
            PublicKey = request.PublicKey,
            DeviceName = request.DeviceName
        });

        return Ok(response);
    }
}

public record RegisterDeviceClientRequest(string PublicKey, string DeviceName);
