using Domain.Entities;

namespace Domain.Interfaces;

public interface IPostRepository
{
    Task<Post?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<Post> Items, long TotalCount)> GetPagedAsync(int page, int limit, CancellationToken ct = default);
    Task<Post> CreateAsync(Post post, CancellationToken ct = default);
    Task<bool> UpdateAsync(Post post, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task DeleteByUserIdAsync(string userId, CancellationToken ct = default);
}

