using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZBank.Application.Invitations;

namespace ZBank.Api.Customer.Controllers;

[ApiController]
[Route("api/v1/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly ValidateInvitationUseCase _validateInvitationUseCase;

    public InvitationsController(ValidateInvitationUseCase validateInvitationUseCase)
    {
        _validateInvitationUseCase = validateInvitationUseCase;
    }

    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] ValidateInvitationRequest request)
    {
        var isValid = await _validateInvitationUseCase.ExecuteAsync(request.Code);

        if (isValid)
            return Ok(new { IsValid = true });

        return BadRequest(new { IsValid = false, Message = "Invalid or already used invitation." });
    }
}

public record ValidateInvitationRequest(string Code);
