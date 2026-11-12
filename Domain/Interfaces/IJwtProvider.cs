using Domain.Entities;

namespace Domain.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}

