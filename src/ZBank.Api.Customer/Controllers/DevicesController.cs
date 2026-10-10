using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Auth;
using System;

namespace ZBank.Api.Customer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DevicesController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromServices] RegisterDeviceUseCase useCase, [FromBody] RegisterDeviceRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }
}
