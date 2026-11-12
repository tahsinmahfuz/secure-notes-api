using Domain.Enums;

namespace Domain.Models;

public class InterestGroup
{
    public string Interest { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public List<InterestUser> Users { get; set; } = new();
}

public class InterestUser
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
}

