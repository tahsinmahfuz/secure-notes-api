using Application.Exceptions;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _users;

    public UserRepository(MongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return null;
        }

        return await _users.Find(u => u.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _users.Find(u => u.Email == email).FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<User> Items, long TotalCount)> GetPagedAsync(int page, int limit, CancellationToken ct = default)
    {
        var skip = (page - 1) * limit;
        var filter = Builders<User>.Filter.Empty;

        var totalTask = _users.CountDocumentsAsync(filter, cancellationToken: ct);
        var itemsTask = _users.Find(filter)
            .SortByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Limit(limit)
            .ToListAsync(ct);

        await Task.WhenAll(totalTask, itemsTask);
        return (itemsTask.Result, totalTask.Result);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        try
        {
            await _users.InsertOneAsync(user, cancellationToken: ct);
            return user;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ConflictException("Email is already registered.");
        }
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        try
        {
            var result = await _users.ReplaceOneAsync(u => u.Id == user.Id, user, cancellationToken: ct);
            return result.MatchedCount > 0;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            throw new ConflictException("Email is already in use.");
        }
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(id, out _))
        {
            return false;
        }

        var result = await _users.DeleteOneAsync(u => u.Id == id, ct);
        return result.DeletedCount > 0;
    }

    public async Task<(IReadOnlyList<InterestGroup> Items, long TotalCount)> GetGroupedByInterestsAsync(int page, int limit, CancellationToken ct = default)
    {
        var skip = (page - 1) * limit;

        var pipeline = PipelineDefinition<User, InterestGroupFacetResult>.Create(new[]
        {
            new BsonDocument("$match", new BsonDocument("interests", new BsonDocument
            {
                { "$exists", true },
                { "$ne", new BsonArray() }
            })),
            new BsonDocument("$unwind", "$interests"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$interests" },
                { "userCount", new BsonDocument("$sum", 1) },
                { "users", new BsonDocument("$push", new BsonDocument
                    {
                        { "id", new BsonDocument("$toString", "$_id") },
                        { "name", "$name" },
                        { "email", "$email" },
                        { "role", "$role" }
                    })
                }
            }),
            new BsonDocument("$sort", new BsonDocument
            {
                { "userCount", -1 },
                { "_id", 1 }
            }),
            new BsonDocument("$facet", new BsonDocument
            {
                { "metadata", new BsonArray
                    {
                        new BsonDocument("$count", "total")
                    }
                },
                { "items", new BsonArray
                    {
                        new BsonDocument("$skip", skip),
                        new BsonDocument("$limit", limit),
                        new BsonDocument("$project", new BsonDocument
                        {
                            { "_id", 0 },
                            { "interest", "$_id" },
                            { "userCount", 1 },
                            { "users", 1 }
                        })
                    }
                }
            })
        });

        var facet = await _users.Aggregate(pipeline).FirstOrDefaultAsync(ct);
        if (facet is null)
        {
            return (Array.Empty<InterestGroup>(), 0);
        }

        var totalCount = facet.Metadata.FirstOrDefault()?.Total ?? 0;
        return (facet.Items, totalCount);
    }

    public async Task<UserWithPosts?> GetUserWithPostsAsync(string userId, int page, int limit, CancellationToken ct = default)
    {
        if (!ObjectId.TryParse(userId, out var objectId))
        {
            return null;
        }

        var skip = (page - 1) * limit;

        var pipeline = PipelineDefinition<User, UserWithPosts>.Create(new[]
        {
            new BsonDocument("$match", new BsonDocument("_id", objectId)),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "posts" },
                { "localField", "_id" },
                { "foreignField", "userId" },
                { "pipeline", new BsonArray
                    {
                        new BsonDocument("$sort", new BsonDocument("createdAt", -1)),
                        new BsonDocument("$skip", skip),
                        new BsonDocument("$limit", limit + 1)
                    }
                },
                { "as", "posts" }
            }),
            new BsonDocument("$project", new BsonDocument("passwordHash", 0))
        });

        var result = await _users.Aggregate(pipeline).FirstOrDefaultAsync(ct);
        if (result is null)
        {
            return null;
        }

        if (result.Posts.Count > limit)
        {
            result.HasMorePosts = true;
            result.Posts = result.Posts.Take(limit).ToList();
        }

        return result;
    }

    private sealed class InterestGroupFacetResult
    {
        public List<FacetMetadata> Metadata { get; set; } = new();
        public List<InterestGroup> Items { get; set; } = new();
    }

    private sealed class FacetMetadata
    {
        public long Total { get; set; }
    }
}
