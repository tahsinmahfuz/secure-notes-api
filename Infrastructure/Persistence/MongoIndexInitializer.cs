using Domain.Entities;
using MongoDB.Driver;

namespace Infrastructure.Persistence;

public class MongoIndexInitializer
{
    private readonly MongoDbContext _context;

    public MongoIndexInitializer(MongoDbContext context)
    {
        _context = context;
    }

    public async Task EnsureIndexesAsync(CancellationToken ct = default)
    {
        var userIndexes = new[]
        {
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true, Name = "ix_users_email_unique" }),
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Descending(u => u.CreatedAt),
                new CreateIndexOptions { Name = "ix_users_createdAt_desc" }),
            new CreateIndexModel<User>(
                Builders<User>.IndexKeys.Ascending(u => u.Interests),
                new CreateIndexOptions { Name = "ix_users_interests" })
        };

        var noteIndexes = new[]
        {
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Ascending(n => n.UserId).Descending(n => n.CreatedAt),
                new CreateIndexOptions { Name = "ix_notes_userId_createdAt_desc" }),
            new CreateIndexModel<Note>(
                Builders<Note>.IndexKeys.Descending(n => n.CreatedAt),
                new CreateIndexOptions { Name = "ix_notes_createdAt_desc" })
        };

        var postIndexes = new[]
        {
            new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys.Ascending(p => p.UserId).Descending(p => p.CreatedAt),
                new CreateIndexOptions { Name = "ix_posts_userId_createdAt_desc" }),
            new CreateIndexModel<Post>(
                Builders<Post>.IndexKeys.Descending(p => p.CreatedAt),
                new CreateIndexOptions { Name = "ix_posts_createdAt_desc" })
        };

        await _context.Users.Indexes.CreateManyAsync(userIndexes, ct);
        await _context.Notes.Indexes.CreateManyAsync(noteIndexes, ct);
        await _context.Posts.Indexes.CreateManyAsync(postIndexes, ct);
    }
}

