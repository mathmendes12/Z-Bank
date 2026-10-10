using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Invitations;

namespace ZBank.Infrastructure.Data.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly AppDbContext _context;

    public InvitationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invitation?> GetByCodeAsync(string code)
    {
        return await _context.Invitations.FirstOrDefaultAsync(i => i.Code == code);
    }

    public Task UpdateAsync(Invitation invitation)
    {
        _context.Invitations.Update(invitation);
        return Task.CompletedTask;
    }
}
