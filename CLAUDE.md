# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Facebook Marketplace-style auction platform built with a microservice architecture. The platform supports both forward auctions (highest bidder wins) and reverse auctions (lowest bidder wins), along with fixed-price listings.

**Tech Stack:**
- Backend: .NET 9 microservices with Domain-Driven Design (DDD)
- Frontend: NextJS 15 with App Router (mobile-responsive)
- Authentication: Keycloak with social login integration
- Database: PostgreSQL 17 with PostGIS for geospatial features
- Real-time: SignalR for live bidding and messaging
- Caching: Redis
- Search: Elasticsearch
- Testing: 100% test coverage using TDD methodology

## Architecture

### Microservices Structure
The platform consists of 15 core microservices organized by domain:

**Core Business Services:**
- User Service (profiles, ratings, verification)
- Product Service (catalog, categories, specifications)
- Listing Service (auctions, fixed-price listings)
- Bidding Service (bid management, auto-bidding, proxy bidding)
- Payment Service (Stripe integration, escrow, commission tracking)
- Messaging Service (SignalR real-time chat)
- Geolocation Service (PostGIS spatial queries)

**Platform Services:**
- Search Service (Elasticsearch integration)
- Analytics Service (business intelligence, user behavior)
- Notification Service (real-time alerts, email/SMS)
- File Storage Service (Azure Blob + CDN)
- Dispute Service (resolution workflows)
- Advertising Service (AdSense + native ads)
- Admin Service (content moderation, platform management)
- Revenue Service (monetization, subscription management)

### Domain-Driven Design Implementation
- Each service follows DDD with domain aggregates, entities, and value objects
- CQRS pattern for complex business operations
- Domain events for cross-service communication via MediatR
- Repository pattern with Entity Framework Core 9

## Development Commands

### Backend (.NET 9)
```bash
# Navigate to backend directory
cd backend/

# Build all services
dotnet build

# Run specific service with hot reload
dotnet watch --project src/Services/User.API

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/User.Service.Tests

# Database migrations
dotnet ef migrations add <MigrationName> --project src/Services/User.API
dotnet ef database update --project src/Services/User.API
```

### Frontend (NextJS 15)
```bash
# Navigate to frontend directory
cd frontend/

# Install dependencies
npm ci

# Run development server
npm run dev

# Build for production
npm run build

# Run tests with coverage
npm run test -- --coverage --watchAll=false

# Run E2E tests
npx playwright test
```

### Infrastructure
```bash
# Start local development environment from root
docker-compose -f infrastructure/docker/docker-compose.dev.yml up -d

# Start test environment
docker-compose -f infrastructure/docker/docker-compose.test.yml up -d

# View logs
docker-compose -f infrastructure/docker/docker-compose.dev.yml logs -f [service-name]

# For convenience, you can also use from infrastructure/docker directory:
cd infrastructure/docker/
docker-compose -f docker-compose.dev.yml up -d
```

## Testing Strategy

### Test-Driven Development (TDD)
All development follows strict TDD with Red-Green-Refactor cycles:
1. **RED**: Write failing test first
2. **GREEN**: Write minimal code to pass
3. **REFACTOR**: Improve code while maintaining tests

### Testing Frameworks
- **Backend**: xUnit with FluentAssertions, NSubstitute for mocking, TestContainers for integration tests
- **Frontend**: Jest with React Testing Library, MSW for API mocking
- **E2E**: Playwright for cross-browser testing
- **Load Testing**: NBomber for .NET services, K6 for frontend

### Coverage Requirements
- **Minimum**: 100% line coverage for all services
- **Quality Gates**: No PR merges below 100% coverage
- **Integration**: TestContainers for database testing
- **Performance**: Load testing with realistic user scenarios

## Environment Configuration

### Development Environment
Local development uses Docker Compose with:
- Keycloak (port 8080) for authentication
- PostgreSQL 17 (port 5432) with PostGIS extension
- Redis (port 6379) for caching and SignalR backplane  
- Elasticsearch (port 9200) for search functionality

### Authentication Setup
Keycloak realms configured for:
- Regular users (buyers)
- Business sellers with verification
- Platform administrators
- Social login providers (Google, Facebook, Apple)

### Database Strategy
- PostgreSQL 17 as primary database with PostGIS for geospatial queries
- JSONB columns for flexible product attributes and metadata
- Table partitioning for bid history and analytics data
- Row Level Security for multi-tenant data isolation

## Key Patterns & Conventions

### SignalR Implementation
Real-time features use SignalR with Redis backplane for scaling:
- Bidding hubs for live auction updates
- Messaging hubs for buyer-seller communication
- Notification hubs for system alerts
- Authentication via JWT tokens from Keycloak

### Payment Processing
Stripe integration with comprehensive error handling:
- Escrow system for secure transactions
- Commission calculation engine
- Subscription billing for premium features
- PCI compliance validation

### Monetization Features
Revenue generation through:
- Dynamic commission fees (configurable percentage + fixed)
- Google AdSense programmatic advertising
- Native advertising platform for sponsored listings
- Premium seller subscriptions with advanced analytics

## Development Workflow

### Version Control
- Feature branches for each development section
- PR required with 100% test coverage
- Code review mandatory before merge
- Automated CI/CD pipeline validation

### Deployment Strategy
- Blue-green deployment for zero downtime
- Kubernetes orchestration in production
- Automatic database migrations in development
- Manual approval for production migrations

## Monitoring & Observability

### Application Monitoring
- Application Insights for .NET services performance
- Prometheus + Grafana for custom metrics
- ELK Stack for centralized logging
- Real-time business metrics tracking

### Business Metrics
Key metrics to monitor:
- Active auctions and bidding activity
- Revenue and commission tracking
- User engagement and conversion rates
- Payment processing success rates
- Search performance and relevance

## Security Considerations

### Authentication & Authorization  
- JWT Bearer tokens from Keycloak
- Role-based access control (RBAC)
- Rate limiting per user and IP
- OWASP security best practices

### Data Protection
- Encryption at rest and in transit
- GDPR compliance with data export/deletion
- Input validation with FluentValidation
- SQL injection prevention via EF Core parameterization

## Performance Optimization

### Caching Strategy
- Redis for distributed caching
- In-memory caching for frequently accessed data
- CDN for static assets and images
- Database query optimization with proper indexing

### Real-time Performance
- SignalR connection pooling and scaling
- Efficient bid processing with concurrency handling
- Background services for auction lifecycle management
- Load testing validation for high-traffic scenarios