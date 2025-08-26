using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.Exceptions;

public class ItemNotFoundException : Exception
{
    public ItemId ItemId { get; }

    public ItemNotFoundException(ItemId itemId) 
        : base($"Item with ID '{itemId}' was not found.")
    {
        ItemId = itemId;
    }
}