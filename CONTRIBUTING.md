# Contributing to SmartHub

First off, thank you for considering contributing to SmartHub! 🎉

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone.

## How Can I Contribute?

### 🐛 Reporting Bugs

Before creating bug reports, please check existing issues to avoid duplicates.

**When submitting a bug report, include:**
- Clear and descriptive title
- Steps to reproduce the behavior
- Expected behavior vs. actual behavior
- Environment details (.NET version, OS, SQL Server version)
- Relevant logs from `logs/smarthub.log`

### ✨ Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues.

**Include in your suggestion:**
- Clear and descriptive title
- Detailed description of the proposed feature
- Use cases and examples
- Why this enhancement would be useful

### 🔀 Pull Requests

1. **Fork the repository** and create your branch from `main`
   ```powershell
   git checkout -b feature/amazing-feature
   ```

2. **Follow the project structure:**
   - `SmartHub.Domain`: Entities, enums, value objects (no external dependencies)
   - `SmartHub.Application`: DTOs, interfaces, validators, use cases
   - `SmartHub.Infrastructure`: EF Core, repositories, external services
   - `SmartHub.Api`: Controllers, middleware, configuration

3. **Write clean code:**
   - Follow C# coding conventions
   - Use meaningful variable and method names
   - Add XML documentation comments for public APIs
   - Keep methods focused and single-purpose

4. **Add tests:**
   - Unit tests for business logic
   - Integration tests for API endpoints
   - Ensure all existing tests pass: `dotnet test`

5. **Update documentation:**
   - Update README.md if you're changing functionality
   - Update API_DOCUMENTATION.md for new endpoints
   - Add XML comments for Swagger documentation

6. **Commit your changes:**
   ```powershell
   git add .
   git commit -m "feat: add amazing feature"
   ```
   
   **Commit message format:**
   - `feat:` - New feature
   - `fix:` - Bug fix
   - `docs:` - Documentation changes
   - `style:` - Code style changes (formatting)
   - `refactor:` - Code refactoring
   - `test:` - Adding or updating tests
   - `chore:` - Maintenance tasks

7. **Push to your fork:**
   ```powershell
   git push origin feature/amazing-feature
   ```

8. **Open a Pull Request** with a clear title and description

## Development Setup

1. **Install prerequisites:**
   - .NET 8 SDK
   - SQL Server (LocalDB or Express)
   - Git

2. **Clone and setup:**
   ```powershell
   git clone https://github.com/YOUR_USERNAME/SmartHub.git
   cd SmartHub
   ./scripts/setup-dev.ps1 -SetEnvVars
   ```

3. **Run migrations:**
   ```powershell
   dotnet ef database update --project SmartHub.Infrastructure --startup-project SmartHub.Api
   ```

4. **Run the application:**
   ```powershell
   dotnet run --project SmartHub.Api
   ```

5. **Run tests:**
   ```powershell
   dotnet test
   ```

## Coding Standards

### C# Style Guide

- **Naming:**
  - PascalCase for classes, methods, properties
  - camelCase for parameters, local variables
  - _camelCase for private fields
  - UPPER_CASE for constants

- **Formatting:**
  - Use 2 spaces for indentation
  - Place opening braces on new line
  - One statement per line
  - Use explicit types instead of `var` when type isn't obvious

- **Code Organization:**
  - Keep classes focused and cohesive
  - Follow SOLID principles
  - Use dependency injection
  - Avoid tight coupling

### Example

```csharp
namespace SmartHub.Application.Services
{
  public class UserService : IUserService
  {
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    public UserService(
      IUserRepository userRepository,
      ILogger<UserService> logger)
    {
      _userRepository = userRepository;
      _logger = logger;
    }

    /// <summary>
    /// Gets a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
      _logger.LogInformation("Retrieving user with ID: {UserId}", userId);
      return await _userRepository.GetByIdAsync(userId);
    }
  }
}
```

## Architecture Guidelines

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│  Presentation Layer (SmartHub.Api)      │  ← Controllers, Middleware
├─────────────────────────────────────────┤
│  Application Layer (Application)        │  ← Use Cases, DTOs, Interfaces
├─────────────────────────────────────────┤
│  Infrastructure Layer (Infrastructure)  │  ← DbContext, Repositories
├─────────────────────────────────────────┤
│  Domain Layer (Domain)                  │  ← Entities, Business Logic
└─────────────────────────────────────────┘
```

**Dependency Rules:**
- Inner layers should not depend on outer layers
- Domain layer has no external dependencies
- Application layer depends only on Domain
- Infrastructure and API depend on Application

## Testing Guidelines

### Unit Tests
- Test business logic in isolation
- Mock external dependencies
- Use meaningful test names: `MethodName_Scenario_ExpectedBehavior`
- Arrange-Act-Assert pattern

```csharp
[Fact]
public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
{
  // Arrange
  var request = new LoginRequest { Email = "test@example.com", Password = "Test123!" };
  
  // Act
  var response = await _authService.LoginAsync(request);
  
  // Assert
  Assert.NotNull(response.Token);
  Assert.Equal(request.Email, response.Email);
}
```

### Integration Tests
- Test complete API flows
- Use in-memory database
- Test authentication and authorization
- Verify HTTP status codes and response formats

## Security Considerations

⚠️ **Never commit secrets!**

- Use environment variables for sensitive data
- Run `./scripts/install-hooks.ps1` to install pre-commit checks
- Review the Security section in README.md
- Hash passwords with BCrypt
- Validate all user input
- Use parameterized queries (EF Core does this)

## Questions?

Feel free to open an issue for discussion or reach out to the maintainers.

## Recognition

Contributors will be recognized in the project's README.md.

---

Thank you for contributing to SmartHub! 🚀
