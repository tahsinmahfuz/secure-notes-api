using Domain.Entities;
using Domain.Models;

namespace Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, long TotalCount)> GetPagedAsync(int page, int limit, CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<bool> UpdateAsync(User user, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<InterestGroup> Items, long TotalCount)> GetGroupedByInterestsAsync(int page, int limit, CancellationToken ct = default);
    Task<UserWithPosts?> GetUserWithPostsAsync(string userId, int page, int limit, CancellationToken ct = default);
}

