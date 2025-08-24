using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Commands;

namespace User.API.Application.Handlers;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResponse>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<CreateUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.New();
        var email = Email.Create(request.Email);
        var username = Username.Create(request.Username);

        if (await _userRepository.ExistsWithEmailAsync(email, cancellationToken))
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists");
        }

        if (await _userRepository.ExistsWithUsernameAsync(username, cancellationToken))
        {
            throw new InvalidOperationException($"User with username {request.Username} already exists");
        }

        var user = Marketplace.Domain.Users.User.Create(
            userId,
            email,
            username,
            request.FirstName,
            request.LastName);

        var addedUser = await _userRepository.AddAsync(user, cancellationToken);

        return new CreateUserResponse(
            addedUser.Id.Value,
            addedUser.Email.Value,
            addedUser.Username.Value,
            addedUser.FirstName,
            addedUser.LastName,
            addedUser.CreatedAt);
    }
}