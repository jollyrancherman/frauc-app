# CLAUDE.md - Backend Development Guide

This file provides guidance to Claude Code for working with the .NET 9 backend microservices.

## Backend Architecture Overview

This backend implements a microservices architecture using Domain-Driven Design (DDD) principles with .NET 9.

### Solution Structure
```
backend/
├── src/
│   ├── Services/                 # Microservices
│   │   ├── User.API/            # User management service
│   │   ├── Listing.API/         # Listing and product service
│   │   ├── Bidding.API/         # Bidding and auction service
│   │   ├── Payment.API/         # Payment processing service
│   │   ├── Messaging.API/       # Real-time messaging service
│   │   ├── Search.API/          # Elasticsearch search service
│   │   ├── Notification.API/    # Notification service
│   │   └── Analytics.API/       # Analytics and reporting service
│   │
│   ├── Shared/                   # Shared libraries
│   │   ├── Marketplace.Domain/  # Core domain models and interfaces
│   │   ├── Marketplace.Application/ # Application services, CQRS
│   │   ├── Marketplace.Infrastructure/ # Infrastructure implementations
│   │   └── Marketplace.Common/  # Common utilities and extensions
│   │
│   └── ApiGateway/              # Ocelot API Gateway
│       └── Marketplace.Gateway/
│
├── tests/                        # Test projects
│   ├── Unit/                    # Unit tests for each service
│   ├── Integration/             # Integration tests
│   └── Performance/             # Load and performance tests
│
└── Marketplace.sln              # Solution file
```

## Domain-Driven Design Implementation

### Each Service Follows This Structure:
```
Service.API/
├── Domain/                # Domain layer
│   ├── Aggregates/       # Aggregate roots
│   ├── Entities/         # Domain entities
│   ├── ValueObjects/     # Value objects
│   ├── Events/           # Domain events
│   └── Interfaces/       # Domain interfaces
│
├── Application/           # Application layer
│   ├── Commands/         # CQRS commands
│   ├── Queries/          # CQRS queries
│   ├── Handlers/         # Command/query handlers
│   ├── Validators/       # FluentValidation validators
│   └── DTOs/             # Data transfer objects
│
├── Infrastructure/        # Infrastructure layer
│   ├── Persistence/      # EF Core DbContext and configurations
│   ├── Repositories/     # Repository implementations
│   ├── Services/         # External service integrations
│   └── Migrations/       # Database migrations
│
├── API/                   # Presentation layer
│   ├── Controllers/      # API controllers
│   ├── Middleware/       # Custom middleware
│   ├── Filters/          # Action filters
│   └── SignalR/          # SignalR hubs (if applicable)
│
└── Program.cs            # Application entry point
```

## Testing Strategy (TDD with 100% Coverage)

### Test Project Structure:
```
tests/
├── User.Service.Tests/
│   ├── Domain/           # Domain logic tests
│   ├── Application/      # Application service tests
│   ├── Infrastructure/   # Infrastructure tests
│   └── API/              # Controller tests
│
└── User.Service.IntegrationTests/
    ├── Fixtures/         # Test fixtures and data
    ├── Scenarios/        # End-to-end scenarios
    └── TestContainers/   # Container configurations
```

### TDD Workflow:
1. **Write failing test first** (RED phase)
2. **Implement minimal code to pass** (GREEN phase)
3. **Refactor while maintaining tests** (REFACTOR phase)
4. **Ensure 100% code coverage before commit**

## Key Patterns and Conventions

### CQRS Pattern
- Commands for write operations (mutations)
- Queries for read operations
- MediatR for handling commands/queries
- Separate read/write models when beneficial

### Repository Pattern
- Generic repository interface in Domain layer
- Concrete implementations in Infrastructure layer
- Unit of Work pattern for transaction management

### Domain Events
- MediatR for in-process domain events
- Integration events via message bus (RabbitMQ/Azure Service Bus)
- Event sourcing for audit-critical operations

### Validation
- FluentValidation for request validation
- Domain validation in aggregate roots
- Guard clauses for preconditions

## Service-Specific Configurations

### User Service
- Keycloak integration for authentication
- User profile management
- Ratings and verification system

### Listing Service
- Product catalog with categories
- Listing lifecycle management
- PostGIS integration for geospatial queries

### Bidding Service
- SignalR for real-time bidding
- Concurrency handling for bid placement
- Redis for bid caching

### Payment Service
- Stripe integration
- Escrow implementation
- Commission calculation engine

## Database Strategy

### PostgreSQL Configuration
- One database per service (microservice pattern)
- Shared database for read-heavy cross-service queries (optional)
- PostGIS extension for geospatial features

### Entity Framework Core
- Code-first migrations
- Fluent API for entity configuration
- Optimistic concurrency control
- Soft deletes for audit trail

## SignalR Implementation

### Hub Structure
```csharp
public class BiddingHub : Hub<IBiddingClient>
{
    // Strongly-typed hub implementation
}
```

### Redis Backplane
- Configure Redis for SignalR scaling
- Connection management for real-time features

## Authentication & Authorization

### JWT Bearer Authentication
- Keycloak as identity provider
- JWT validation middleware
- Role-based and policy-based authorization

### Service-to-Service Communication
- Internal API keys or mTLS
- Service discovery via API Gateway

## Development Commands

### Solution Management
```bash
# Create new solution
dotnet new sln -n Marketplace

# Add project to solution
dotnet sln add src/Services/User.API/User.API.csproj

# Build entire solution
dotnet build

# Run tests with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### Service Creation
```bash
# Create new Web API project
dotnet new webapi -n Service.API -f net9.0

# Add required packages
dotnet add package Microsoft.EntityFrameworkCore.PostgreSQL
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Testing
```bash
# Create test project
dotnet new xunit -n Service.Tests

# Add test packages
dotnet add package FluentAssertions
dotnet add package NSubstitute
dotnet add package Testcontainers.PostgreSql
dotnet add package coverlet.collector
```

### Database Migrations
```bash
# Add migration
dotnet ef migrations add InitialCreate --project src/Services/User.API

# Update database
dotnet ef database update --project src/Services/User.API

# Generate SQL script
dotnet ef migrations script --project src/Services/User.API
```

## Code Quality Standards

### Naming Conventions
- PascalCase for public members
- camelCase for private fields
- Async suffix for async methods
- I prefix for interfaces

### Code Organization
- One class per file
- Organize by feature, not by type
- Keep controllers thin
- Business logic in domain/application layers

### Performance Considerations
- Use async/await throughout
- Implement caching strategically
- Optimize database queries (includes, projections)
- Use pagination for list endpoints

## CI/CD Integration

### Build Pipeline Requirements
- Build all services
- Run all unit tests
- Check code coverage (must be 100%)
- Run code analysis (SonarQube)
- Security scanning (dependency check)

### Deployment Considerations
- Health checks for each service
- Graceful shutdown handling
- Rolling deployments
- Database migration strategy

## Common Pitfalls to Avoid

1. **Don't share databases between services** - Each service owns its data
2. **Don't create circular dependencies** - Use events for cross-service communication
3. **Don't bypass the domain layer** - All business logic goes through aggregates
4. **Don't ignore test coverage** - Maintain 100% coverage
5. **Don't use synchronous service-to-service calls** - Prefer async messaging

## Monitoring and Observability

### Application Insights Integration
- Structured logging with Serilog
- Distributed tracing
- Custom metrics and events
- Performance counters

### Health Checks
- Liveness probe: `/health/live`
- Readiness probe: `/health/ready`
- Database connectivity checks
- External service dependency checks

## Security Best Practices

1. **Input validation** - Validate all inputs
2. **SQL injection prevention** - Use parameterized queries via EF Core
3. **Sensitive data** - Never log PII or sensitive information
4. **HTTPS only** - Enforce HTTPS in production
5. **Rate limiting** - Implement per-user rate limiting
6. **CORS configuration** - Configure appropriately for frontend

## Backend Commit Strategy

### Commit Granularity for Services

When implementing a new microservice, follow this commit pattern:

#### Day 1: Domain Layer
```bash
git commit -m "feat: Add [Service] aggregate with business rules"
git commit -m "feat: Add [Service] value objects and domain events"
git commit -m "test: Add [Service] domain unit tests"
```

#### Day 2: Application Layer
```bash
git commit -m "feat: Add [Service] CQRS commands and handlers"
git commit -m "feat: Add [Service] queries and projections"
git commit -m "test: Add [Service] application layer tests"
```

#### Day 3: Infrastructure Layer
```bash
git commit -m "feat: Add [Service] repository with EF Core"
git commit -m "feat: Add [Service] database migrations"
git commit -m "test: Add [Service] integration tests with TestContainers"
```

#### Day 4: API Layer
```bash
git commit -m "feat: Add [Service] API controller with endpoints"
git commit -m "feat: Add [Service] OpenAPI documentation"
git commit -m "test: Add [Service] API integration tests"
```

### Testing Requirements Per Commit
- Domain commits: Include unit tests in same commit
- Application commits: Include handler tests
- Infrastructure commits: Include integration tests
- API commits: Include controller and E2E tests
- **Target**: 100% coverage for new code in each commit

### Example Week 2 Commits (Product & Listing)
```bash
# Day 1
feat: Add Product aggregate with specifications pattern
feat: Add Category aggregate with hierarchical structure

# Day 2
feat: Add Product CQRS commands and handlers
feat: Add Product search queries with filtering

# Day 3
feat: Add Product repository with EF Core
feat: Add Elasticsearch product indexing

# Day 4
feat: Add Listing aggregate with auction types
feat: Add Listing CQRS operations

# Day 5
feat: Add Products API controller
feat: Add Listings API controller
feat: Add PostGIS geospatial search
```

## Getting Started Checklist

When creating a new service:
- [ ] Create project structure following DDD
- [ ] Set up test project with 100% coverage target
- [ ] Configure Entity Framework with PostgreSQL
- [ ] Add health checks
- [ ] Configure authentication/authorization
- [ ] Set up logging with Serilog
- [ ] Add API versioning
- [ ] Configure Swagger/OpenAPI
- [ ] Implement error handling middleware
- [ ] Add validation with FluentValidation
- [ ] Set up MediatR for CQRS
- [ ] Configure AutoMapper for DTO mapping
- [ ] Add integration tests with TestContainers
- [ ] Ensure Docker support