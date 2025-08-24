using Marketplace.Domain.Users.ValueObjects;
using MediatR;

namespace Marketplace.Domain.Users.Events;

public sealed record UserProfileUpdatedEvent(
    UserId UserId,
    string FirstName,
    string LastName,
    DateTime UpdatedAt) : INotification;