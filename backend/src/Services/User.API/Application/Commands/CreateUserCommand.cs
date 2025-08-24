using MediatR;

namespace User.API.Application.Commands;

public sealed record CreateUserCommand(
    string Email,
    string Username, 
    string FirstName,
    string LastName
) : IRequest<CreateUserResponse>;

public sealed record CreateUserResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    DateTime CreatedAt
);