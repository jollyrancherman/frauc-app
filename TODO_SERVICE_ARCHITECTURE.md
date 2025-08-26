# Todo Service Architecture

## Overview

The Todo Service is implemented as a **separate bounded context** to ensure high performance and scalability for task management functionality. This service manages buyer and seller todo lists with urgency-based prioritization and real-time updates.

## Architectural Decisions

### Separate Bounded Context Rationale
1. **Performance Isolation**: Todo operations are high-frequency and need independent scaling
2. **Data Access Patterns**: Different from transactional listing/bidding operations
3. **Caching Strategy**: Heavy use of Redis for fast todo retrieval
4. **Event-Driven Updates**: Reactive to business events from other contexts
5. **User Experience**: Critical for user engagement and transaction completion

## Service Architecture

### Core Components

#### TodoService API
```csharp
public class TodoService
{
    private readonly ITodoRepository _todoRepository;
    private readonly ITodoCacheService _cacheService;
    private readonly IEventBus _eventBus;
    private readonly ITodoGeneratorService _generatorService;
    
    public async Task<IEnumerable<TodoItemDto>> GetUserTodosAsync(UserId userId)
    {
        // Try cache first
        var cachedTodos = await _cacheService.GetUserTodosAsync(userId);
        if (cachedTodos != null) return cachedTodos;
        
        // Fallback to database
        var todos = await _todoRepository.GetActiveByUserIdAsync(userId);
        await _cacheService.SetUserTodosAsync(userId, todos);
        return todos;
    }
    
    public async Task<TodoItemDto> CreateTodoAsync(CreateTodoCommand command)
    {
        var todo = new TodoItem(command);
        await _todoRepository.SaveAsync(todo);
        await _cacheService.InvalidateUserTodosAsync(command.UserId);
        
        // Publish event for real-time updates
        await _eventBus.PublishAsync(new TodoCreatedEvent(todo.Id, todo.UserId));
        return todo.ToDto();
    }
}
```

#### Redis Caching Strategy
```csharp
public class TodoCacheService : ITodoCacheService
{
    private readonly IRedisDatabase _redis;
    private const int CacheTtlHours = 24;
    
    public async Task<IEnumerable<TodoItemDto>> GetUserTodosAsync(UserId userId)
    {
        var key = $"user:todos:{userId}";
        var cached = await _redis.StringGetAsync(key);
        
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<IEnumerable<TodoItemDto>>(cached);
        }
        
        return null;
    }
    
    public async Task SetUserTodosAsync(UserId userId, IEnumerable<TodoItemDto> todos)
    {
        var key = $"user:todos:{userId}";
        var serialized = JsonSerializer.Serialize(todos);
        await _redis.StringSetAsync(key, serialized, TimeSpan.FromHours(CacheTtlHours));
    }
    
    public async Task InvalidateUserTodosAsync(UserId userId)
    {
        var key = $"user:todos:{userId}";
        await _redis.KeyDeleteAsync(key);
    }
}
```

### Event-Driven Todo Generation

#### Event Listeners
```csharp
public class TodoEventHandlers
{
    [EventHandler]
    public async Task Handle(ListingCreatedEvent @event)
    {
        // Create seller todos for new listing
        await _todoGenerator.CreateSellerListingTodosAsync(@event);
    }
    
    [EventHandler]
    public async Task Handle(BidPlacedEvent @event)
    {
        // Create seller notification todo
        await _todoGenerator.CreateBidNotificationTodoAsync(@event);
        
        // Create buyer payment todo if auction ends
        if (@event.IsWinningBid)
        {
            await _todoGenerator.CreatePaymentTodoAsync(@event);
        }
    }
    
    [EventHandler] 
    public async Task Handle(ListingExpiringEvent @event)
    {
        // Create urgent todo for seller
        await _todoGenerator.CreateExpirationWarningTodoAsync(@event);
    }
}
```

#### Auto-Todo Generation Service
```csharp
public class TodoGeneratorService : ITodoGeneratorService
{
    public async Task CreateSellerListingTodosAsync(ListingCreatedEvent @event)
    {
        var todos = new List<TodoItem>();
        
        // Add photos reminder (if no images)
        if (@event.ImageCount == 0)
        {
            todos.Add(new TodoItem(
                userId: @event.SellerId,
                type: TodoType.ADD_PHOTOS,
                urgency: UrgencyLevel.MEDIUM,
                title: "Add photos to your listing",
                description: "Listings with photos get 5x more views",
                relatedEntityId: @event.ListingId,
                dueDate: DateTime.UtcNow.AddDays(1)
            ));
        }
        
        // Answer questions reminder
        todos.Add(new TodoItem(
            userId: @event.SellerId,
            type: TodoType.MONITOR_QUESTIONS,
            urgency: UrgencyLevel.LOW,
            title: "Monitor buyer questions",
            description: "Respond quickly to increase trust score",
            relatedEntityId: @event.ListingId
        ));
        
        await _todoRepository.SaveAllAsync(todos);
        await InvalidateUserCacheAsync(@event.SellerId);
    }
}
```

## Urgency Level System

### Urgency Calculation Algorithm
```csharp
public class UrgencyCalculationService
{
    public UrgencyLevel CalculateUrgency(TodoType type, DateTime? dueDate, RelatedEntityContext context)
    {
        var hoursUntilDue = dueDate?.Subtract(DateTime.UtcNow).TotalHours;
        
        return type switch
        {
            TodoType.PAYMENT_DUE => hoursUntilDue <= 24 ? UrgencyLevel.URGENT : UrgencyLevel.HIGH,
            TodoType.PICKUP_REQUIRED => hoursUntilDue <= 48 ? UrgencyLevel.URGENT : UrgencyLevel.HIGH,
            TodoType.ANSWER_QUESTION => UrgencyLevel.HIGH,
            TodoType.LISTING_EXPIRING => hoursUntilDue <= 24 ? UrgencyLevel.URGENT : UrgencyLevel.MEDIUM,
            TodoType.ADD_PHOTOS => UrgencyLevel.MEDIUM,
            TodoType.UPDATE_PROFILE => UrgencyLevel.LOW,
            _ => UrgencyLevel.LOW
        };
    }
}
```

### Todo Types by Category

#### URGENT (Red - <24 hours)
- `PAYMENT_DUE`: Won auction payment required
- `PICKUP_REQUIRED`: Item pickup deadline approaching
- `DISPUTE_RESPONSE`: Respond to transaction dispute
- `IDENTITY_VERIFICATION_EXPIRING`: Verification status expiring

#### HIGH (Orange - <3 days)  
- `LISTING_EXPIRING`: Listing about to expire
- `BUYER_QUESTION_UNANSWERED`: Question waiting >4 hours
- `SHIPPING_CONFIRMATION`: Confirm shipping details
- `RATE_TRANSACTION`: Rate completed transaction

#### MEDIUM (Yellow - <1 week)
- `ADD_PHOTOS`: Improve listing with photos
- `COMPLETE_PROFILE`: Fill missing profile information  
- `MESSAGE_UNREAD`: New conversation messages
- `TRUST_SCORE_IMPROVEMENT`: Suggestions to boost score

#### LOW (Green - General improvements)
- `OPTIMIZE_LISTINGS`: Seasonal suggestions
- `EXPLORE_CATEGORIES`: Discover new item categories
- `SECURITY_CHECKUP`: Review account security
- `REFERRAL_OPPORTUNITY`: Invite friends for bonuses

## Platform-Specific Display Architecture

### Web Interface Components
```typescript
// React components for web dashboard
interface TodoDashboardProps {
  userId: string;
  displayMode: 'prominent' | 'sidebar' | 'modal';
}

export const TodoDashboard: React.FC<TodoDashboardProps> = ({ userId, displayMode }) => {
  const { todos, loading } = useTodos(userId);
  const urgentTodos = todos.filter(t => t.urgency === 'URGENT');
  
  if (displayMode === 'prominent') {
    return (
      <div className="todo-dashboard-prominent">
        <UrgentTodosAlert todos={urgentTodos} />
        <TodoList todos={todos} showBatchActions />
        <TodoFilters />
      </div>
    );
  }
  
  return <CompactTodoWidget todos={urgentTodos} />;
};
```

### Mobile Interface Components
```typescript
// React Native components for mobile dashboard
interface MobileTodoDashboardProps {
  userId: string;
}

export const MobileTodoDashboard: React.FC<MobileTodoDashboardProps> = ({ userId }) => {
  const { todos } = useTodos(userId);
  const stats = calculateTodoStats(todos);
  
  return (
    <ScrollView>
      <TodoStatsCards stats={stats} />
      <UrgentTodosList todos={todos.filter(t => t.urgency === 'URGENT')} />
      <QuickActions todos={todos} />
    </ScrollView>
  );
};

const TodoStatsCards = ({ stats }) => (
  <View className="flex-row">
    <StatsCard title="Urgent" count={stats.urgent} color="red" />
    <StatsCard title="High" count={stats.high} color="orange" />
    <StatsCard title="Medium" count={stats.medium} color="yellow" />
    <StatsCard title="Low" count={stats.low} color="green" />
  </View>
);
```

## Real-Time Updates with SignalR

### Todo Hub
```csharp
public class TodoHub : Hub
{
    public async Task JoinUserTodoGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-todos-{userId}");
    }
    
    public async Task LeaveUserTodoGroup(string userId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-todos-{userId}");
    }
}

public class TodoNotificationService
{
    private readonly IHubContext<TodoHub> _hubContext;
    
    public async Task NotifyTodoCreated(UserId userId, TodoItemDto todo)
    {
        await _hubContext.Clients.Group($"user-todos-{userId}")
            .SendAsync("TodoCreated", todo);
    }
    
    public async Task NotifyTodoCompleted(UserId userId, TodoId todoId)
    {
        await _hubContext.Clients.Group($"user-todos-{userId}")
            .SendAsync("TodoCompleted", todoId);
    }
    
    public async Task NotifyUrgencyChanged(UserId userId, TodoId todoId, UrgencyLevel newUrgency)
    {
        await _hubContext.Clients.Group($"user-todos-{userId}")
            .SendAsync("TodoUrgencyChanged", todoId, newUrgency);
    }
}
```

## Batch Operations & Auto-Actions

### Batch Operations
```csharp
public class TodoBatchService
{
    public async Task<BatchOperationResult> ExecuteBatchActionAsync(BatchTodoCommand command)
    {
        var results = new List<TodoOperationResult>();
        
        foreach (var todoId in command.TodoIds)
        {
            var result = command.Action switch
            {
                BatchAction.MarkCompleted => await MarkTodoCompletedAsync(todoId),
                BatchAction.Dismiss => await DismissTodoAsync(todoId),
                BatchAction.Reschedule => await RescheduleTodoAsync(todoId, command.NewDueDate),
                _ => throw new ArgumentException("Invalid batch action")
            };
            
            results.Add(result);
        }
        
        // Invalidate cache after batch operation
        await _cacheService.InvalidateUserTodosAsync(command.UserId);
        
        return new BatchOperationResult(results);
    }
}
```

### Smart Auto-Actions
```csharp
public class SmartTodoService
{
    public async Task ProcessAutoActionsAsync(UserId userId)
    {
        var todos = await GetUserTodosAsync(userId);
        var autoActionCandidates = todos.Where(CanAutoComplete);
        
        foreach (var todo in autoActionCandidates)
        {
            await ExecuteAutoActionAsync(todo);
        }
    }
    
    private bool CanAutoComplete(TodoItem todo)
    {
        return todo.Type switch
        {
            TodoType.ADD_PHOTOS => HasPhotosBeenAdded(todo.RelatedEntityId),
            TodoType.COMPLETE_PROFILE => IsProfileComplete(todo.UserId),
            TodoType.ANSWER_QUESTION => HasQuestionBeenAnswered(todo.RelatedEntityId),
            _ => false
        };
    }
}
```

## Database Schema

### Todo Tables
```sql
CREATE TABLE TodoItems (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,
    Type VARCHAR(100) NOT NULL,
    Urgency VARCHAR(20) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    DueDate TIMESTAMP,
    Status VARCHAR(20) DEFAULT 'PENDING',
    RelatedEntityType VARCHAR(50), -- 'Listing', 'Item', 'Transaction'
    RelatedEntityId UUID,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    CompletedAt TIMESTAMP,
    DismissedAt TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_todos_user_status ON TodoItems (UserId, Status);
CREATE INDEX idx_todos_urgency ON TodoItems (Urgency) WHERE Status = 'PENDING';
CREATE INDEX idx_todos_due_date ON TodoItems (DueDate) WHERE Status = 'PENDING';
CREATE INDEX idx_todos_type ON TodoItems (Type, Status);

-- Cleanup expired todos
CREATE INDEX idx_todos_expired ON TodoItems (CreatedAt) 
    WHERE Status IN ('COMPLETED', 'DISMISSED');
```

## Performance Considerations

### Caching Strategy
- **L1 Cache**: Redis for active user todos (24hr TTL)
- **L2 Cache**: In-memory cache for todo type definitions
- **Cache Invalidation**: Event-driven cache invalidation
- **Cache Warmup**: Pre-populate cache for active users

### Database Optimization
- **Partitioning**: Partition todos by user_id for large scale
- **Archiving**: Auto-archive completed todos older than 90 days
- **Read Replicas**: Separate read replicas for todo queries
- **Connection Pooling**: Optimize database connections

### Scaling Strategy
- **Horizontal Scaling**: Multiple TodoService instances
- **Load Balancing**: Round-robin with sticky sessions for SignalR
- **Redis Cluster**: Distributed caching for high availability
- **Event Sourcing**: Consider event sourcing for audit trail

## Monitoring & Metrics

### Key Performance Indicators
- **Todo Completion Rate**: Percentage of todos completed vs created
- **Response Time**: Average time to load user todos
- **Cache Hit Rate**: Redis cache effectiveness
- **Event Processing Latency**: Time from event to todo creation
- **User Engagement**: Active users completing todos

### Health Checks
```csharp
public class TodoServiceHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        // Check Redis connectivity
        var redisHealthy = await _redis.PingAsync();
        
        // Check database connectivity
        var dbHealthy = await _todoRepository.HealthCheckAsync();
        
        // Check event bus connectivity
        var eventBusHealthy = await _eventBus.HealthCheckAsync();
        
        if (redisHealthy && dbHealthy && eventBusHealthy)
            return HealthCheckResult.Healthy("TodoService is healthy");
            
        return HealthCheckResult.Unhealthy("TodoService has issues");
    }
}
```

This architecture ensures the Todo Service can handle high-frequency operations while providing a responsive user experience across web and mobile platforms.