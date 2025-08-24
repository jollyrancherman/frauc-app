using FluentAssertions;
using Marketplace.Domain.Users;
using Marketplace.Domain.Users.ValueObjects;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Testcontainers.PostgreSql;
using User.API.Application.Commands;
using User.API.Application.Queries;

namespace User.API.IntegrationTests.Controllers;

public class UsersControllerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:17-3.4")
        .WithDatabase("marketplace_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add the test database
                    services.AddDbContext<MarketplaceDbContext>(options =>
                        options.UseNpgsql(_postgresContainer.GetConnectionString()));

                    services.AddScoped<IUserRepository, UserRepository>();
                });
            });

        _client = _factory.CreateClient();

        // Ensure database is created
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgresContainer.StopAsync();
    }

    [Fact]
    public async Task CreateUser_ShouldReturnCreated_WhenValidRequest()
    {
        // Arrange
        var command = new CreateUserCommand(
            "test@example.com",
            "testuser",
            "Test",
            "User");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createUserResponse = JsonSerializer.Deserialize<CreateUserResponse>(
            responseContent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        createUserResponse.Should().NotBeNull();
        createUserResponse!.Email.Should().Be(command.Email);
        createUserResponse.Username.Should().Be(command.Username);
        createUserResponse.FirstName.Should().Be(command.FirstName);
        createUserResponse.LastName.Should().Be(command.LastName);
        createUserResponse.Id.Should().NotBe(Guid.Empty);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/Users/{createUserResponse.Id}");
    }

    [Fact]
    public async Task CreateUser_ShouldReturnConflict_WhenEmailAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand(
            "duplicate@example.com",
            "user1",
            "First",
            "User");

        // Create first user
        await _client.PostAsJsonAsync("/api/users", command);

        // Try to create user with same email but different username
        var duplicateCommand = new CreateUserCommand(
            "duplicate@example.com",
            "user2",
            "Second",
            "User");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnConflict_WhenUsernameAlreadyExists()
    {
        // Arrange
        var command = new CreateUserCommand(
            "user1@example.com",
            "duplicateuser",
            "First",
            "User");

        // Create first user
        await _client.PostAsJsonAsync("/api/users", command);

        // Try to create user with same username but different email
        var duplicateCommand = new CreateUserCommand(
            "user2@example.com",
            "duplicateuser",
            "Second",
            "User");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", duplicateCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUnauthorized_WhenNoToken()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/users/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenInvalidEmail()
    {
        // Arrange
        var command = new CreateUserCommand(
            "invalid-email",
            "testuser",
            "Test",
            "User");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnBadRequest_WhenEmptyFields()
    {
        // Arrange
        var command = new CreateUserCommand("", "", "", "");

        // Act
        var response = await _client.PostAsJsonAsync("/api/users", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static Marketplace.Domain.Users.User CreateTestUser(string email = "test@example.com", string username = "testuser")
    {
        var userId = UserId.New();
        var emailObj = Email.Create(email);
        var usernameObj = Username.Create(username);
        return Marketplace.Domain.Users.User.Create(userId, emailObj, usernameObj, "Test", "User");
    }
}