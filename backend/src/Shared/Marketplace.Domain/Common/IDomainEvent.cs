using MediatR;

namespace Marketplace.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn => DateTime.UtcNow;
}