# SmartHub

SmartHub is a modular .NET 8 web API following Clean Architecture principles. This repository contains API, Application, Domain, and Infrastructure layers.

## Development notes
- Secrets: Replace or set `Jwt:Key` using environment variables in production. A sample configuration is available at `SmartHub.Api/appsettings.example.json`.
- To run the API locally:

```bash
dotnet build
dotnet ef database update --project SmartHub.Infrastructure --startup-project SmartHub.Api
dotnet run --project SmartHub.Api
```

## Testing
- Run unit tests:
```bash
dotnet test
```

## Environment variables for secrets (recommended)
Set the following environment variables instead of storing them in code or appsettings.json:
- JWT_KEY: Your symmetric key for HS256 signing (at least 32 characters)
- JWT_ISSUER: Token issuer (e.g., SmartHub)
- JWT_AUDIENCE: Token audience (e.g., SmartHubClient)
- ADMIN_EMAIL: Email for an admin seed user (optional)
- ADMIN_PASSWORD: Admin password used for seeding (optional)

Example (PowerShell):
```powershell
$env:JWT_KEY = "01234567890123456789012345678901"
$env:JWT_ISSUER = "SmartHub"
$env:JWT_AUDIENCE = "SmartHubClient"
$env:ADMIN_EMAIL = "admin@example.com"
$env:ADMIN_PASSWORD = "ChangeMe!123"
dotnet run --project SmartHub.Api
```

Note: The repository no longer contains `appsettings.json` to reduce the risk of accidentally committing secrets; use `appsettings.example.json` as a template.
