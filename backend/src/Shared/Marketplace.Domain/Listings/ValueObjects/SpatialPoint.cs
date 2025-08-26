using Marketplace.Domain.Common;

namespace Marketplace.Domain.Listings.ValueObjects;

/// <summary>
/// Domain abstraction for spatial coordinates without infrastructure dependencies
/// </summary>
public sealed class SpatialPoint : ValueObject
{
    public double Longitude { get; }
    public double Latitude { get; }

    public SpatialPoint(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
        
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

        Latitude = latitude;
        Longitude = longitude;
    }

    public static SpatialPoint FromLocation(Location location)
    {
        return new SpatialPoint(location.Latitude, location.Longitude);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"({Latitude}, {Longitude})";
}