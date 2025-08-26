using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Items.Exceptions;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.Handlers;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Result<ItemDto>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<CreateItemCommandHandler> _logger;

    public CreateItemCommandHandler(
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<CreateItemCommandHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _domainEventDispatcher = domainEventDispatcher ?? throw new ArgumentNullException(nameof(domainEventDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ItemDto>> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating item with ID {ItemId} for seller {SellerId}", 
                request.ItemId, request.SellerId);

            // Note: Existence validation is now handled by FluentValidation in ValidationBehavior
            // This eliminates duplicate validation logic

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // Create domain entity
            var item = Marketplace.Domain.Items.Item.Create(
                request.ItemId,
                request.Title,
                request.Description,
                request.CategoryId,
                request.SellerId,
                request.Condition
            );

            // Save to repository
            await _itemRepository.AddAsync(item, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Dispatch domain events
            await _domainEventDispatcher.DispatchDomainEventsAsync<Item, ItemId>(item, cancellationToken);

            // Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully created item {ItemId}", request.ItemId);

            return Result.Success(ItemDto.FromDomain(item));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create item {ItemId}", request.ItemId);
            
            // UnitOfWork should handle rollback automatically on exception
            // But ensure it's called explicitly for clarity
            try
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Error during transaction rollback for item {ItemId}", request.ItemId);
            }

            return Result.Failure<ItemDto>($"Failed to create item: {ex.Message}");
        }
    }
}