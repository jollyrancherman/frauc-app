using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Commands;

namespace User.API.Application.Handlers;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UpdateUserResponse>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UpdateUserResponse> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.Id);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.Id} not found");
        }

        user.UpdateProfile(request.FirstName, request.LastName);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UpdateUserResponse(
            user.Id.Value,
            user.Email.Value,
            user.Username.Value,
            user.FirstName,
            user.LastName,
            user.UpdatedAt!.Value);
    }
}