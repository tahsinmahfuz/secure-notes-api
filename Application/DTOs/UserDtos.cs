using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record UserDto(
    string Id,
    string Name,
    string Email,
    string Role,
    List<string> Interests,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MinLength(6), MaxLength(100)] string Password,
    string? Role,
    List<string>? Interests);

public record UpdateUserRequest(
    [Required, MaxLength(100)] string Name,
    [Required, EmailAddress, MaxLength(200)] string Email,
    [MinLength(6), MaxLength(100)] string? Password,
    string? Role,
    List<string>? Interests);

public record UpdateProfileRequest(
    [Required, MaxLength(100)] string Name,
    List<string>? Interests);

public record InterestGroupDto(
    string Interest,
    int UserCount,
    List<InterestUserDto> Users);

public record InterestUserDto(
    string Id,
    string Name,
    string Email,
    string Role);

public record UserPostsDto(
    string Id,
    string Name,
    string Email,
    string Role,
    List<string> Interests,
    DateTime CreatedAt,
    int Page,
    int Limit,
    bool HasMorePosts,
    List<PostDto> Posts);

