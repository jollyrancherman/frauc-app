using Marketplace.Domain.Common;
using Marketplace.Domain.Users.Events;
using Marketplace.Domain.Users.ValueObjects;

namespace Marketplace.Domain.Users;

public sealed class User : Entity<UserId>
{
    public Email Email { get; private set; }
    public Username Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private User(UserId id, Email email, Username username, string firstName, string lastName) 
        : base(id)
    {
        Email = email;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(UserId id, Email email, Username username, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be null or empty", nameof(lastName));

        var user = new User(id, email, username, firstName, lastName);

        user.AddDomainEvent(new UserCreatedEvent(
            id,
            email.Value,
            username.Value,
            firstName,
            lastName,
            user.CreatedAt));

        return user;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("FirstName cannot be null or empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("LastName cannot be null or empty", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new UserProfileUpdatedEvent(
            Id,
            firstName,
            lastName,
            UpdatedAt.Value));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}