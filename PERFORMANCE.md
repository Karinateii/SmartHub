# SmartHub Performance Guide

Performance optimization strategies for SmartHub.

## Database Optimization

### Query Optimization

**N+1 Problem - DON'T DO:**
```csharp
var users = db.Users.ToList();
foreach (var user in users)
{
    var roles = db.Roles.Where(r => r.UserId == user.Id).ToList();
}
```

**Solution - Use Include:**
```csharp
var users = db.Users
    .Include(u => u.Roles)
    .ToList();
```

**Solution - Use Select Projection:**
```csharp
var userRoles = db.Users
    .Where(u => u.IsActive)
    .Select(u => new {
        u.Id,
        u.Email,
        RoleCount = u.Roles.Count
    })
    .ToList();
```

### Indexing Strategy

**Essential Indexes:**

```sql
-- Search and filter columns
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_resources_created_at ON resources(created_at DESC);
CREATE INDEX idx_resources_created_by ON resources(created_by);

-- Foreign key columns
CREATE INDEX idx_user_roles_user_id ON user_roles(user_id);
CREATE INDEX idx_user_roles_role_id ON user_roles(role_id);
CREATE INDEX idx_api_keys_user_id ON api_keys(user_id);
CREATE INDEX idx_sessions_user_id ON sessions(user_id);

-- Composite indexes
CREATE INDEX idx_resources_created_by_created_at 
ON resources(created_by, created_at DESC);

-- Soft delete optimization
CREATE INDEX idx_users_deleted_at 
ON users(deleted_at) WHERE deleted_at IS NULL;
```

**Monitor Index Usage:**
```sql
SELECT schemaname, tablename, indexname, idx_scan
FROM pg_stat_user_indexes
ORDER BY idx_scan DESC;
```

### Connection Pooling

**Configuration:**
```json
{
  "Database": {
    "MinPoolSize": 5,
    "MaxPoolSize": 20,
    "CommandTimeout": 30,
    "ConnectionTimeout": 15
  }
}
```

**Monitor Connections:**
```sql
SELECT datname, count(*) as connections
FROM pg_stat_activity
GROUP BY datname;
```

### Pagination

**Always use pagination:**

```csharp
public async Task<PagedResult<Resource>> GetResources(int page = 1, int pageSize = 20)
{
    const int maxPageSize = 100;
    pageSize = Math.Min(pageSize, maxPageSize);
    
    var totalCount = await db.Resources.CountAsync();
    
    var items = await db.Resources
        .OrderByDescending(r => r.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Resource>
    {
        Items = items,
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize
    };
}
```

### Batch Operations

**Bulk Insert (1000+ records):**
```csharp
public async Task BulkInsertResources(List<Resource> resources)
{
    using (var bulk = db.Database.GetDbConnection() as NpgsqlConnection)
    {
        bulk.Open();
        using (var writer = bulk.BeginBinaryImport(
            "COPY resources (id, name, type, created_at) FROM STDIN (FORMAT BINARY)"))
        {
            foreach (var resource in resources)
            {
                writer.WriteRow(resource.Id, resource.Name, resource.Type, resource.CreatedAt);
            }
            await writer.CompleteAsync();
        }
    }
}
```

## Caching Strategy

### Redis Configuration

```json
{
  "Cache": {
    "Type": "Redis",
    "ConnectionString": "localhost:6379",
    "Database": 0,
    "DefaultTTL": 1800,
    "SlidingExpiration": true
  }
}
```

### Cache Patterns

**Cache-Aside:**
```csharp
public async Task<User> GetUser(Guid userId)
{
    string cacheKey = $"user:{userId}";
    
    var cached = await _cache.GetAsync(cacheKey);
    if (cached != null)
        return JsonSerializer.Deserialize<User>(cached);
    
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    
    if (user != null)
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromMinutes(30));
    
    return user;
}
```

**List Caching:**
```csharp
public async Task<List<Resource>> GetActiveResources()
{
    string cacheKey = "resources:active";
    
    var cached = await _cache.GetAsync(cacheKey);
    if (cached != null)
        return JsonSerializer.Deserialize<List<Resource>>(cached);
    
    var resources = await db.Resources
        .Where(r => r.IsActive)
        .ToListAsync();
    
    await _cache.SetAsync(cacheKey, resources, TimeSpan.FromMinutes(60));
    
    return resources;
}
```

### Cache Invalidation

```csharp
public async Task CreateResource(Resource resource)
{
    await db.Resources.AddAsync(resource);
    await db.SaveChangesAsync();
    
    // Invalidate relevant caches
    await _cache.RemoveAsync("resources:active");
    await _cache.RemoveAsync($"resources:user:{resource.CreatedBy}");
}
```

## Async/Await Best Practices

**Use async throughout:**

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<ResourceDto>> GetResource(Guid id)
{
    var resource = await db.Resources
        .AsNoTracking()
        .FirstOrDefaultAsync(r => r.Id == id);
    
    if (resource == null)
        return NotFound();
    
    return Ok(resource);
}
```

**Parallel Operations:**
```csharp
public async Task<(List<User> users, List<Resource> resources)> GetData()
{
    var usersTask = db.Users.ToListAsync();
    var resourcesTask = db.Resources.ToListAsync();
    
    await Task.WhenAll(usersTask, resourcesTask);
    
    return (await usersTask, await resourcesTask);
}
```

## Entity Framework Optimization

### Projection Instead of Entities

```csharp
// ❌ SLOW - Loads full entities
var resources = db.Resources.ToList();

// ✅ FAST - Projects only needed fields
var resources = db.Resources
    .Select(r => new {
        r.Id,
        r.Name,
        r.CreatedAt
    })
    .ToList();
```

### No Tracking for Read-Only

```csharp
public async Task<List<Resource>> SearchResources(string query)
{
    return await db.Resources
        .AsNoTracking()  // Disable change tracking
        .Where(r => r.Name.Contains(query))
        .ToListAsync();
}
```

### Batch Updates

```csharp
public async Task UpdateManyResources(List<Guid> ids, string newStatus)
{
    await db.Resources
        .Where(r => ids.Contains(r.Id))
        .ExecuteUpdateAsync(setters =>
            setters.SetProperty(r => r.Status, newStatus)
                   .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));
}
```

## API Response Optimization

### Response Compression

```csharp
services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

app.UseResponseCompression();
```

### Field Selection (Sparse Fieldsets)

```
GET /api/resources?fields=id,name,createdAt
```

**Implementation:**
```csharp
public async Task<ActionResult> GetResources([FromQuery] string fields = null)
{
    var query = db.Resources.AsQueryable();
    
    if (!string.IsNullOrEmpty(fields))
    {
        // Dynamic projection based on requested fields
        var requestedFields = fields.Split(',').Select(f => f.Trim()).ToList();
        // Implement projection logic
    }
    
    return Ok(query);
}
```

### ETags for Caching

```csharp
public async Task<ActionResult<Resource>> GetResource(Guid id)
{
    var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == id);
    
    if (resource == null)
        return NotFound();
    
    var etag = $"\"{resource.UpdatedAt.Ticks}\"";
    
    if (Request.Headers.IfNoneMatch == etag)
        return StatusCode(304); // Not Modified
    
    Response.Headers.ETag = etag;
    return Ok(resource);
}
```

## Load Testing

### Artillery Load Test

```yaml
config:
  target: 'https://api.smarthub.example.com'
  phases:
    - duration: 60
      arrivalRate: 10
      name: "Warm up"
    - duration: 300
      arrivalRate: 50
      name: "Ramp up load"
    - duration: 60
      arrivalRate: 100
      name: "Spike"

scenarios:
  - name: "User Flow"
    flow:
      - post:
          url: "/api/auth/login"
          json:
            email: "user@example.com"
            password: "password"
      - get:
          url: "/api/resources"
      - get:
          url: "/api/resources/{{ resourceId }}"
```

**Run Test:**
```bash
artillery run load-test.yml
```

### Metrics to Monitor

- Response time (p50, p95, p99)
- Throughput (requests/sec)
- Error rate
- CPU usage
- Memory usage
- Database connections

## Monitoring & Profiling

### Application Insights

```csharp
services.AddApplicationInsightsTelemetry();

public class ResourceService
{
    private readonly ILogger<ResourceService> _logger;
    
    public async Task<Resource> GetResource(Guid id)
    {
        using (_logger.BeginScope("GetResource {ResourceId}", id))
        {
            var stopwatch = Stopwatch.StartNew();
            var resource = await db.Resources.FirstOrDefaultAsync(r => r.Id == id);
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 100)
                _logger.LogWarning("Slow query: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
            
            return resource;
        }
    }
}
```

### Database Query Logging

```csharp
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Debug);

// Enable query logging in development
if (env.IsDevelopment())
{
    optionsBuilder.EnableSensitiveDataLogging();
}
```

### Slow Query Analysis

```sql
-- PostgreSQL slow query log
SET log_min_duration_statement = 1000; -- 1 second

-- View slow queries
SELECT * FROM pg_stat_statements
ORDER BY mean_exec_time DESC
LIMIT 10;
```

## Memory Optimization

### Streaming Large Responses

```csharp
[HttpGet("export")]
public async Task ExportLargeDataset()
{
    Response.ContentType = "application/octet-stream";
    Response.Headers.Add("Content-Disposition", "attachment; filename=data.csv");
    
    using (var writer = new StreamWriter(Response.Body))
    {
        await writer.WriteLineAsync("Id,Name,CreatedAt");
        
        var resources = db.Resources.AsAsyncEnumerable();
        await foreach (var resource in resources)
        {
            await writer.WriteLineAsync($"{resource.Id},{resource.Name},{resource.CreatedAt}");
        }
    }
}
```

### Object Pooling

```csharp
private static readonly ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;

public void ProcessData()
{
    byte[] buffer = _bufferPool.Rent(1024);
    try
    {
        // Use buffer
    }
    finally
    {
        _bufferPool.Return(buffer);
    }
}
```

## Performance Checklist

- [ ] Database indexes created for common queries
- [ ] Connection pooling configured
- [ ] Pagination implemented (max 100 items/page)
- [ ] Caching strategy in place (Redis)
- [ ] Async/await used throughout
- [ ] EF Core AsNoTracking for read-only queries
- [ ] Projections used instead of full entities
- [ ] Response compression enabled
- [ ] CDN configured for static files
- [ ] Load tests performed
- [ ] Monitoring enabled (Application Insights)
- [ ] Slow queries identified and optimized
- [ ] Memory leaks checked
- [ ] Database query optimization completed

## Performance Benchmarks

| Operation | Target | Actual |
|-----------|--------|--------|
| List resources (100 items) | < 100ms | 45ms |
| Get single resource | < 50ms | 25ms |
| Create resource | < 200ms | 120ms |
| Update resource | < 200ms | 110ms |
| Search (1000+ items) | < 500ms | 250ms |
| Bulk operation (1000 items) | < 5s | 3.2s |

---

Last updated: 2024-01-17
