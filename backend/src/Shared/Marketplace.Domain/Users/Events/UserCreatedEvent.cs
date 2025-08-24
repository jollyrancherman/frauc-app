using Marketplace.Domain.Users.ValueObjects;
using MediatR;

namespace Marketplace.Domain.Users.Events;

public sealed record UserCreatedEvent(
    UserId UserId,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    DateTime CreatedAt) : INotification;