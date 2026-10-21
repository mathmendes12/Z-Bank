using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Infrastructure.Data.Repositories;

public class BiometricChallengeRepository : IBiometricChallengeRepository
{
    private readonly AppDbContext _context;

    public BiometricChallengeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BiometricChallenge challenge)
    {
        await _context.BiometricChallenges.AddAsync(challenge);
    }

    public async Task<BiometricChallenge?> GetValidChallengeAsync(Guid deviceId, string nonce)
    {
        return await _context.BiometricChallenges
            .Where(c => c.DeviceId == deviceId && c.Nonce == nonce && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(c => c.ExpiresAt)
            .FirstOrDefaultAsync();
    }
}
