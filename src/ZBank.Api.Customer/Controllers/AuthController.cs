using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Auth;
using System;

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
        if (response.IsSuccess) return Ok(new { Message = "User registered successfully." });
        return BadRequest(new { Message = response.ErrorMessage });
    }

    [HttpPost("totp/setup")]
    public async Task<IActionResult> SetupTotp([FromServices] SetupTotpUseCase useCase, [FromBody] SetupTotpRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
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
    public async Task<IActionResult> Challenge([FromServices] BiometricChallengeUseCase useCase)
    {
        var response = await useCase.ExecuteAsync();
        return Ok(response);
    }

    [HttpPost("biometric/login")]
    public async Task<IActionResult> BiometricLogin([FromServices] BiometricLoginUseCase useCase, [FromBody] BiometricLoginRequest request)
    {
        var response = await useCase.ExecuteAsync(request);
        return Ok(response);
    }
}

