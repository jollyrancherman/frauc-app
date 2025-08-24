using MediatR;

namespace User.API.Application.Commands;

public sealed record DeactivateUserCommand(Guid Id) : IRequest<bool>;