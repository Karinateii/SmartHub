# 🚀 SmartHub

[![.NET CI](https://github.com/Karinateii/SmartHub/actions/workflows/ci.yml/badge.svg)](https://github.com/Karinateii/SmartHub/actions/workflows/ci.yml)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)

> A production-ready RESTful API built with .NET 8 following Clean Architecture principles, featuring JWT authentication, comprehensive security, and modern development practices.

## 📑 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Security](#-security)
- [Testing](#-testing)
- [Scripts](#-scripts)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### 🔐 Authentication & Authorization
- **JWT Token-based Authentication** with refresh token support
- **Role-based Access Control** (User, Admin)
- **Secure Password Hashing** with BCrypt
- **Email Verification** system ready
- **Token Refresh & Revocation** mechanisms

### 🛡️ Security
- **Rate Limiting** on sensitive endpoints (login, register, refresh)
- **Environment-based Secret Management** (no hardcoded secrets)
- **Git Pre-commit Hooks** to prevent secret commits
- **Secure Token Storage** with hashed refresh tokens
- **HTTPS Redirection** enforced

### 🏗️ Architecture & Code Quality
- **Clean Architecture** (Domain, Application, Infrastructure, API layers)
- **Entity Framework Core** with SQL Server
- **FluentValidation** for request validation
- **Structured Logging** with Serilog (Console + File)
- **Health Check Endpoints** for monitoring
- **Exception Handling Middleware** for centralized error management

### 🧪 Testing & CI/CD
- **Integration Tests** with in-memory database
- **Unit Tests** for business logic
- **GitHub Actions CI/CD** (Ubuntu & Windows runners)
- **Automated JWT Secret Validation** in CI pipeline

### 📊 API Features
- **RESTful Design** principles
- **Swagger/OpenAPI Documentation** with XML comments
- **Consistent Response Formats**
- **CORS Ready** for frontend integration

## 🛠️ Tech Stack

### Backend Framework
- **.NET 8** - Latest LTS version
- **ASP.NET Core Web API** - Modern web framework
- **Entity Framework Core 8** - ORM for database operations

### Database
- **SQL Server** - Production database
- **In-Memory Database** - Integration testing

### Security & Authentication
- **JWT Bearer Authentication** - Stateless authentication
- **BCrypt.NET** - Password hashing
- **Microsoft Identity** - Claims-based security

### Validation & Logging
- **FluentValidation** - Request validation
- **Serilog** - Structured logging

### Testing
- **xUnit** - Testing framework
- **WebApplicationFactory** - Integration testing

### DevOps
- **GitHub Actions** - CI/CD pipeline
- **PowerShell Scripts** - Development automation

## 🏛️ Architecture

SmartHub follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         SmartHub.Api (Presentation)     │
│  Controllers, Middleware, Configuration │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│      SmartHub.Application (Use Cases)   │
│     DTOs, Interfaces, Validators        │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│   SmartHub.Infrastructure (External)    │
│  DbContext, Repositories, Services      │
└──────────────────┬──────────────────────┘
                   │
┌──────────────────▼──────────────────────┐
│      SmartHub.Domain (Core Business)    │
│    Entities, Enums, Value Objects       │
└─────────────────────────────────────────┘
```

**Key Benefits:**
- ✅ **Testability**: Business logic independent of external dependencies
- ✅ **Maintainability**: Clear boundaries between layers
- ✅ **Flexibility**: Easy to swap implementations (e.g., change database)
- ✅ **Scalability**: Clean separation enables team collaboration

> 📖 See [ARCHITECTURE.md](./ARCHITECTURE.md) for detailed architecture documentation

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB, Express, or Developer Edition)
- [Git](https://git-scm.com/)

### Quick Start

1️⃣ **Clone the repository**
```powershell
git clone https://github.com/Karinateii/SmartHub.git
cd SmartHub
```

2️⃣ **Run the setup script** (recommended)
```powershell
cd scripts
./setup-dev.ps1 -SetEnvVars
```

This script will:
- Create `appsettings.json` from the example template
- Generate secure JWT secrets
- Set environment variables for your session
- Validate your setup

3️⃣ **Apply database migrations**
```powershell
dotnet ef database update --project SmartHub.Infrastructure --startup-project SmartHub.Api
```

4️⃣ **Run the application**
```powershell
dotnet run --project SmartHub.Api
```

5️⃣ **Access the API**
- API: `https://localhost:7XXX` (check console output for exact port)
- Swagger UI: `https://localhost:7XXX/swagger`
- Health Check: `https://localhost:7XXX/health`

### Manual Setup (Alternative)

If you prefer manual configuration:

**Set Environment Variables** (PowerShell)
```powershell
$env:JWT_KEY = "your-super-secret-key-at-least-32-characters-long"
$env:JWT_ISSUER = "SmartHub"
$env:JWT_AUDIENCE = "SmartHubClient"
$env:ADMIN_EMAIL = "admin@smarthub.com"
$env:ADMIN_PASSWORD = "SecurePass123!"
```

**Update Connection String** in `SmartHub.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "SmartHubDatabase": "Server=(localdb)\\mssqllocaldb;Database=SmartHubDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

## 📚 API Documentation

### Authentication Endpoints

| Method | Endpoint | Description | Rate Limit |
|--------|----------|-------------|------------|
| POST | `/api/auth/register` | Register new user | 3 req/hour |
| POST | `/api/auth/login` | Login with credentials | 5 req/min |
| POST | `/api/auth/refresh` | Refresh access token | 30 req/min |
| POST | `/api/auth/logout` | Revoke refresh token | - |

### Example: Register User

**Request:**
```json
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "CfDJ8P+5K...",
  "expiresAt": "2025-11-26T15:30:00Z",
  "refreshTokenExpiry": "2025-12-03T14:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "User"
}
```

> 📖 See [API_DOCUMENTATION.md](./API_DOCUMENTATION.md) for comprehensive API reference

## 🔒 Security

### Secret Management

**Never commit secrets!** SmartHub uses environment variables for all sensitive configuration.

#### Required Environment Variables

```powershell
# JWT Configuration (Required)
$env:JWT_KEY = "minimum-32-characters-for-hs256-security"
$env:JWT_ISSUER = "SmartHub"
$env:JWT_AUDIENCE = "SmartHubClient"

# Admin Seeding (Optional)
$env:ADMIN_EMAIL = "admin@smarthub.com"
$env:ADMIN_PASSWORD = "AdminSecure123!"
```

#### Pre-commit Hooks

Install Git hooks to prevent accidental secret commits:

```powershell
cd scripts
./install-hooks.ps1
```

This validates that sensitive files aren't committed.

### Secret Rotation

**Local/Development:**
```powershell
$env:JWT_KEY = "new-secure-key-at-least-32-chars"
# Restart application
```

**Production/CI:**
1. Update GitHub repository secrets: Settings → Secrets and variables → Actions
2. Update `JWT_KEY` with a new secure value
3. Redeploy application
4. Consider token grace period or force user re-login

> 🛡️ The CI pipeline validates JWT_KEY presence and length (≥32 chars)

## 🧪 Testing

### Run All Tests

```powershell
dotnet test
```

### Run Specific Test Projects

```powershell
# Unit Tests
dotnet test SmartHub.Tests/SmartHub.Tests.csproj --filter Category=Unit

# Integration Tests
dotnet test SmartHub.Tests/SmartHub.Tests.csproj --filter Category=Integration
```

### Test Coverage

- ✅ Authentication flow (register → login → refresh → logout)
- ✅ Token generation and validation
- ✅ Refresh token rotation
- ✅ FluentValidation rules
- ✅ Integration tests with in-memory database

## 📜 Scripts

SmartHub includes automation scripts in the `scripts/` directory:

| Script | Description |
|--------|-------------|
| `setup-dev.ps1` | Initialize development environment |
| `install-hooks.ps1` | Install Git pre-commit hooks |
| `pre-commit.ps1` | Validate commits for secrets |
| `smoke-test.ps1` | Quick API health validation |

### Usage Examples

```powershell
# Setup with environment variables
./scripts/setup-dev.ps1 -SetEnvVars

# Install security hooks
./scripts/install-hooks.ps1

# Run smoke tests after deployment
./scripts/smoke-test.ps1
```

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](./CONTRIBUTING.md) for details on:
- Code of conduct
- Development workflow
- Pull request process
- Coding standards

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](./LICENSE) file for details.

---

<div align="center">

**Built with ❤️ using .NET 8 and Clean Architecture**

[Report Bug](https://github.com/Karinateii/SmartHub/issues) • [Request Feature](https://github.com/Karinateii/SmartHub/issues)

</div>
