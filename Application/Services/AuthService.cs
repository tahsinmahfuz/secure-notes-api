using Application.DTOs;
using Application.Exceptions;
using Application.Mapping;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtProvider _jwtProvider;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtProvider jwtProvider)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        var existing = await _users.GetByEmailAsync(email, ct);
        if (existing is not null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = Role.User,
            Interests = NormalizeInterests(request.Interests),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _users.CreateAsync(user, ct);
        var token = _jwtProvider.GenerateToken(created);

        return new AuthResponse(token, created.ToDto());
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var token = _jwtProvider.GenerateToken(user);
        return new AuthResponse(token, user.ToDto());
    }

    internal static List<string> NormalizeInterests(List<string>? interests)
    {
        if (interests is null || interests.Count == 0)
        {
            return new List<string>();
        }

        return interests
            .Select(i => i.Trim().ToLowerInvariant())
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Distinct()
            .ToList();
    }
}

