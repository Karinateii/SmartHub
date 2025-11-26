# SmartHub Architecture

## Overview

SmartHub implements **Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture), which emphasizes separation of concerns and dependency inversion to create a maintainable, testable, and flexible codebase.

## Architecture Diagram

```
┌──────────────────────────────────────────────────────────────────┐
│                        PRESENTATION LAYER                        │
│                        (SmartHub.Api)                            │
│                                                                  │
│  ┌────────────┐  ┌──────────────┐  ┌─────────────────────────┐ │
│  │Controllers │  │  Middleware  │  │  Configurations         │ │
│  │  - Auth    │  │  - Exception │  │  - JWT                  │ │
│  │  - Users   │  │  - Logging   │  │  - Database             │ │
│  │  - Orders  │  │              │  │  - Swagger              │ │
│  │  - Products│  │              │  │  - Rate Limiting        │ │
│  └────────────┘  └──────────────┘  └─────────────────────────┘ │
│                                                                  │
└────────────────────────────┬─────────────────────────────────────┘
                             │ Uses
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                         │
│                     (SmartHub.Application)                       │
│                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌───────────────────────┐  │
│  │   DTOs      │  │ Interfaces  │  │     Validators        │  │
│  │ - Auth      │  │ IAuthService│  │  - LoginValidator     │  │
│  │ - Users     │  │ IUserService│  │  - RegisterValidator  │  │
│  │ - Orders    │  │ IRepository │  │  - FluentValidation   │  │
│  │ - Products  │  │             │  │                       │  │
│  └─────────────┘  └─────────────┘  └───────────────────────┘  │
│                                                                  │
└────────────────────────────┬─────────────────────────────────────┘
                             │ Depends on
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                        │
│                    (SmartHub.Infrastructure)                     │
│                                                                  │
│  ┌──────────────┐  ┌───────────────┐  ┌────────────────────┐  │
│  │  Persistence │  │   Services    │  │   Configurations   │  │
│  │ DbContext    │  │ AuthService   │  │ UserConfiguration  │  │
│  │ Migrations   │  │ (Implements   │  │ (EF Fluent API)    │  │
│  │              │  │  IAuthService)│  │                    │  │
│  └──────────────┘  └───────────────┘  └────────────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │              External Dependencies                        │  │
│  │  - Entity Framework Core                                 │  │
│  │  - SQL Server Provider                                   │  │
│  │  - BCrypt.NET                                            │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                  │
└────────────────────────────┬─────────────────────────────────────┘
                             │ Implements
                             ▼
┌──────────────────────────────────────────────────────────────────┐
│                          DOMAIN LAYER                            │
│                        (SmartHub.Domain)                         │
│                                                                  │
│  ┌─────────────┐  ┌──────────┐  ┌────────────────────────────┐ │
│  │  Entities   │  │  Enums   │  │    Common/Base Classes     │ │
│  │   - User    │  │  - Role  │  │  - BaseEntity              │ │
│  │   - Order   │  │          │  │  - AuditableEntity         │ │
│  │   - Product │  │          │  │                            │ │
│  └─────────────┘  └──────────┘  └────────────────────────────┘ │
│                                                                  │
│         ⚠️  NO EXTERNAL DEPENDENCIES - PURE BUSINESS LOGIC       │
└──────────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (Core)
**Location:** `SmartHub.Domain`

**Purpose:** Contains the business logic and domain entities. This is the heart of the application.

**Characteristics:**
- ✅ **Zero external dependencies** (no NuGet packages except maybe basic utilities)
- ✅ Contains business entities (`User`, `Order`, `Product`)
- ✅ Contains enums (`Role`)
- ✅ Contains value objects
- ✅ Contains domain events (if needed)
- ✅ Contains business rules and invariants

**Files:**
```
SmartHub.Domain/
├── Entities/
│   └── User.cs              # Core user entity with business properties
├── Enums/
│   └── Role.cs              # User roles (Admin, User)
├── Common/
│   ├── BaseEntity.cs        # Base class with Id
│   └── AuditableEntity.cs   # Adds CreatedAt, UpdatedAt timestamps
└── ValueObjects/            # Value objects (future: Address, Money, etc.)
```

**Example:**
```csharp
public class User : AuditableEntity
{
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string Email { get; set; }
  public string PasswordHash { get; set; }
  public Role Role { get; set; }
  // Business logic here
}
```

---

### 2. Application Layer (Use Cases)
**Location:** `SmartHub.Application`

**Purpose:** Orchestrates the flow of data between the Domain and Infrastructure layers. Defines use cases and DTOs.

**Characteristics:**
- ✅ Depends **only** on Domain layer
- ✅ Defines interfaces (contracts) for services and repositories
- ✅ Contains DTOs for data transfer
- ✅ Contains validators (FluentValidation)
- ✅ No direct database or external service access

**Files:**
```
SmartHub.Application/
├── DTOs/
│   ├── Auth/
│   │   ├── LoginRequest.cs
│   │   ├── RegisterRequest.cs
│   │   └── AuthResponse.cs
│   ├── Users/
│   └── Orders/
├── Interfaces/
│   ├── Services/
│   │   ├── IAuthService.cs
│   │   └── IUserService.cs
│   └── Repositories/
│       └── IUserRepository.cs
└── Validators/
    └── Auth/
        ├── LoginRequestValidator.cs
        └── RegisterRequestValidator.cs
```

**Example:**
```csharp
public interface IAuthService
{
  Task<AuthResponse> RegisterAsync(RegisterRequest request);
  Task<AuthResponse> LoginAsync(LoginRequest request);
  Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
```

---

### 3. Infrastructure Layer (Data Access & External Services)
**Location:** `SmartHub.Infrastructure`

**Purpose:** Implements interfaces defined in the Application layer. Handles data persistence, external APIs, file systems, etc.

**Characteristics:**
- ✅ Depends on Application and Domain layers
- ✅ Implements repository and service interfaces
- ✅ Contains EF Core `DbContext`
- ✅ Contains entity configurations (Fluent API)
- ✅ Handles database migrations
- ✅ External service integrations (email, SMS, etc.)

**Files:**
```
SmartHub.Infrastructure/
├── Persistence/
│   ├── SmartHubDbContext.cs      # EF Core DbContext
│   └── Migrations.cs
├── Configurations/
│   └── UserConfiguration.cs      # EF Fluent API configuration
├── Services/
│   └── AuthService.cs            # Implements IAuthService
├── Repositories/
│   └── UserRepository.cs         # Implements IUserRepository
└── Migrations/
    └── 20251124203728_InitialCreate.cs
```

**Example:**
```csharp
public class AuthService : IAuthService
{
  private readonly SmartHubDbContext _dbContext;
  
  public AuthService(SmartHubDbContext dbContext)
  {
    _dbContext = dbContext;
  }
  
  public async Task<AuthResponse> LoginAsync(LoginRequest request)
  {
    var user = await _dbContext.Users
      .FirstOrDefaultAsync(u => u.Email == request.Email);
    // Implementation...
  }
}
```

---

### 4. Presentation Layer (API)
**Location:** `SmartHub.Api`

**Purpose:** Exposes the application functionality via HTTP endpoints. Handles HTTP concerns.

**Characteristics:**
- ✅ Depends on Application and Infrastructure layers
- ✅ Contains controllers
- ✅ Contains middleware
- ✅ Contains startup configuration
- ✅ Handles authentication/authorization
- ✅ Manages API documentation (Swagger)

**Files:**
```
SmartHub.Api/
├── Controllers/
│   ├── Auth/
│   │   └── AuthController.cs     # /api/auth endpoints
│   ├── Users/
│   │   └── UsersController.cs
│   └── Orders/
├── Middlewares/
│   └── ExceptionHandlingMiddleware.cs
├── Configurations/
│   └── JwtConfiguration.cs
└── Program.cs                    # Application startup
```

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;
  
  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }
  
  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginRequest request)
  {
    var response = await _authService.LoginAsync(request);
    return Ok(response);
  }
}
```

---

## Dependency Flow

```
┌─────────────────────────────────────────────────────┐
│                  Dependency Rule                    │
│                                                     │
│  Inner layers DO NOT depend on outer layers        │
│  Dependencies point INWARD towards Domain          │
└─────────────────────────────────────────────────────┘

        API Layer (Controllers)
              ⬇ depends on
        Application Layer (Interfaces)
              ⬆ implemented by
        Infrastructure Layer (Services)
              ⬇ depends on
        Domain Layer (Entities)
```

### Dependency Inversion Principle

Instead of:
```
Controller → AuthService → Database
```

We use:
```
Controller → IAuthService ← AuthService → Database
              (Interface)   (Implementation)
```

**Benefits:**
- Easy to swap implementations
- Testable (mock interfaces)
- No tight coupling

---

## Request/Response Flow

### Example: User Login

```
1. HTTP POST /api/auth/login
         ⬇
2. AuthController
   - Receives LoginRequest DTO
   - Validates request (FluentValidation)
         ⬇
3. IAuthService (Application Layer)
   - Interface defines contract
         ⬇
4. AuthService (Infrastructure Layer)
   - Queries database via DbContext
   - Verifies password with BCrypt
   - Generates JWT token
         ⬇
5. AuthResponse DTO
   - Returns token, user info
         ⬇
6. HTTP 200 OK with JSON response
```

---

## Data Flow Diagram

```
┌─────────────┐
│   Client    │
│  (Browser)  │
└──────┬──────┘
       │ HTTP Request
       ▼
┌──────────────────────────────────┐
│     Middleware Pipeline          │
│  1. Exception Handling           │
│  2. Rate Limiting                │
│  3. Authentication (JWT)         │
│  4. Request Logging (Serilog)    │
└──────────────┬───────────────────┘
               │
               ▼
┌───────────────────────────────────┐
│        Controller                 │
│  - Receives DTO                   │
│  - Validates (FluentValidation)   │
└──────────────┬────────────────────┘
               │
               ▼
┌───────────────────────────────────┐
│      Application Service          │
│  - Business logic orchestration   │
│  - Calls repositories             │
└──────────────┬────────────────────┘
               │
               ▼
┌───────────────────────────────────┐
│      Infrastructure Service       │
│  - Database queries (EF Core)     │
│  - External API calls             │
└──────────────┬────────────────────┘
               │
               ▼
┌───────────────────────────────────┐
│         Database                  │
│  - SQL Server                     │
│  - Data persistence               │
└───────────────────────────────────┘
```

---

## Benefits of This Architecture

### ✅ Testability
- **Unit tests:** Test business logic in Domain without dependencies
- **Integration tests:** Test API endpoints with in-memory database
- **Mock interfaces:** Easy to mock `IAuthService`, `IUserRepository`

### ✅ Maintainability
- **Clear boundaries:** Each layer has specific responsibilities
- **Easy to navigate:** Predictable project structure
- **Separation of concerns:** Business logic separate from infrastructure

### ✅ Flexibility
- **Swap databases:** Change from SQL Server to PostgreSQL by updating Infrastructure
- **Change authentication:** Replace JWT with OAuth2 without touching Domain
- **Add new features:** Add new entities to Domain without breaking existing code

### ✅ Scalability
- **Team collaboration:** Different teams can work on different layers
- **Microservices ready:** Domain and Application layers can be reused
- **Performance:** Infrastructure can be optimized without changing business logic

---

## Design Patterns Used

### 1. **Repository Pattern**
Abstracts data access logic behind interfaces.

```csharp
public interface IUserRepository
{
  Task<User?> GetByIdAsync(Guid id);
  Task<User?> GetByEmailAsync(string email);
  Task AddAsync(User user);
}
```

### 2. **Dependency Injection**
Services are injected via constructor.

```csharp
public class AuthService : IAuthService
{
  private readonly SmartHubDbContext _dbContext;
  private readonly IConfiguration _configuration;
  
  public AuthService(SmartHubDbContext dbContext, IConfiguration configuration)
  {
    _dbContext = dbContext;
    _configuration = configuration;
  }
}
```

### 3. **DTO Pattern**
Data Transfer Objects for API communication.

```csharp
public class LoginRequest
{
  public string Email { get; set; }
  public string Password { get; set; }
}
```

### 4. **Middleware Pattern**
Cross-cutting concerns handled by middleware.

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
```

---

## Testing Strategy

### Unit Tests (Application Layer)
```csharp
[Fact]
public async Task RegisterAsync_ValidRequest_ReturnsAuthResponse()
{
  // Arrange
  var mockDbContext = new Mock<SmartHubDbContext>();
  var authService = new AuthService(mockDbContext.Object, configuration);
  
  // Act
  var result = await authService.RegisterAsync(request);
  
  // Assert
  Assert.NotNull(result.Token);
}
```

### Integration Tests (API Layer)
```csharp
[Fact]
public async Task Login_ValidCredentials_Returns200()
{
  // Arrange
  var client = _factory.CreateClient();
  
  // Act
  var response = await client.PostAsJsonAsync("/api/auth/login", request);
  
  // Assert
  Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

---

## Future Enhancements

### Planned Architecture Improvements

1. **CQRS (Command Query Responsibility Segregation)**
   - Separate read and write operations
   - `Commands/` and `Queries/` folders in Application layer

2. **MediatR**
   - Implement mediator pattern
   - Decouple controllers from service implementations

3. **Domain Events**
   - `UserRegisteredEvent`, `OrderPlacedEvent`
   - Event handlers for cross-cutting concerns

4. **Repository Pattern (Explicit)**
   - Move from direct `DbContext` usage to repositories
   - `IUserRepository`, `IOrderRepository`

5. **Unit of Work Pattern**
   - Manage transactions across multiple repositories

---

## Conclusion

SmartHub's architecture provides a solid foundation for building scalable, maintainable, and testable applications. By following Clean Architecture principles, the codebase remains flexible and easy to evolve as requirements change.

For questions or suggestions about the architecture, please open an issue or contribute via pull request!

---

**Related Documentation:**
- [README.md](./README.md) - Project overview and setup
- [CONTRIBUTING.md](./CONTRIBUTING.md) - Contribution guidelines
- [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) - API reference
