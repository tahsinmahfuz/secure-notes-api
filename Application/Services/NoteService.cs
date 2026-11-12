using Application.Common;
using Application.DTOs;
using Application.Exceptions;
using Application.Mapping;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class NoteService
{
    private readonly INoteRepository _notes;

    public NoteService(INoteRepository notes)
    {
        _notes = notes;
    }

    public async Task<PagedResult<NoteDto>> GetMyNotesAsync(string currentUserId, int page, int limit, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(currentUserId);

        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var (items, total) = await _notes.GetPagedByUserAsync(currentUserId, normalizedPage, normalizedLimit, ct);

        return new PagedResult<NoteDto>
        {
            Items = items.Select(n => n.ToDto()).ToList(),
            Page = normalizedPage,
            Limit = normalizedLimit,
            TotalCount = total
        };
    }

    public async Task<PagedResult<NoteDto>> GetAllNotesAsync(string? userId, int page, int limit, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            UserService.EnsureValidObjectId(userId);
        }

        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var (items, total) = await _notes.GetPagedAllAsync(string.IsNullOrWhiteSpace(userId) ? null : userId, normalizedPage, normalizedLimit, ct);

        return new PagedResult<NoteDto>
        {
            Items = items.Select(n => n.ToDto()).ToList(),
            Page = normalizedPage,
            Limit = normalizedLimit,
            TotalCount = total
        };
    }

    public async Task<NoteDto> GetByIdAsync(string id, string currentUserId, bool isAdmin, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var note = await _notes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Note not found.");

        if (note.UserId != currentUserId && !isAdmin)
        {
            throw new ForbiddenException("You do not have access to this note.");
        }

        return note.ToDto();
    }

    public async Task<NoteDto> CreateAsync(string currentUserId, CreateNoteRequest request, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(currentUserId);

        var title = request.Title.Trim();
        var content = request.Content.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            throw new BadRequestException("Title and content are required.");
        }

        var now = DateTime.UtcNow;
        var note = new Note
        {
            UserId = currentUserId,
            Title = title,
            Content = content,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _notes.CreateAsync(note, ct);
        return created.ToDto();
    }

    public async Task<NoteDto> UpdateAsync(string id, string currentUserId, UpdateNoteRequest request, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var note = await _notes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Note not found.");

        if (note.UserId != currentUserId)
        {
            throw new ForbiddenException("You can only edit your own notes.");
        }

        var title = request.Title.Trim();
        var content = request.Content.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            throw new BadRequestException("Title and content are required.");
        }

        note.Title = title;
        note.Content = content;
        note.UpdatedAt = DateTime.UtcNow;

        await _notes.UpdateAsync(note, ct);
        return note.ToDto();
    }

    public async Task DeleteAsync(string id, string currentUserId, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var note = await _notes.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Note not found.");

        if (note.UserId != currentUserId)
        {
            throw new ForbiddenException("You can only delete your own notes.");
        }

        await _notes.DeleteAsync(id, ct);
    }
}

