using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Commands;

namespace User.API.Application.Handlers;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.Id);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        user.Deactivate();
        await _userRepository.UpdateAsync(user, cancellationToken);

        return true;
    }
}