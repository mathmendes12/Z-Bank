using System.Threading.Tasks;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Users;
using Microsoft.EntityFrameworkCore;
using System;

namespace ZBank.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task<User?> GetByCpfAsync(string cpf)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Cpf == cpf);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }
}

