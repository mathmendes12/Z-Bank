using Microsoft.EntityFrameworkCore;
using ZBank.Domain.Invitations;
using ZBank.Domain.Users;
using ZBank.Domain.Auth;
using ZBank.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace ZBank.Infrastructure.Data;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Invitation> Invitations { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Credential> Credentials { get; set; } = null!;
    public DbSet<Device> Devices { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;
    public DbSet<BiometricChallenge> BiometricChallenges { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.HasIndex(e => e.Cpf).IsUnique();
        });

        modelBuilder.Entity<Credential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique(); // 1:1
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.TotpSecret).HasMaxLength(100);
            entity.Property(e => e.TotpEnabled).IsRequired();
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PublicKey).IsRequired();
            entity.Property(e => e.DeviceName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RefreshTokenHash).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.DeviceId);
        });

        modelBuilder.Entity<BiometricChallenge>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nonce).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.DeviceId, e.Nonce });
        });
    }

    public async Task CommitAsync()
    {
        await SaveChangesAsync();
    }
}
