using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Common;
using Marketplace.Domain.Items;
using Marketplace.Application.Common;
using Testcontainers.PostgreSql;

namespace Item.API.IntegrationTests.Infrastructure;

public class IntegrationTestBase : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:17-3.4")
        .WithDatabase("marketplace_item_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    protected MarketplaceDbContext DbContext { get; private set; } = null!;
    protected IItemRepository ItemRepository { get; private set; } = null!;
    protected IUnitOfWork UnitOfWork { get; private set; } = null!;
    
    public virtual async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        
        using var scope = Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
        ItemRepository = scope.ServiceProvider.GetRequiredService<IItemRepository>();
        UnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        await DbContext.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<MarketplaceDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database context
            services.AddDbContext<MarketplaceDbContext>(options =>
                options.UseNpgsql(_postgresContainer.GetConnectionString()));

            // Ensure all required services are registered
            services.AddScoped<IItemRepository, ItemRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

            // Add authentication services for testing
            services.AddAuthentication("Test")
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });
            
            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("Test")
                    .RequireAuthenticatedUser()
                    .Build();
            });
        });

        builder.UseEnvironment("Testing");
    }

    protected async Task<T> ExecuteInScopeAsync<T>(Func<IServiceProvider, Task<T>> operation)
    {
        using var scope = Services.CreateScope();
        return await operation(scope.ServiceProvider);
    }

    protected async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> operation)
    {
        using var scope = Services.CreateScope();
        await operation(scope.ServiceProvider);
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<MarketplaceDbContext, Task<T>> operation)
    {
        return await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var result = await operation(context);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    protected async Task ExecuteInTransactionAsync(Func<MarketplaceDbContext, Task> operation)
    {
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await operation(context);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    protected async Task SeedAsync<T>(params T[] entities) where T : class
    {
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            await context.Set<T>().AddRangeAsync(entities);
            await context.SaveChangesAsync();
        });
    }

    protected async Task ClearDatabase()
    {
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            // Clear all data while preserving schema
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"ItemImages\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Items\" CASCADE");
            await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" CASCADE");
        });
    }
}