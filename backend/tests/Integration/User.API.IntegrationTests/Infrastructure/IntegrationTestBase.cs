using Marketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace User.API.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
        .WithImage("postgis/postgis:17-3.4")
        .WithDatabase("marketplace_test")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    protected IServiceProvider ServiceProvider { get; private set; } = null!;
    protected MarketplaceDbContext DbContext { get; private set; } = null!;

    public virtual async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var services = new ServiceCollection();
        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
        DbContext = ServiceProvider.GetRequiredService<MarketplaceDbContext>();

        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (ServiceProvider is IAsyncDisposable asyncDisposable)
            await asyncDisposable.DisposeAsync();
        else if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();

        await _postgresContainer.StopAsync();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MarketplaceDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));
    }

    protected async Task<T> ExecuteInTransactionAsync<T>(Func<MarketplaceDbContext, Task<T>> operation)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            var result = await operation(DbContext);
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    protected async Task ExecuteInTransactionAsync(Func<MarketplaceDbContext, Task> operation)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await operation(DbContext);
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}