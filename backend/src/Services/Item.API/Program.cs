using MediatR;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Marketplace.Infrastructure.Data;
using Marketplace.Infrastructure.Repositories;
using Marketplace.Infrastructure.Common;
using Marketplace.Domain.Items;
using Marketplace.Application.Common;
using Marketplace.Application.Items.Handlers;
using Marketplace.Application.Items.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<MarketplaceDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MediatR
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(CreateItemCommandHandler).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

// FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(CreateItemCommandValidator).Assembly);

// Repository pattern
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Memory Cache
builder.Services.AddMemoryCache();

// Logging
builder.Services.AddLogging();

// Authentication (configure as needed)
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         // Configure JWT settings
//     });

// CORS (configure as needed for frontend)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Ensure database is created (for development)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
    await context.Database.EnsureCreatedAsync();
}

app.Run();

// Make the implicit Program class accessible to integration tests
public partial class Program { }