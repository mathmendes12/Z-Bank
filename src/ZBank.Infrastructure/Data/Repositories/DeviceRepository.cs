using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZBank.Application.Common.Interfaces;
using ZBank.Domain.Auth;

namespace ZBank.Infrastructure.Data.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _context;

    public DeviceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Device device)
    {
        await _context.Devices.AddAsync(device);
    }

    public async Task<Device?> GetByIdAsync(Guid id)
    {
        return await _context.Devices.FindAsync(id);
    }

    public async Task<Device?> GetByUserIdAndIdAsync(Guid userId, Guid deviceId)
    {
        return await _context.Devices.FirstOrDefaultAsync(d => d.UserId == userId && d.Id == deviceId);
    }
}
