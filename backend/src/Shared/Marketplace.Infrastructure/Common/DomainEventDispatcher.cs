using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Domain.Common;

namespace Marketplace.Infrastructure.Common;

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IMediator _mediator;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IMediator mediator, ILogger<DomainEventDispatcher> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchDomainEventsAsync<T, TId>(T entity, CancellationToken cancellationToken = default) 
        where T : Entity<TId> 
        where TId : notnull
    {
        var domainEvents = entity.DomainEvents.ToList();
        entity.ClearDomainEvents();

        _logger.LogDebug("Dispatching {EventCount} domain events for entity {EntityType} with ID {EntityId}",
            domainEvents.Count, typeof(T).Name, entity.Id);

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogDebug("Publishing domain event {EventType}", domainEvent.GetType().Name);
                await _mediator.Publish(domainEvent, cancellationToken);
                _logger.LogDebug("Successfully published domain event {EventType}", domainEvent.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing domain event {EventType}", domainEvent.GetType().Name);
                throw;
            }
        }

        _logger.LogDebug("Successfully dispatched all domain events for entity {EntityType} with ID {EntityId}",
            typeof(T).Name, entity.Id);
    }
}