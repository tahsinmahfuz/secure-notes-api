using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Persistence;

public class DataSeeder
{
    private readonly MongoDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedAdminSettings _adminSettings;

    public DataSeeder(
        MongoDbContext context,
        IPasswordHasher passwordHasher,
        IOptions<SeedAdminSettings> adminSettings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _adminSettings = adminSettings.Value;
    }

    public async Task SeedAdminAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_adminSettings.Email) ||
            string.IsNullOrWhiteSpace(_adminSettings.Password))
        {
            return;
        }

        var email = _adminSettings.Email.Trim().ToLowerInvariant();
        var existing = await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync(ct);
        if (existing is not null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var admin = new User
        {
            Name = string.IsNullOrWhiteSpace(_adminSettings.Name) ? "System Admin" : _adminSettings.Name.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(_adminSettings.Password),
            Role = Role.Admin,
            Interests = new List<string> { "architecture", "security" },
            CreatedAt = now,
            UpdatedAt = now
        };

        await _context.Users.InsertOneAsync(admin, cancellationToken: ct);
    }
}

