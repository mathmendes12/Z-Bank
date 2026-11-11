using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Accounts;

namespace ZBank.Api.Customer.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class AccountsController : ControllerBase
{
    private Guid GetUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(idStr, out var id))
            return id;
        return Guid.Empty;
    }

    [HttpGet("me/balance")]
    public async Task<IActionResult> GetBalance([FromServices] IGetBalanceUseCase useCase)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var response = await useCase.ExecuteAsync(userId);
        if (response == null) return NotFound(new { Message = "Account not found." });

        return Ok(response);
    }

    [HttpGet("me/statement")]
    public async Task<IActionResult> GetStatement(
        [FromServices] IGetStatementUseCase useCase,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var response = await useCase.ExecuteAsync(userId, page, limit);
        if (response == null) return NotFound(new { Message = "Account not found." });

        return Ok(response);
    }
}
