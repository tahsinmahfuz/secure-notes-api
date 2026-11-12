using Domain.Entities;
using Domain.Enums;
using Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace Infrastructure.Persistence;

internal static class MongoClassMappings
{
    private static readonly object SyncLock = new();
    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        lock (SyncLock)
        {
            if (_registered)
            {
                return;
            }

            var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new EnumRepresentationConvention(BsonType.String),
                new IgnoreExtraElementsConvention(true)
            };
            ConventionRegistry.Register("AppConventions", pack, _ => true);

            if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            {
                BsonClassMap.RegisterClassMap<User>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(u => u.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(u => u.Role)
                        .SetSerializer(new EnumSerializer<Role>(BsonType.String));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Note)))
            {
                BsonClassMap.RegisterClassMap<Note>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(n => n.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(n => n.UserId)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(Post)))
            {
                BsonClassMap.RegisterClassMap<Post>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(p => p.Id)
                        .SetIdGenerator(StringObjectIdGenerator.Instance)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(p => p.UserId)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(InterestUser)))
            {
                BsonClassMap.RegisterClassMap<InterestUser>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(null);
                    cm.MapMember(u => u.Id).SetElementName("id");
                    cm.MapMember(u => u.Role)
                        .SetSerializer(new EnumSerializer<Role>(BsonType.String));
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(InterestGroup)))
            {
                BsonClassMap.RegisterClassMap<InterestGroup>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIgnoreExtraElements(true);
                });
            }

            if (!BsonClassMap.IsClassMapRegistered(typeof(UserWithPosts)))
            {
                BsonClassMap.RegisterClassMap<UserWithPosts>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdMember(u => u.Id)
                        .SetSerializer(new StringSerializer(BsonType.ObjectId));
                    cm.MapMember(u => u.Role)
                        .SetSerializer(new EnumSerializer<Role>(BsonType.String));
                    cm.UnmapMember(u => u.HasMorePosts);
                });
            }

            _registered = true;
        }
    }
}

