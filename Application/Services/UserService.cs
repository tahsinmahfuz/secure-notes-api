using Application.Common;
using Application.DTOs;
using Application.Exceptions;
using Application.Mapping;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class UserService
{
    private readonly IUserRepository _users;
    private readonly INoteRepository _notes;
    private readonly IPostRepository _posts;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(
        IUserRepository users,
        INoteRepository notes,
        IPostRepository posts,
        IPasswordHasher passwordHasher)
    {
        _users = users;
        _notes = notes;
        _posts = posts;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        EnsureValidObjectId(id);

        var user = await _users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User not found.");

        return user.ToDto();
    }

    public async Task<PagedResult<UserDto>> GetPagedAsync(int page, int limit, CancellationToken ct = default)
    {
        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var (items, total) = await _users.GetPagedAsync(normalizedPage, normalizedLimit, ct);

        return new PagedResult<UserDto>
        {
            Items = items.Select(u => u.ToDto()).ToList(),
            Page = normalizedPage,
            Limit = normalizedLimit,
            TotalCount = total
        };
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        var role = ParseRole(request.Role);

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
            Role = role,
            Interests = AuthService.NormalizeInterests(request.Interests),
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _users.CreateAsync(user, ct);
        return created.ToDto();
    }

    public async Task<UserDto> UpdateAsync(string id, UpdateUserRequest request, CancellationToken ct = default)
    {
        EnsureValidObjectId(id);

        var user = await _users.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("User not found.");

        var name = request.Name.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var conflict = await _users.GetByEmailAsync(email, ct);
            if (conflict is not null && conflict.Id != id)
            {
                throw new ConflictException("Email is already in use.");
            }
        }

        user.Name = name;
        user.Email = email;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            user.Role = ParseRole(request.Role);
        }

        if (request.Interests is not null)
        {
            user.Interests = AuthService.NormalizeInterests(request.Interests);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _users.UpdateAsync(user, ct);
        return user.ToDto();
    }

    public async Task<UserDto> UpdateOwnProfileAsync(string userId, UpdateProfileRequest request, CancellationToken ct = default)
    {
        EnsureValidObjectId(userId);

        var user = await _users.GetByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Name is required.");
        }

        user.Name = name;
        if (request.Interests is not null)
        {
            user.Interests = AuthService.NormalizeInterests(request.Interests);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await _users.UpdateAsync(user, ct);
        return user.ToDto();
    }

    public async Task DeleteAsync(string id, string currentAdminId, CancellationToken ct = default)
    {
        EnsureValidObjectId(id);

        if (id == currentAdminId)
        {
            throw new BadRequestException("Cannot delete your own account.");
        }

        var deleted = await _users.DeleteAsync(id, ct);
        if (!deleted)
        {
            throw new NotFoundException("User not found.");
        }

        await _notes.DeleteByUserIdAsync(id, ct);
        await _posts.DeleteByUserIdAsync(id, ct);
    }

    public async Task<PagedResult<InterestGroupDto>> GetGroupedByInterestsAsync(int page, int limit, CancellationToken ct = default)
    {
        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var (items, total) = await _users.GetGroupedByInterestsAsync(normalizedPage, normalizedLimit, ct);

        return new PagedResult<InterestGroupDto>
        {
            Items = items.Select(g => g.ToDto()).ToList(),
            Page = normalizedPage,
            Limit = normalizedLimit,
            TotalCount = total
        };
    }

    public async Task<UserPostsDto> GetUserWithPostsAsync(string userId, int page, int limit, CancellationToken ct = default)
    {
        EnsureValidObjectId(userId);

        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var result = await _users.GetUserWithPostsAsync(userId, normalizedPage, normalizedLimit, ct)
            ?? throw new NotFoundException("User not found.");

        return result.ToDto(normalizedPage, normalizedLimit);
    }

    internal static void EnsureValidObjectId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length != 24 || !id.All(Uri.IsHexDigit))
        {
            throw new BadRequestException("Invalid identifier format.");
        }
    }

    private static Role ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Role.User;
        }

        if (!Enum.TryParse<Role>(role.Trim(), ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
        {
            throw new BadRequestException("Role must be User or Admin.");
        }

        return parsed;
    }
}

