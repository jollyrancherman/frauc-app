using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Queries;

namespace User.API.Application.Handlers;

public sealed class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserResponse?> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var username = Username.Create(request.Username);
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new UserResponse(
            user.Id.Value,
            user.Email.Value,
            user.Username.Value,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt);
    }
}