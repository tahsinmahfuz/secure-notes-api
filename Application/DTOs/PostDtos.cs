using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record PostDto(
    string Id,
    string UserId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreatePostRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content);

public record UpdatePostRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content);

