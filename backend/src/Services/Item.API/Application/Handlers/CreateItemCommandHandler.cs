using MediatR;
using Item.API.Application.Commands;
using Item.API.Application.DTOs;
using Marketplace.Domain.Items;

namespace Item.API.Application.Handlers;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IItemRepository _itemRepository;

    public CreateItemCommandHandler(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
    }

    public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = Marketplace.Domain.Items.Item.Create(
            request.ItemId,
            request.Title,
            request.Description,
            request.CategoryId,
            request.SellerId,
            request.Condition
        );

        await _itemRepository.AddAsync(item, cancellationToken);

        return ItemDto.FromDomain(item);
    }
}