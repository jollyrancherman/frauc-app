using FluentAssertions;
using Xunit;
using Marketplace.Domain.Listings.ValueObjects;

namespace Marketplace.Domain.Tests.Listings;

public class ValueObjectTests
{
    public class LocationTests
    {
        [Fact]
        public void Create_WithValidCoordinates_ShouldCreateLocation()
        {
            // Arrange
            var latitude = 40.7128;
            var longitude = -74.0060;

            // Act
            var location = new Location(latitude, longitude);

            // Assert
            location.Latitude.Should().Be(latitude);
            location.Longitude.Should().Be(longitude);
        }

        [Theory]
        [InlineData(91, 0)] // Latitude too high
        [InlineData(-91, 0)] // Latitude too low
        [InlineData(0, 181)] // Longitude too high
        [InlineData(0, -181)] // Longitude too low
        public void Create_WithInvalidCoordinates_ShouldThrowArgumentException(double latitude, double longitude)
        {
            // Act & Assert
            var action = () => new Location(latitude, longitude);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void DistanceTo_WithAnotherLocation_ShouldCalculateDistance()
        {
            // Arrange - New York to Los Angeles approximately 3944 km
            var newYork = new Location(40.7128, -74.0060);
            var losAngeles = new Location(34.0522, -118.2437);

            // Act
            var distance = newYork.DistanceTo(losAngeles);

            // Assert
            distance.Should().BeApproximately(3944, 50); // Within 50 km tolerance
        }
    }

    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidAmount_ShouldCreateMoney()
        {
            // Arrange
            var amount = 100.50m;
            var currency = "USD";

            // Act
            var money = new Money(amount, currency);

            // Assert
            money.Amount.Should().Be(amount);
            money.Currency.Should().Be(currency);
        }

        [Fact]
        public void Create_WithNegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            var amount = -10m;
            var currency = "USD";

            // Act & Assert
            var action = () => new Money(amount, currency);

            action.Should().Throw<ArgumentException>()
                .WithMessage("*amount*");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Create_WithInvalidCurrency_ShouldThrowArgumentException(string invalidCurrency)
        {
            // Act & Assert
            var action = () => new Money(100m, invalidCurrency);

            action.Should().Throw<ArgumentException>()
                .WithMessage("*currency*");
        }

        [Fact]
        public void Zero_ShouldReturnMoneyWithZeroAmount()
        {
            // Act
            var zero = Money.Zero;

            // Assert
            zero.Amount.Should().Be(0m);
            zero.Currency.Should().Be("USD");
        }

        [Fact]
        public void Equals_WithSameAmountAndCurrency_ShouldReturnTrue()
        {
            // Arrange
            var money1 = new Money(100m, "USD");
            var money2 = new Money(100m, "USD");

            // Act & Assert
            money1.Should().Be(money2);
        }

        [Fact]
        public void Equals_WithDifferentCurrency_ShouldReturnFalse()
        {
            // Arrange
            var money1 = new Money(100m, "USD");
            var money2 = new Money(100m, "EUR");

            // Act & Assert
            money1.Should().NotBe(money2);
        }
    }

    public class AuctionSettingsTests
    {
        [Fact]
        public void CreateForwardAuction_WithValidData_ShouldCreateSettings()
        {
            // Arrange
            var startingPrice = new Money(100m, "USD");
            var reservePrice = new Money(200m, "USD");
            var duration = TimeSpan.FromDays(7);

            // Act
            var settings = AuctionSettings.CreateForwardAuction(startingPrice, reservePrice, duration);

            // Assert
            settings.StartingPrice.Should().Be(startingPrice);
            settings.ReservePrice.Should().Be(reservePrice);
            settings.Duration.Should().Be(duration);
            settings.MaxPrice.Should().BeNull();
        }

        [Fact]
        public void CreateReverseAuction_WithValidData_ShouldCreateSettings()
        {
            // Arrange
            var maxPrice = new Money(500m, "USD");
            var duration = TimeSpan.FromDays(5);

            // Act
            var settings = AuctionSettings.CreateReverseAuction(maxPrice, duration);

            // Assert
            settings.MaxPrice.Should().Be(maxPrice);
            settings.Duration.Should().Be(duration);
            settings.StartingPrice.Should().BeNull();
            settings.ReservePrice.Should().BeNull();
        }

        [Fact]
        public void CreateForwardAuction_WithReservePriceLowerThanStarting_ShouldThrowArgumentException()
        {
            // Arrange
            var startingPrice = new Money(200m, "USD");
            var reservePrice = new Money(100m, "USD");
            var duration = TimeSpan.FromDays(7);

            // Act & Assert
            var action = () => AuctionSettings.CreateForwardAuction(startingPrice, reservePrice, duration);

            action.Should().Throw<ArgumentException>()
                .WithMessage("*Reserve price must be greater than or equal to starting price*");
        }
    }
}