using MediatR;

namespace User.API.Application.Queries;

public sealed record GetUserByIdQuery(Guid Id) : IRequest<UserResponse?>;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);