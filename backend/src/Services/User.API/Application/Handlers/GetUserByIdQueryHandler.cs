using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using MediatR;
using User.API.Application.Queries;

namespace User.API.Application.Handlers;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    public async Task<UserResponse?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.Id);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

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