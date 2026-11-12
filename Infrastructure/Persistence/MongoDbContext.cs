using Domain.Entities;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Persistence;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<User> Users => Database.GetCollection<User>("users");
    public IMongoCollection<Note> Notes => Database.GetCollection<Note>("notes");
    public IMongoCollection<Post> Posts => Database.GetCollection<Post>("posts");

    public MongoDbContext(IOptions<MongoDbSettings> options)
    {
        MongoClassMappings.Register();

        var settings = options.Value;
        var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
        clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);

        var client = new MongoClient(clientSettings);
        Database = client.GetDatabase(settings.DatabaseName);
    }
}

