using Application.Common;
using Application.DTOs;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Extensions;

namespace WebApi.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController : ControllerBase
{
    private readonly PostService _postService;

    public PostsController(PostService postService)
    {
        _postService = postService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<PostDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await _postService.GetPagedAsync(page, limit, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<PostDto>> GetById(string id, CancellationToken ct)
    {
        var post = await _postService.GetByIdAsync(id, ct);
        return Ok(post);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostDto>> Create([FromBody] CreatePostRequest request, CancellationToken ct)
    {
        var created = await _postService.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<PostDto>> Update(string id, [FromBody] UpdatePostRequest request, CancellationToken ct)
    {
        var updated = await _postService.UpdateAsync(id, User.GetUserId(), request, ct);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        await _postService.DeleteAsync(id, User.GetUserId(), ct);
        return NoContent();
    }
}

