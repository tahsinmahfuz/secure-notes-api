using Domain.Entities;
using Domain.Enums;

namespace Domain.Models;

public class UserWithPosts
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public List<string> Interests { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public List<Post> Posts { get; set; } = new();
    public bool HasMorePosts { get; set; }
}

