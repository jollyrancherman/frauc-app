# Business Requirements

## Application Overview

This is a Facebook Marketplace-style auction platform that supports multiple listing types with integrated trust scoring and advanced todo management for users.

## Listing Types

The platform supports 5 distinct listing types with specific business rules:

### 1. FREE Listings
- **Purpose**: Allow users to give away items at no cost
- **Duration**: 30-day automatic expiration
- **Geographic Scope**: Location-based with buyer-selected search radius
- **Business Rules**:
  - No payment processing required
  - First-come, first-served basis
  - Automatic expiration after 30 days
  - Location verification required for pickup coordination

### 2. FREE-to-AUCTION Listings
- **Purpose**: Start as free listing, convert to auction when interest shown
- **Trigger**: Converts to auction when first bid is placed
- **Business Rules**:
  - Initially listed as FREE
  - Seller can set minimum bid amount
  - Once first bid placed, becomes standard auction
  - Original "free" offer becomes invalid
  - Standard auction rules apply after conversion

### 3. AUCTION Listings (Forward Auction)
- **Purpose**: Traditional auction where highest bidder wins
- **Duration**: Seller-defined end time
- **Business Rules**:
  - Minimum bid increments enforced
  - Highest bidder wins at auction end
  - Real-time bid updates via SignalR
  - Automatic winner notification
  - Payment processing required

### 4. REVERSE AUCTION Listings
- **Purpose**: Buyer posts what they want, sellers compete with lowest prices
- **Duration**: Buyer-defined acceptance period
- **Business Rules**:
  - Buyer specifies item/service requirements
  - Sellers submit competitive bids (lowest wins)
  - Buyer can accept any bid before expiration
  - Early acceptance ends auction
  - Payment processing required

### 5. FOR SALE Listings (Fixed Price)
- **Purpose**: Traditional fixed-price sales
- **Duration**: Until sold or manually removed
- **Business Rules**:
  - Fixed price set by seller
  - Immediate purchase available
  - "Buy It Now" functionality
  - Payment processing required
  - Inventory tracking (quantity available)

## Core Domain Model

### Item vs Listing Relationship
- **1:1 Relationship**: Each Item has exactly one Listing
- **Not a Product Catalog**: Items are individual, unique listings
- **Item**: Physical or digital good being offered
- **Listing**: The marketplace entry with pricing, duration, and business rules

### Categories
- **Predefined Categories**: System-managed category hierarchy
- **Filterable**: Users can filter listings by category
- **Hierarchical Structure**: Parent-child category relationships
- **Examples**: Electronics > Phones > Smartphones

## Trust Score System

### Scoring Framework
- **Scale**: 100-point system (100 = perfect trust)
- **Starting Score**: New users begin at 50 points
- **Real-time Updates**: Scores update immediately after transactions

### Score Calculation Rules
**Positive Actions** (increase score):
- Successful transaction completion: +2 points
- Positive buyer/seller rating: +1 point
- Identity verification: +5 points
- Long-term account activity: +1 point per month (max +12/year)

**Negative Actions** (decrease score):
- Transaction dispute: -5 points
- Negative rating: -2 points
- Cancelled transaction: -1 point
- Policy violation: -10 points
- Fraudulent activity: -50 points (major penalty)

### Trust Score Impact
- **Display**: Visible on all user profiles and listings
- **Business Impact**: Higher trust scores get better search ranking
- **Restrictions**: Users below 20 points have limited functionality
- **Recovery**: Scores can be rebuilt through consistent positive behavior

## Todo System

### Purpose
Provide buyers and sellers with task management based on urgency levels to improve transaction completion rates.

### Todo Categories by Urgency

#### URGENT (Red - Action Required Within 24 Hours)
- Payment due for won auction
- Pickup/shipping deadline approaching
- Dispute response required
- Identity verification expiring

#### HIGH (Orange - Action Required Within 3 Days)
- Listing about to expire
- Buyer questions unanswered
- Shipping confirmation needed
- Rating/review pending

#### MEDIUM (Yellow - Action Required Within 1 Week)
- Profile information incomplete
- New messages in conversations
- Seasonal listing optimization suggestions
- Trust score improvement recommendations

#### LOW (Green - General Improvements)
- Add more photos to listings
- Update listing descriptions
- Explore new categories
- Account security recommendations

### Platform-Specific Display

#### Web Interface
- **Prominent Display**: Dashboard widget with urgent items highlighted
- **Detailed View**: Full todo list with filtering and sorting
- **Batch Actions**: Select multiple todos for bulk operations
- **Progress Tracking**: Visual indicators for completion rates

#### Mobile Interface
- **Dashboard Stats**: Summary cards showing todo counts by urgency
- **Quick Actions**: Swipe-to-complete functionality
- **Push Notifications**: Urgent todo alerts
- **Simplified View**: Condensed list prioritizing urgent items

### Auto-Actions
- **Smart Suggestions**: AI-powered recommendations based on user behavior
- **Automated Reminders**: Email/SMS alerts for time-sensitive todos
- **Bulk Operations**: Group similar actions (e.g., "Answer 3 buyer questions")
- **Integration**: Connect with calendar for pickup scheduling

## Geographic Features

### Location-Based Search
- **Buyer-Controlled**: Buyers set search radius preferences
- **PostGIS Integration**: Efficient spatial queries for location filtering
- **Distance Calculation**: Real-time distance calculations for pickup coordination
- **Privacy**: Approximate locations shown until transaction commitment

## Communication System

### Message Types

#### Direct Messaging
- **One-on-One**: Private conversations between users
- **Persistent**: Message history maintained
- **Real-time**: SignalR integration for instant delivery
- **Moderation**: Automated content filtering

#### Listing-Scoped Messaging
- **Public Q&A**: Questions visible to all potential buyers
- **Seller Responses**: Public responses to common questions
- **Transparency**: Builds trust through open communication
- **Moderation**: Community reporting and admin oversight

## Notification System

### Notification Channels
- **Email**: Transaction confirmations, auction updates, important alerts
- **Push Notifications**: Mobile app real-time updates
- **SMS**: Critical alerts for high-value transactions (opt-in)
- **In-App**: Dashboard notifications for todos and messages

### Notification Categories
- **Transactional**: Payment confirmations, shipping updates
- **Auction**: Bid updates, auction ending alerts, winner notifications
- **Social**: New messages, ratings received, trust score changes
- **System**: Todo reminders, policy updates, maintenance alerts

## Technical Architecture Requirements

### TodoService as Separate Bounded Context
- **Performance**: Isolated service for high-frequency todo operations
- **Redis Caching**: Fast access to user todo lists
- **Event-Driven**: Updates triggered by business events from other services
- **Scalability**: Independent scaling based on todo management load

### Integration Points
- **User Service**: Trust score integration and user profile data
- **Listing Service**: Todo creation based on listing lifecycle events
- **Payment Service**: Transaction-related todo generation
- **Notification Service**: Todo reminder and alert delivery

## Business Rules Summary

### Listing Lifecycle
1. Item created with one of 5 listing types
2. Business rules applied based on listing type
3. Geographic filtering enables local discovery
4. Trust scores influence search ranking and buyer confidence
5. Communication facilitates transaction coordination
6. Todos guide users through transaction completion
7. Notifications keep users engaged throughout process

### Success Metrics
- **Transaction Completion Rate**: Measure of successful buyer-seller connections
- **Trust Score Distribution**: Health of user trust ecosystem
- **Todo Completion Rate**: Effectiveness of task management system
- **Geographic Coverage**: Success of location-based features
- **User Engagement**: Message response rates and notification engagement