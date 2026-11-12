using Domain.Interfaces;
using Infrastructure.Configuration;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(options =>
        {
            configuration.GetSection(MongoDbSettings.SectionName).Bind(options);
            var envUri = Environment.GetEnvironmentVariable("MONGODB_URI");
            if (!string.IsNullOrWhiteSpace(envUri))
            {
                options.ConnectionString = envUri;
            }
            var envDb = Environment.GetEnvironmentVariable("MONGODB_DATABASE");
            if (!string.IsNullOrWhiteSpace(envDb))
            {
                options.DatabaseName = envDb;
            }
        });

        services.Configure<JwtSettings>(options =>
        {
            configuration.GetSection(JwtSettings.SectionName).Bind(options);
            var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
            if (!string.IsNullOrWhiteSpace(envSecret))
            {
                options.Secret = envSecret;
            }
        });

        services.Configure<SeedAdminSettings>(configuration.GetSection(SeedAdminSettings.SectionName));

        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<MongoIndexInitializer>();
        services.AddScoped<DataSeeder>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<IPostRepository, PostRepository>();

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtProvider, JwtProvider>();

        return services;
    }
}

