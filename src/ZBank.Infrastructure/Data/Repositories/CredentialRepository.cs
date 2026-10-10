using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;

namespace ZBank.Infrastructure.Data.Repositories;

public class CredentialRepository : ICredentialRepository
{
    private readonly AppDbContext _context;

    public CredentialRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Credential credential)
    {
        await _context.Credentials.AddAsync(credential);
    }

    public async Task<Credential?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Credentials.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}

