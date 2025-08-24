# Week 2: Product & Listing Services Sprint Plan

## Sprint Overview
**Duration**: 5 days  
**Focus**: Product catalog and listing management with search capabilities  
**Branch**: `feature/week2-product-listing-services`  
**Coverage Target**: 100% test coverage for all new code  

## Daily Breakdown & Commit Strategy

### Day 1: Product Domain (Monday)
**Goal**: Establish Product and Category aggregates with full test coverage

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write failing tests for Product aggregate
  - Product creation with required fields
  - Specification pattern for product attributes
  - Product validation rules
  - Product state transitions
- [ ] **GREEN**: Implement Product aggregate
  - Product entity with properties
  - Specification value objects
  - Business rule enforcement
- [ ] **REFACTOR**: Clean up Product design
  - Extract value objects
  - Optimize validation logic

**Commit 1**: `feat: Add Product aggregate with specifications pattern`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write failing tests for Category aggregate
  - Category hierarchy management
  - Parent-child relationships
  - Category path generation
  - Circular reference prevention
- [ ] **GREEN**: Implement Category aggregate
  - Category entity with hierarchy
  - Path management logic
  - Validation rules
- [ ] **REFACTOR**: Optimize category structure

**Commit 2**: `feat: Add Category aggregate with hierarchical structure`

**End of Day Checklist**:
- [ ] Domain layer builds successfully
- [ ] All domain tests passing
- [ ] 100% test coverage for domain
- [ ] Push commits to remote

---

### Day 2: Product Application Layer (Tuesday)
**Goal**: Implement CQRS pattern for Product operations

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write tests for Product commands
  - CreateProductCommand validation
  - UpdateProductCommand logic
  - DeleteProductCommand constraints
- [ ] **GREEN**: Implement command handlers
  - Create product with category assignment
  - Update product with validation
  - Soft delete implementation
- [ ] **REFACTOR**: Extract common command logic

**Commit 3**: `feat: Add Product CQRS commands and handlers`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Product queries
  - GetProductById query
  - SearchProducts with filtering
  - GetProductsByCategory
- [ ] **GREEN**: Implement query handlers
  - Product projections
  - Search implementation
  - Category filtering
- [ ] **REFACTOR**: Optimize query performance

**Commit 4**: `feat: Add Product search queries with filtering`

**End of Day Checklist**:
- [ ] Application layer builds successfully
- [ ] All handler tests passing
- [ ] 100% test coverage for application
- [ ] Push commits to remote

---

### Day 3: Product Infrastructure (Wednesday)
**Goal**: Implement data persistence and search infrastructure

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write integration tests for repository
  - CRUD operations
  - Concurrency handling
  - Transaction support
- [ ] **GREEN**: Implement Product repository
  - EF Core configurations
  - Repository pattern implementation
  - Database migrations
- [ ] **REFACTOR**: Optimize database queries

**Commit 5**: `feat: Add Product repository with EF Core`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Elasticsearch integration
  - Product indexing
  - Search functionality
  - Index synchronization
- [ ] **GREEN**: Implement Elasticsearch service
  - Product document mapping
  - Index management
  - Search service implementation
- [ ] **REFACTOR**: Optimize search performance

**Commit 6**: `feat: Add Elasticsearch product indexing`

**End of Day Checklist**:
- [ ] Infrastructure layer builds successfully
- [ ] Integration tests passing with TestContainers
- [ ] 100% test coverage for infrastructure
- [ ] Push commits to remote

---

### Day 4: Listing Domain & Application (Thursday)
**Goal**: Implement Listing aggregate with auction functionality

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write tests for Listing aggregate
  - Listing creation with product reference
  - Auction type handling (forward/reverse/fixed)
  - Listing lifecycle (draft/active/expired/sold)
  - Price validation rules
- [ ] **GREEN**: Implement Listing aggregate
  - Listing entity with states
  - Auction type value objects
  - Business rules enforcement
- [ ] **REFACTOR**: Clean up listing design

**Commit 7**: `feat: Add Listing aggregate with auction types`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Listing CQRS
  - CreateListing command
  - UpdateListing command
  - Listing state transitions
  - Listing queries
- [ ] **GREEN**: Implement CQRS operations
  - Command handlers
  - Query handlers
  - Event publishing
- [ ] **REFACTOR**: Optimize CQRS implementation

**Commit 8**: `feat: Add Listing CQRS operations`

**End of Day Checklist**:
- [ ] Listing domain/application builds successfully
- [ ] All tests passing
- [ ] 100% test coverage
- [ ] Push commits to remote

---

### Day 5: API Layer & Integration (Friday)
**Goal**: Expose APIs and integrate geospatial features

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write API tests
  - Products controller endpoints
  - Listings controller endpoints
  - Authentication/authorization
  - Input validation
- [ ] **GREEN**: Implement API controllers
  - Products API with Swagger docs
  - Listings API with Swagger docs
  - Error handling
  - Response formatting

**Commit 9**: `feat: Add Products API controller`
**Commit 10**: `feat: Add Listings API controller`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for geospatial features
  - Location-based search
  - Distance calculations
  - Boundary queries
- [ ] **GREEN**: Implement PostGIS integration
  - Spatial queries
  - Location indexing
  - Distance algorithms
- [ ] **REFACTOR**: Optimize spatial queries

**Commit 11**: `feat: Add PostGIS geospatial search`

**End of Day Checklist**:
- [ ] All APIs documented in Swagger
- [ ] Integration tests passing
- [ ] 100% test coverage maintained
- [ ] Push all commits to remote
- [ ] Create PR for review

---

## Testing Strategy

### Unit Tests
- Domain models: 100% coverage
- Application handlers: 100% coverage
- Value objects: 100% coverage

### Integration Tests
- Repository operations with TestContainers
- Elasticsearch indexing with test cluster
- PostGIS queries with spatial test data

### API Tests
- Controller endpoints with WebApplicationFactory
- Authentication/authorization flows
- Error handling scenarios

### Performance Tests
- Load testing for search operations
- Concurrent listing creation
- Spatial query performance

## Definition of Done

- [ ] All code follows DDD principles
- [ ] 100% test coverage achieved
- [ ] All tests passing (unit, integration, API)
- [ ] Code reviewed via PR
- [ ] Documentation updated
- [ ] No outstanding TODOs
- [ ] Performance benchmarks met
- [ ] Security scan passed

## Git Commit Summary

Expected commits for Week 2:
1. `feat: Add Product aggregate with specifications pattern`
2. `feat: Add Category aggregate with hierarchical structure`
3. `feat: Add Product CQRS commands and handlers`
4. `feat: Add Product search queries with filtering`
5. `feat: Add Product repository with EF Core`
6. `feat: Add Elasticsearch product indexing`
7. `feat: Add Listing aggregate with auction types`
8. `feat: Add Listing CQRS operations`
9. `feat: Add Products API controller`
10. `feat: Add Listings API controller`
11. `feat: Add PostGIS geospatial search`

Total: ~11 focused commits following TDD methodology

## Notes

- Each commit must build and pass tests
- Push at end of each day minimum
- Use feature branch throughout week
- Create PR on Friday for review
- Merge to main after approval