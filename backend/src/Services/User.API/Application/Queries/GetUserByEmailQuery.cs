using MediatR;

namespace User.API.Application.Queries;

public sealed record GetUserByEmailQuery(string Email) : IRequest<UserResponse?>;