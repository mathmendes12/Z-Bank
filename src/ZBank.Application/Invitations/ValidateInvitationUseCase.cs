using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;

namespace ZBank.Application.Invitations;

public class ValidateInvitationUseCase
{
    private readonly IInvitationRepository _invitationRepository;

    public ValidateInvitationUseCase(IInvitationRepository invitationRepository)
    {
        _invitationRepository = invitationRepository;
    }

    public async Task<bool> ExecuteAsync(string code)
    {
        var invitation = await _invitationRepository.GetByCodeAsync(code);
        
        if (invitation == null)
            return false;

        return invitation.Status == InvitationStatus.Pending;
    }
}
