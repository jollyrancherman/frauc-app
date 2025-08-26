# Week 2: Item & Listing Services Sprint Plan

## Sprint Overview
**Duration**: 5 days  
**Focus**: Item and listing management with 5 listing types, PostGIS spatial search  
**Branch**: `feature/week2-item-listing-services`  
**Coverage Target**: 100% test coverage for all new code  

## Daily Breakdown & Commit Strategy

### Day 1: Item & Category Domain (Monday)
**Goal**: Establish Item and Category aggregates with full test coverage

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write failing tests for Item aggregate
  - Item creation with required fields (title, description, condition)
  - Item image management
  - Item validation rules
  - Category assignment
- [ ] **GREEN**: Implement Item aggregate
  - Item entity with properties
  - ItemImage value objects
  - ItemCondition enumeration
  - Business rule enforcement
- [ ] **REFACTOR**: Clean up Item design
  - Extract value objects
  - Optimize validation logic

**Commit 1**: `feat: Add Item aggregate with image management`

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

### Day 2: Item Application Layer (Tuesday)
**Goal**: Implement CQRS pattern for Item operations

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write tests for Item commands
  - CreateItemCommand validation
  - UpdateItemCommand logic
  - AddItemImageCommand constraints
- [ ] **GREEN**: Implement command handlers
  - Create item with category assignment
  - Update item with validation
  - Image upload implementation
- [ ] **REFACTOR**: Extract common command logic

**Commit 3**: `feat: Add Item CQRS commands and handlers`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Item queries
  - GetItemById query
  - GetItemsBySeller query
  - GetItemsByCategory query
- [ ] **GREEN**: Implement query handlers
  - Item projections
  - Basic search implementation
  - Category filtering
- [ ] **REFACTOR**: Optimize query performance

**Commit 4**: `feat: Add Item queries with category filtering`

**End of Day Checklist**:
- [ ] Application layer builds successfully
- [ ] All handler tests passing
- [ ] 100% test coverage for application
- [ ] Push commits to remote

---

### Day 3: Item Infrastructure & Listing Domain (Wednesday)
**Goal**: Implement Item persistence and establish Listing domain with 5 types

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write integration tests for Item repository
  - CRUD operations
  - Image management
  - Category relationships
- [ ] **GREEN**: Implement Item repository
  - EF Core configurations
  - Repository pattern implementation
  - Database migrations
- [ ] **REFACTOR**: Optimize database queries

**Commit 5**: `feat: Add Item repository with EF Core`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Listing aggregate
  - Listing creation with 5 types (FREE, FREE_TO_AUCTION, AUCTION, REVERSE_AUCTION, FOR_SALE)
  - Location assignment (PostGIS)
  - State transitions
  - Bidding logic validation
- [ ] **GREEN**: Implement Listing aggregate
  - Listing entity with ListingType enumeration
  - Location value object for PostGIS
  - Business rules for each listing type
- [ ] **REFACTOR**: Clean up Listing design

**Commit 6**: `feat: Add Listing aggregate with 5 types and PostGIS location`

**End of Day Checklist**:
- [ ] Infrastructure layer builds successfully
- [ ] Integration tests passing with TestContainers
- [ ] 100% test coverage for infrastructure
- [ ] Push commits to remote

---

### Day 4: Listing Application & Infrastructure (Thursday)
**Goal**: Implement CQRS for Listing operations and PostGIS integration

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write tests for Listing CQRS
  - CreateListingCommand with Item reference
  - PlaceBidCommand for auction types
  - ConvertToAuctionCommand for FREE_TO_AUCTION
  - UpdateListingLocationCommand
- [ ] **GREEN**: Implement CQRS operations
  - Command handlers for all 5 listing types
  - Query handlers for listing search
  - Event publishing for state changes
- [ ] **REFACTOR**: Optimize CQRS implementation

**Commit 7**: `feat: Add Listing CQRS with 5 listing type support`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for PostGIS integration
  - Spatial queries for listing location
  - Radius-based search
  - Distance calculations
  - Listing repository with spatial methods
- [ ] **GREEN**: Implement PostGIS service
  - Listing repository with spatial queries
  - LocationSearchService for buyer radius search
  - PostGIS database configuration
- [ ] **REFACTOR**: Optimize spatial queries

**Commit 8**: `feat: Add PostGIS spatial queries for location-based search`

**End of Day Checklist**:
- [ ] Listing domain/application builds successfully
- [ ] All tests passing
- [ ] 100% test coverage
- [ ] Push commits to remote

---

### Day 5: API Layer & Todo Service Integration (Friday)
**Goal**: Expose APIs and integrate Todo Service for user task management

#### Morning Session (9 AM - 12 PM)
- [ ] **RED**: Write API tests
  - Items controller endpoints
  - Listings controller endpoints with spatial search
  - Authentication/authorization
  - Input validation for 5 listing types
- [ ] **GREEN**: Implement API controllers
  - Items API with Swagger docs
  - Listings API with location-based search
  - Error handling
  - Response formatting

**Commit 9**: `feat: Add Items API controller with image upload`
**Commit 10**: `feat: Add Listings API controller with PostGIS search`

#### Afternoon Session (1 PM - 5 PM)
- [ ] **RED**: Write tests for Todo Service integration
  - Todo creation from listing events
  - Urgency level calculation
  - User todo retrieval
- [ ] **GREEN**: Implement Todo Service
  - TodoItem aggregate in separate bounded context
  - Event handlers for listing lifecycle
  - Redis caching for fast todo access
- [ ] **REFACTOR**: Optimize todo generation

**Commit 11**: `feat: Add Todo Service with Redis caching and event-driven updates`

**End of Day Checklist**:
- [ ] All APIs documented in Swagger
- [ ] Integration tests passing
- [ ] 100% test coverage maintained
- [ ] Push all commits to remote
- [ ] Create PR for review

---

## Testing Strategy

### Unit Tests
- Item & Listing domain models: 100% coverage
- CQRS handlers: 100% coverage
- Value objects (ItemImage, Location, ListingType): 100% coverage

### Integration Tests
- Repository operations with TestContainers
- PostGIS spatial queries with test data
- Todo Service with Redis test container

### API Tests
- Item & Listing controller endpoints with WebApplicationFactory
- Location-based search API testing
- 5 listing type creation flows
- Authentication/authorization flows

### Performance Tests
- PostGIS spatial query performance
- Concurrent listing creation with different types
- Todo Service Redis cache performance

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
1. `feat: Add Item aggregate with image management`
2. `feat: Add Category aggregate with hierarchical structure`
3. `feat: Add Item CQRS commands and handlers`
4. `feat: Add Item queries with category filtering`
5. `feat: Add Item repository with EF Core`
6. `feat: Add Listing aggregate with 5 types and PostGIS location`
7. `feat: Add Listing CQRS with 5 listing type support`
8. `feat: Add PostGIS spatial queries for location-based search`
9. `feat: Add Items API controller with image upload`
10. `feat: Add Listings API controller with PostGIS search`
11. `feat: Add Todo Service with Redis caching and event-driven updates`

Total: ~11 focused commits following TDD methodology

## Notes

- Each commit must build and pass tests
- Push at end of each day minimum
- Use feature branch throughout week
- Create PR on Friday for review
- Merge to main after approval