using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Auth;

namespace ZBank.Api.Customer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUserUseCase;

    public AuthController(RegisterUserUseCase registerUserUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _registerUserUseCase.ExecuteAsync(request);
        if (response.IsSuccess)
        {
            return Ok(new
            {
                userId = response.UserId,
                setupToken = response.SetupToken,
                message = "User registered successfully."
            });
        }
        return BadRequest(new { message = response.ErrorMessage });
    }

    [Authorize]
    [HttpPost("totp/setup")]
    public async Task<IActionResult> SetupTotp([FromServices] SetupTotpUseCase useCase)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User identity not found in token." });

        var response = await useCase.ExecuteAsync(userId);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("totp/confirm")]
    public async Task<IActionResult> ConfirmTotp([FromServices] ConfirmTotpUseCase useCase, [FromBody] ConfirmTotpRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "User identity not found in token." });

        var response = await useCase.ExecuteAsync(userId, request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromServices] LoginUseCase useCase, [FromBody] LoginRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromServices] RefreshTokensUseCase useCase, [FromBody] RefreshTokensRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("biometric/challenge")]
    public async Task<IActionResult> Challenge([FromServices] BiometricChallengeUseCase useCase, [FromBody] BiometricChallengeRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }

    [HttpPost("biometric/login")]
    public async Task<IActionResult> BiometricLogin([FromServices] BiometricLoginUseCase useCase, [FromBody] BiometricLoginRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }
}
