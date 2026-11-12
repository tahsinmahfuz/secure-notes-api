using Application.DTOs;
using Domain.Entities;
using Domain.Models;

namespace Application.Mapping;

public static class DtoMapper
{
    public static UserDto ToDto(this User user) =>
        new(
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.Interests,
            user.CreatedAt,
            user.UpdatedAt);

    public static NoteDto ToDto(this Note note) =>
        new(
            note.Id,
            note.UserId,
            note.Title,
            note.Content,
            note.CreatedAt,
            note.UpdatedAt);

    public static PostDto ToDto(this Post post) =>
        new(
            post.Id,
            post.UserId,
            post.Title,
            post.Content,
            post.CreatedAt,
            post.UpdatedAt);

    public static InterestGroupDto ToDto(this InterestGroup group) =>
        new(
            group.Interest,
            group.UserCount,
            group.Users.Select(u => new InterestUserDto(u.Id, u.Name, u.Email, u.Role.ToString())).ToList());

    public static UserPostsDto ToDto(this UserWithPosts source, int page, int limit) =>
        new(
            source.Id,
            source.Name,
            source.Email,
            source.Role.ToString(),
            source.Interests,
            source.CreatedAt,
            page,
            limit,
            source.HasMorePosts,
            source.Posts.Select(p => p.ToDto()).ToList());
}

