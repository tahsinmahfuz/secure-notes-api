namespace Infrastructure.Configuration;

public class MongoDbSettings
{
    public const string SectionName = "MongoDb";
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "secure_notes";
}

public class JwtSettings
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SecureNotesApi";
    public string Audience { get; set; } = "SecureNotesClient";
    public int ExpirationMinutes { get; set; } = 120;
}

public class SeedAdminSettings
{
    public const string SectionName = "SeedAdmin";
    public string Name { get; set; } = "System Admin";
    public string Email { get; set; } = "admin@securenotes.local";
    public string Password { get; set; } = "Admin@12345";
}

