using System.Text.RegularExpressions;
using Marketplace.Domain.Common;

namespace Marketplace.Domain.Users.ValueObjects;

public sealed class Username : ValueObject
{
    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_-]+$",
        RegexOptions.Compiled);

    public string Value { get; private set; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));

        if (username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters long", nameof(username));

        if (username.Length > 50)
            throw new ArgumentException("Username cannot exceed 50 characters", nameof(username));

        if (!UsernameRegex.IsMatch(username))
            throw new ArgumentException("Username can only contain letters, numbers, underscores and hyphens", nameof(username));

        return new Username(username.ToLowerInvariant());
    }

    public static implicit operator string(Username username) => username.Value;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}