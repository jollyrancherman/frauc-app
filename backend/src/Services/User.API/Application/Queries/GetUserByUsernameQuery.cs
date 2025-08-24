using MediatR;

namespace User.API.Application.Queries;

public sealed record GetUserByUsernameQuery(string Username) : IRequest<UserResponse?>;