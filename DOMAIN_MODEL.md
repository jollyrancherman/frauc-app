# Domain Model Architecture

## Overview

This document defines the domain model for our Facebook Marketplace-style auction platform, focusing on Domain-Driven Design (DDD) principles with clear bounded contexts and aggregate relationships.

## Core Domain Concepts

### Item vs Listing Relationship
- **Item**: The physical or digital good being offered (no location data)
- **Listing**: The marketplace entry containing business rules, pricing, duration, and **location for PostGIS queries**
- **Relationship**: 1:1 (Each Item has exactly one Listing, no catalog concept)
- **Lifecycle**: Item and Listing are created together and share the same lifecycle

## Bounded Contexts

### 1. Item & Listing Context
**Responsibility**: Core marketplace functionality for items and their associated listings

#### Aggregates

##### Item Aggregate Root
```csharp
public class Item : Entity<ItemId>
{
    // Properties
    public ItemId Id { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public UserId SellerId { get; private set; }
    public List<ItemImage> Images { get; private set; }
    public ItemCondition Condition { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Business Methods
    public void UpdateDetails(string title, string description)
    public void AddImage(ItemImage image)
    public void RemoveImage(ImageId imageId)
    public void ChangeCondition(ItemCondition condition)
}
```

**Value Objects**:
- `ItemId`: Unique identifier
- `ItemImage`: Photo with metadata
- `ItemCondition`: New, Like New, Good, Fair, Poor

##### Listing Aggregate Root
```csharp
public class Listing : Entity<ListingId>
{
    // Properties
    public ListingId Id { get; }
    public ItemId ItemId { get; private set; }
    public ListingType Type { get; private set; }
    public ListingStatus Status { get; private set; }
    public Money Price { get; private set; }
    public Location Location { get; private set; } // PostGIS spatial data for search
    public DateTime? EndTime { get; private set; }
    public List<Bid> Bids { get; private set; }
    
    // Business Methods
    public void UpdateLocation(Location location)
    public void PlaceBid(UserId bidderId, Money amount)
    public void ConvertToAuction(Money minimumBid) // For FREE-to-AUCTION
    public void AcceptBid(BidId bidId) // For REVERSE AUCTION
    public void CompleteSale(UserId buyerId)
    public void ExpireListing()
    public bool CanBid(UserId userId, Money amount)
    public bool IsWithinRadius(Location buyerLocation, int radiusKm)
}
```

**Value Objects**:
- `ListingId`: Unique identifier
- `ListingType`: FREE, FREE_TO_AUCTION, AUCTION, REVERSE_AUCTION, FOR_SALE
- `ListingStatus`: DRAFT, ACTIVE, EXPIRED, SOLD, CANCELLED
- `Money`: Amount with currency
- `Location`: Geographic coordinates for PostGIS spatial queries
- `Bid`: Amount, bidder, timestamp

#### Domain Services
- `ListingService`: Handles complex listing state transitions
- `BiddingService`: Manages bidding logic and validation
- `PriceCalculationService`: Handles pricing rules and fee calculations
- `LocationSearchService`: PostGIS spatial queries for radius-based search

### 2. Category Context
**Responsibility**: Category hierarchy and classification

#### Aggregates

##### Category Aggregate Root
```csharp
public class Category : Entity<CategoryId>
{
    public CategoryId Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public CategoryId? ParentCategoryId { get; private set; }
    public List<CategoryId> SubCategories { get; private set; }
    public bool IsActive { get; private set; }
    public string CategoryPath { get; private set; } // e.g., "Electronics/Phones/Smartphones"
    
    public void AddSubCategory(CategoryId subCategoryId)
    public void RemoveSubCategory(CategoryId subCategoryId)
    public void UpdatePath()
    public bool HasCircularReference(CategoryId parentId)
}
```

**Value Objects**:
- `CategoryId`: Unique identifier
- `CategoryPath`: Hierarchical path string

### 3. User Context
**Responsibility**: User profiles, authentication, and trust management

#### Aggregates

##### User Aggregate Root
```csharp
public class User : Entity<UserId>
{
    public UserId Id { get; }
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public UserProfile Profile { get; private set; }
    public TrustScore TrustScore { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastActiveAt { get; private set; }
    
    public void UpdateProfile(UserProfile profile)
    public void UpdateTrustScore(TrustScoreAction action)
    public void DeactivateUser()
    public void RecordActivity()
    public bool CanCreateListing()
    public bool CanPlaceBid()
}
```

**Value Objects**:
- `UserId`: Unique identifier linked to Keycloak
- `Username`: Unique username with validation
- `Email`: Email with validation
- `UserProfile`: Personal information (name, location, bio)
- `TrustScore`: Score (0-100) with history
- `UserStatus`: ACTIVE, SUSPENDED, DEACTIVATED

#### Domain Events
- `UserCreated`: New user registration
- `TrustScoreUpdated`: Score change events
- `UserProfileUpdated`: Profile modification events

### 4. Todo Context (Separate Bounded Context)
**Responsibility**: Task management for buyers and sellers

#### Aggregates

##### TodoItem Aggregate Root
```csharp
public class TodoItem : Entity<TodoId>
{
    public TodoId Id { get; }
    public UserId UserId { get; private set; }
    public TodoType Type { get; private set; }
    public UrgencyLevel Urgency { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public TodoStatus Status { get; private set; }
    public RelatedEntityId? RelatedEntityId { get; private set; } // ListingId, ItemId, etc.
    
    public void MarkCompleted()
    public void UpdateUrgency(UrgencyLevel urgency)
    public void ExtendDueDate(DateTime newDueDate)
    public bool IsOverdue()
    public void Dismiss()
}
```

**Value Objects**:
- `TodoId`: Unique identifier
- `TodoType`: PAYMENT_DUE, PICKUP_REQUIRED, ANSWER_QUESTION, etc.
- `UrgencyLevel`: URGENT, HIGH, MEDIUM, LOW
- `TodoStatus`: PENDING, COMPLETED, DISMISSED, EXPIRED
- `RelatedEntityId`: Reference to related domain entity

#### Domain Services
- `TodoGeneratorService`: Creates todos based on business events
- `UrgencyCalculationService`: Determines urgency based on context
- `TodoCleanupService`: Archives completed/expired todos

## Cross-Context Integration

### Event-Driven Architecture

#### Domain Events
```csharp
// From Listing Context
public class ListingCreated : DomainEvent
{
    public ListingId ListingId { get; }
    public UserId SellerId { get; }
    public ListingType Type { get; }
    public Location Location { get; }
    public DateTime EndTime { get; }
}

public class BidPlaced : DomainEvent
{
    public ListingId ListingId { get; }
    public UserId BidderId { get; }
    public Money Amount { get; }
    public DateTime BidTime { get; }
}

public class ListingExpired : DomainEvent
{
    public ListingId ListingId { get; }
    public UserId SellerId { get; }
    public bool HasWinner { get; }
    public UserId? WinnerId { get; }
}
```

#### Event Handlers
- **TodoService** listens to listing events to generate relevant todos
- **NotificationService** listens to all events for alert generation
- **TrustScoreService** listens to transaction events for score updates

### Repository Patterns

#### Item & Listing Repositories
```csharp
public interface IItemRepository
{
    Task<Item> GetByIdAsync(ItemId id);
    Task<IEnumerable<Item>> GetBySellerIdAsync(UserId sellerId);
    Task SaveAsync(Item item);
}

public interface IListingRepository
{
    Task<Listing> GetByIdAsync(ListingId id);
    Task<IEnumerable<Listing>> GetActiveListingsByCategoryAsync(CategoryId categoryId);
    Task<IEnumerable<Listing>> GetExpiringListingsAsync(DateTime threshold);
    Task<IEnumerable<Listing>> SearchByLocationAsync(Location centerPoint, int radiusKm);
    Task<IEnumerable<Listing>> GetListingsWithinBoundsAsync(BoundingBox bounds);
    Task SaveAsync(Listing listing);
}
```

#### Category Repository
```csharp
public interface ICategoryRepository
{
    Task<Category> GetByIdAsync(CategoryId id);
    Task<IEnumerable<Category>> GetRootCategoriesAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(CategoryId parentId);
    Task<string> GetCategoryPathAsync(CategoryId id);
}
```

## CQRS Pattern Implementation

### Commands
- `CreateItemWithListingCommand`: Create new item with associated listing
- `PlaceBidCommand`: Place bid on auction listing
- `ConvertToAuctionCommand`: Convert FREE-to-AUCTION listing
- `CompleteSaleCommand`: Mark listing as sold
- `UpdateListingLocationCommand`: Update listing location for spatial searches
- `UpdateTrustScoreCommand`: Modify user trust score

### Queries
- `GetItemByIdQuery`: Retrieve item details
- `SearchListingsQuery`: Search with filters (category, location radius, price)
- `GetListingsNearLocationQuery`: PostGIS spatial query for nearby listings
- `GetUserTodosQuery`: Retrieve user's todo list
- `GetCategoryHierarchyQuery`: Get category tree structure

### Query Projections
- `ListingSearchProjection`: Optimized for search operations with spatial data
- `UserDashboardProjection`: User activity and statistics
- `CategoryTreeProjection`: Hierarchical category display
- `LocationBasedListingProjection`: Optimized for PostGIS spatial queries

## Database Design with PostGIS

### Item & Listing Tables
```sql
-- Items table (no location data)
CREATE TABLE Items (
    Id UUID PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    CategoryId UUID REFERENCES Categories(Id),
    SellerId UUID NOT NULL,
    Condition VARCHAR(50),
    CreatedAt TIMESTAMP DEFAULT NOW()
);

-- Listings table (with PostGIS location for spatial queries)
CREATE TABLE Listings (
    Id UUID PRIMARY KEY,
    ItemId UUID REFERENCES Items(Id),
    Type VARCHAR(50) NOT NULL,
    Status VARCHAR(50) DEFAULT 'ACTIVE',
    PriceAmount DECIMAL(10,2),
    PriceCurrency VARCHAR(3) DEFAULT 'USD',
    Location GEOGRAPHY(POINT, 4326), -- PostGIS spatial column
    EndTime TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW()
);

-- PostGIS spatial index for efficient location-based queries
CREATE INDEX idx_listings_location ON Listings USING GIST (Location);

-- Index for active listings within radius queries
CREATE INDEX idx_listings_active_status ON Listings (Status) WHERE Status = 'ACTIVE';

-- Composite index for category and location searches
CREATE INDEX idx_listings_category_location ON Listings USING GIST (
    Location
) WHERE Status = 'ACTIVE';
```

### PostGIS Spatial Queries
```sql
-- Find listings within radius (buyer-selected search area)
SELECT l.*, i.Title, i.Description
FROM Listings l
JOIN Items i ON l.ItemId = i.Id
WHERE l.Status = 'ACTIVE'
AND ST_DWithin(
    l.Location,
    ST_GeogFromText('POINT(' || $longitude || ' ' || $latitude || ')'),
    $radiusInMeters
);

-- Find listings within bounding box
SELECT l.*, i.Title, i.Description  
FROM Listings l
JOIN Items i ON l.ItemId = i.Id
WHERE l.Status = 'ACTIVE'
AND ST_Intersects(
    l.Location,
    ST_MakeEnvelope($minLng, $minLat, $maxLng, $maxLat, 4326)
);
```

### Performance Optimizations
- **PostGIS Spatial Indexing**: GIST indexes for efficient radius queries
- **Category Hierarchies**: Materialized path for fast category queries  
- **Read Replicas**: Separate read models for search operations
- **Redis Caching**: Cache frequently accessed categories and user preferences
- **Query Optimization**: Composite indexes for common search patterns

## Business Rules Enforcement

### Listing Type Rules
1. **FREE Listings**: No payment processing, 30-day expiration, location-based discovery
2. **FREE-to-AUCTION**: State transition on first bid placement, location inherited
3. **AUCTION**: Real-time bidding with increment validation, geographic visibility
4. **REVERSE AUCTION**: Seller competition with buyer acceptance, location-based matching
5. **FOR SALE**: Immediate purchase with inventory tracking, spatial search integration

### Location-Based Business Rules
- **Search Radius**: Buyers control search radius (1km to 100km)
- **Pickup Coordination**: Location displayed after transaction commitment
- **Privacy Protection**: Approximate location shown until trust established
- **Distance Calculations**: Real-time distance calculations for logistics

### Trust Score Rules
- Score updates trigger recalculation of user privileges
- Low scores restrict listing creation and bidding
- Score history maintained for audit purposes
- Location-based trust factors (local reputation)

### Todo Generation Rules
- Automatic creation based on listing lifecycle events
- Location-aware todos (pickup reminders, travel distance)
- Urgency calculation includes geographic factors
- Cleanup of completed/expired todos

This domain model provides a solid foundation for implementing the marketplace platform while maintaining clear boundaries, enforcing business rules, and enabling efficient PostGIS spatial queries for location-based discovery.