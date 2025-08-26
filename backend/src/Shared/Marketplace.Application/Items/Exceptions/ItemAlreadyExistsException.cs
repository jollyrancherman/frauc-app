using Marketplace.Domain.Items.ValueObjects;

namespace Marketplace.Application.Items.Exceptions;

public class ItemAlreadyExistsException : Exception
{
    public ItemId ItemId { get; }

    public ItemAlreadyExistsException(ItemId itemId) 
        : base($"Item with ID '{itemId}' already exists.")
    {
        ItemId = itemId;
    }
}