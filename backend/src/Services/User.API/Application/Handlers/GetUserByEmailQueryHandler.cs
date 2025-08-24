using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Queries;

namespace User.API.Application.Handlers;

public sealed class GetUserByEmailQueryHandler : IRequestHandler<GetUserByEmailQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByEmailQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserResponse?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

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