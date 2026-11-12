using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public record NoteDto(
    string Id,
    string UserId,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record CreateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content);

public record UpdateNoteRequest(
    [Required, MaxLength(200)] string Title,
    [Required, MaxLength(10000)] string Content);

