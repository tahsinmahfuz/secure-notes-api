using Domain.Entities;

namespace Domain.Interfaces;

public interface INoteRepository
{
    Task<Note?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedByUserAsync(string userId, int page, int limit, CancellationToken ct = default);
    Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedAllAsync(string? userId, int page, int limit, CancellationToken ct = default);
    Task<Note> CreateAsync(Note note, CancellationToken ct = default);
    Task<bool> UpdateAsync(Note note, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task DeleteByUserIdAsync(string userId, CancellationToken ct = default);
}

