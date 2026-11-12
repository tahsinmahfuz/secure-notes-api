using Application.Common;
using Application.DTOs;
using Application.Exceptions;
using Application.Mapping;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class PostService
{
    private readonly IPostRepository _posts;

    public PostService(IPostRepository posts)
    {
        _posts = posts;
    }

    public async Task<PagedResult<PostDto>> GetPagedAsync(int page, int limit, CancellationToken ct = default)
    {
        var (normalizedPage, normalizedLimit) = PaginationDefaults.Normalize(page, limit);
        var (items, total) = await _posts.GetPagedAsync(normalizedPage, normalizedLimit, ct);

        return new PagedResult<PostDto>
        {
            Items = items.Select(p => p.ToDto()).ToList(),
            Page = normalizedPage,
            Limit = normalizedLimit,
            TotalCount = total
        };
    }

    public async Task<PostDto> GetByIdAsync(string id, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var post = await _posts.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Post not found.");

        return post.ToDto();
    }

    public async Task<PostDto> CreateAsync(string currentUserId, CreatePostRequest request, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(currentUserId);

        var title = request.Title.Trim();
        var content = request.Content.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            throw new BadRequestException("Title and content are required.");
        }

        var now = DateTime.UtcNow;
        var post = new Post
        {
            UserId = currentUserId,
            Title = title,
            Content = content,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _posts.CreateAsync(post, ct);
        return created.ToDto();
    }

    public async Task<PostDto> UpdateAsync(string id, string currentUserId, UpdatePostRequest request, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var post = await _posts.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Post not found.");

        if (post.UserId != currentUserId)
        {
            throw new ForbiddenException("You can only edit your own posts.");
        }

        var title = request.Title.Trim();
        var content = request.Content.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            throw new BadRequestException("Title and content are required.");
        }

        post.Title = title;
        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;

        await _posts.UpdateAsync(post, ct);
        return post.ToDto();
    }

    public async Task DeleteAsync(string id, string currentUserId, CancellationToken ct = default)
    {
        UserService.EnsureValidObjectId(id);

        var post = await _posts.GetByIdAsync(id, ct)
            ?? throw new NotFoundException("Post not found.");

        if (post.UserId != currentUserId)
        {
            throw new ForbiddenException("You can only delete your own posts.");
        }

        await _posts.DeleteAsync(id, ct);
    }
}

