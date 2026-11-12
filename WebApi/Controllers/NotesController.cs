using Application.Common;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/notes")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly NoteService _noteService;

    public NotesController(NoteService noteService)
    {
        _noteService = noteService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await _noteService.GetMyNotesAsync(User.GetUserId(), page, limit, ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<NoteDto>>> GetAll(
        [FromQuery] string? userId = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await _noteService.GetAllNotesAsync(userId, page, limit, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NoteDto>> GetById(string id, CancellationToken ct)
    {
        var note = await _noteService.GetByIdAsync(id, User.GetUserId(), User.IsAdmin(), ct);
        return Ok(note);
    }

    [HttpPost]
    public async Task<ActionResult<NoteDto>> Create([FromBody] CreateNoteRequest request, CancellationToken ct)
    {
        var created = await _noteService.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<NoteDto>> Update(string id, [FromBody] UpdateNoteRequest request, CancellationToken ct)
    {
        var updated = await _noteService.UpdateAsync(id, User.GetUserId(), request, ct);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _noteService.DeleteAsync(id, User.GetUserId(), ct);
        return NoContent();
    }
}

