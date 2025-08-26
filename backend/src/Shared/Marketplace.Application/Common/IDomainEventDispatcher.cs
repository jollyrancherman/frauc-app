using Marketplace.Domain.Common;

namespace Marketplace.Application.Common;

public interface IDomainEventDispatcher
{
    Task DispatchDomainEventsAsync<T, TId>(T entity, CancellationToken cancellationToken = default) 
        where T : Entity<TId>
        where TId : notnull;
}