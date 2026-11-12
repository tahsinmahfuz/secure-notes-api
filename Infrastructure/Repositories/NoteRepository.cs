using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class NoteRepository : INoteRepository
{
    private readonly IMongoCollection<Note> _notes;

    public NoteRepository(MongoDbContext context)
    {
        _notes = context.Notes;
    }

    public async Task<Note?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await _notes.Find(n => n.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedByUserAsync(string userId, int page, int limit, CancellationToken ct = default)
    {
        var skip = (page - 1) * limit;
        var filter = Builders<Note>.Filter.Eq(n => n.UserId, userId);

        var totalTask = _notes.CountDocumentsAsync(filter, cancellationToken: ct);
        var itemsTask = _notes.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<(IReadOnlyList<Note> Items, long TotalCount)> GetPagedAllAsync(string? userId, int page, int limit, CancellationToken ct = default)
    {
        var skip = (page - 1) * limit;
        var filter = string.IsNullOrWhiteSpace(userId)
            ? Builders<Note>.Filter.Empty
            : Builders<Note>.Filter.Eq(n => n.UserId, userId);

        var totalTask = _notes.CountDocumentsAsync(filter, cancellationToken: ct);
        var itemsTask = _notes.Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Note> CreateAsync(Note note, CancellationToken ct = default)
    {
        await _notes.InsertOneAsync(note, cancellationToken: ct);
        return note;
    }

    public async Task<bool> UpdateAsync(Note note, CancellationToken ct = default)
    {
        var result = await _notes.ReplaceOneAsync(n => n.Id == note.Id, note, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await _notes.DeleteOneAsync(n => n.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task DeleteByUserIdAsync(string userId, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(userId, out _))
        {
            return;
        }

        await _notes.DeleteManyAsync(n => n.UserId == userId, ct);
    }
}

