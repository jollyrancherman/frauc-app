using MediatR;

namespace User.API.Application.Commands;

public sealed record UpdateUserCommand(
    Guid Id,
    string FirstName,
    string LastName
) : IRequest<UpdateUserResponse>;

public sealed record UpdateUserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    DateTime UpdatedAt
);