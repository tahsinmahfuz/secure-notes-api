using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly IMongoCollection<Post> _posts;

    public PostRepository(MongoDbContext context)
    {
        _posts = context.Posts;
    }

    public async Task<Post?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await _posts.Find(p => p.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<Post> Items, long TotalCount)> GetPagedAsync(int page, int limit, CancellationToken ct = default)
    {
        var skip = (page - 1) * limit;
        var filter = Builders<Post>.Filter.Empty;

        var totalTask = _posts.CountDocumentsAsync(filter, cancellationToken: ct);
        var itemsTask = _posts.Find(filter)
            .SortByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<Post> CreateAsync(Post post, CancellationToken ct = default)
    {
        await _posts.InsertOneAsync(post, cancellationToken: ct);
        return post;
    }

    public async Task<bool> UpdateAsync(Post post, CancellationToken ct = default)
    {
        var result = await _posts.ReplaceOneAsync(p => p.Id == post.Id, post, cancellationToken: ct);
        return result.MatchedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await _posts.DeleteOneAsync(p => p.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task DeleteByUserIdAsync(string userId, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(userId, out _))
        {
            return;
        }

        await _posts.DeleteManyAsync(p => p.UserId == userId, ct);
    }
}

