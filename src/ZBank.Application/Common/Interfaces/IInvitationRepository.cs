using System.Threading.Tasks;
using ZBank.Domain.Invitations;

namespace ZBank.Application.Common.Interfaces;

public interface IInvitationRepository
{
    Task<Invitation?> GetByCodeAsync(string code);
    Task UpdateAsync(Invitation invitation);
}
